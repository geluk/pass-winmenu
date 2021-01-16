using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Core;
using PassWinmenu.Actions;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenu.Hotkeys;
using PassWinmenu.PasswordManagement;
using PassWinmenu.UpdateChecking;
using PassWinmenu.Utilities;
using PassWinmenu.WinApi;
using PassWinmenu.Windows;

using YamlDotNet.Core;
using IContainer = Autofac.IContainer;

namespace PassWinmenu
{
	internal sealed class Program : IDisposable
	{
		public static string Version => EmbeddedResources.Version;

		public const string LastConfigVersion = "1.7";
		public const string EncryptedFileExtension = ".gpg";
		public const string PlaintextFileExtension = ".txt";
		public const string ConfigFileName = "pass-winmenu.yaml";

		private UpdateChecker updateChecker;
		private Option<RemoteUpdateChecker> remoteUpdateChecker;
		private Notifications notificationService;

		private IContainer container;

		public void Start()
		{
			try
			{
				Initialise();
				RunInitialCheck();
			}
			catch (Exception e)
			{
				Log.EnableFileLogging();
				Log.Send("Could not start pass-winmenu: An exception occurred.", LogLevel.Error);
				Log.ReportException(e);

				if (e is DependencyResolutionException de && de.InnerException != null)
				{
					e = de.InnerException;
				}
				string errorMessage = $"pass-winmenu failed to start ({e.GetType().Name}: {e.Message})";
				if (notificationService == null)
				{
					// We have no notification service yet. Instantiating one is risky,
					// so we'll make do with a call to MessageBox.Show() instead.
					MessageBox.Show(errorMessage, "An error occurred.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				else
				{
					notificationService.ShowErrorWindow(errorMessage);
					notificationService.Dispose();
				}
				App.Exit();
			}
		}

		/// <summary>
		/// Loads all required resources.
		/// </summary>
		private void Initialise()
		{
			// Load compiled-in resources.
			EmbeddedResources.Load();

			Log.Send("------------------------------");
			Log.Send($"Starting pass-winmenu {Version}");
			Log.Send("------------------------------");

			Log.Send($"Enabled security protocols: {ServicePointManager.SecurityProtocol}");

			// Create the notification service first, so it's available if initialisation fails.
			notificationService = Notifications.Create();

			// Initialise the DI Container builder.
			var builder = new ContainerBuilder();

			builder.Register(_ => notificationService)
				.AsImplementedInterfaces()
				.SingleInstance();

			// Now load the configuration options that we'll need 
			// to continue initialising the rest of the applications.
			var runtimeConfig = RuntimeConfiguration.Parse(Environment.GetCommandLineArgs());
			LoadConfigFile(runtimeConfig);

			builder.Register(_ => ConfigManager.Config).AsSelf();
			builder.Register(_ => ConfigManager.Config.Gpg).AsSelf();
			builder.Register(_ => ConfigManager.Config.Git).AsSelf();
			builder.Register(_ => ConfigManager.Config.PasswordStore).AsSelf();
			builder.Register(_ => ConfigManager.Config.Application.UpdateChecking).AsSelf();
			builder.Register(_ => ConfigManager.Config.PasswordStore.UsernameDetection).AsSelf();

#if DEBUG
			Log.EnableFileLogging();
#else
			if (ConfigManager.Config.CreateLogFile)
			{
				Log.EnableFileLogging();
			}
#endif

			// Register actions and hotkeys
			builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(ActionDispatcher)))
				.InNamespaceOf<ActionDispatcher>()
				.Except<ActionDispatcher>()
				.AsImplementedInterfaces();
			builder.RegisterType<HotkeyManager>()
				.AsSelf();

			builder.RegisterType<ActionDispatcher>()
				.WithParameter(
					(p, ctx) => p.ParameterType == typeof(Dictionary<HotkeyAction, IAction>),
					(info, context) => context.Resolve<IEnumerable<IAction>>().ToDictionary(a => a.ActionType));

			// Register environment wrappers
			builder.RegisterTypes(
				typeof(FileSystem),
				typeof(SystemEnvironment),
				typeof(Processes),
				typeof(ExecutablePathResolver)
			).AsImplementedInterfaces();

			// Register GPG types
			builder.RegisterTypes(
					typeof(GpgInstallationFinder),
					typeof(GpgHomedirResolver),
					typeof(GpgAgentConfigReader),
					typeof(GpgAgentConfigUpdater),
					typeof(GpgTransport),
					typeof(GpgAgent),
					typeof(GpgResultVerifier),
					typeof(GPG)
				).AsImplementedInterfaces()
				.AsSelf();

			// Register GPG installation
			// Single instance, as there is no need to look for the same GPG installation multiple times.
			builder.Register(context => context.Resolve<GpgInstallationFinder>().FindGpgInstallation(ConfigManager.Config.Gpg.GpgPath))
				.SingleInstance();

			builder.RegisterType<DialogCreator>()
				.AsSelf();

			// Register the internal password manager
			builder.Register(context => context.Resolve<IFileSystem>().DirectoryInfo.FromDirectoryName(context.Resolve<PasswordStoreConfig>().Location))
				.Named("PasswordStore", typeof(IDirectoryInfo));

			builder.RegisterType<GpgRecipientFinder>().WithParameter(
					(parameter, context) => true,
					(parameter, context) => context.ResolveNamed<IDirectoryInfo>("PasswordStore"))
				.AsImplementedInterfaces();

			builder.RegisterType<PasswordManager>().WithParameter(
					(parameter, context) => parameter.ParameterType == typeof(IDirectoryInfo),
					(parameter, context) => context.ResolveNamed<IDirectoryInfo>("PasswordStore"))
				.AsImplementedInterfaces()
				.AsSelf();

			builder.RegisterType<PasswordFileParser>().AsSelf();

			// Create the Git wrapper, if enabled.
			// This needs to be a single instance to stop startup warnings being displayed multiple times.
			builder.RegisterType<GitSyncStrategies>().AsSelf();
			builder.Register(CreateSyncService)
				.AsSelf()
				.SingleInstance();

			builder.Register(context => UpdateCheckerFactory.CreateUpdateChecker(context.Resolve<UpdateCheckingConfig>(), context.Resolve<INotificationService>()));
			builder.RegisterType<RemoteUpdateCheckerFactory>().AsSelf();
			builder.Register(context => context.Resolve<RemoteUpdateCheckerFactory>().Build()).AsSelf();

			// Build the container
			container = builder.Build();

			var gpgConfig = container.Resolve<GpgConfig>();
			if (gpgConfig.GpgAgent.Config.AllowConfigManagement)
			{
				container.Resolve<GpgAgentConfigUpdater>().UpdateAgentConfig(gpgConfig.GpgAgent.Config.Keys);
			}

			var actionDispatcher = container.Resolve<ActionDispatcher>();

			notificationService.AddMenuActions(actionDispatcher);

			// Assign our hotkeys.
			AssignHotkeys(actionDispatcher);

			// Start checking for updates
			updateChecker = container.Resolve<UpdateChecker>();
			remoteUpdateChecker = container.Resolve<Option<RemoteUpdateChecker>>();

			if (container.Resolve<UpdateCheckingConfig>().CheckForUpdates)
			{
				updateChecker.Start();
			}
			remoteUpdateChecker.Apply(c => c.Start());
		}

		private static Option<ISyncService> CreateSyncService(IComponentContext context)
		{
			var config = context.Resolve<GitConfig>();
			var signService = context.Resolve<ISignService>();
			var passwordStore = context.ResolveNamed<IDirectoryInfo>("PasswordStore");
			var notificationService = context.Resolve<INotificationService>();
			var strategies = context.Resolve<GitSyncStrategies>();

			var factory = new SyncServiceFactory(config, passwordStore.FullName, signService, strategies);

			var syncService = factory.BuildSyncService();
			switch (factory.Status)
			{
				case SyncServiceStatus.GitLibraryNotFound:
					notificationService.ShowErrorWindow("The git2 DLL could not be found. Git support will be disabled.");
					break;
				case SyncServiceStatus.GitRepositoryNotFound:
					notificationService.ShowErrorWindow($"Failed to open the password store Git repository ({factory.Exception.GetType().Name}: {factory.Exception.Message}). Git support will be disabled.");
					break;
			}

			return Option.FromNullable(syncService);
		}

		/// <summary>
		/// Checks if all components are configured correctly.
		/// </summary>
		private void RunInitialCheck()
		{
			var gpg = container.Resolve<GPG>();

			if (!Directory.Exists(ConfigManager.Config.PasswordStore.Location))
			{
				notificationService.ShowErrorWindow($"Could not find the password store at {Path.GetFullPath(ConfigManager.Config.PasswordStore.Location)}. Please make sure it exists.");
				App.Exit();
				return;
			}
			try
			{
				Log.Send("Using GPG version " + gpg.GetVersion());
			}
			catch (System.ComponentModel.Win32Exception)
			{
				notificationService.ShowErrorWindow("Could not find GPG. Make sure your gpg-path is set correctly.");
				App.Exit();
				return;
			}
			catch (Exception e)
			{
				notificationService.ShowErrorWindow($"Failed to initialise GPG. {e.GetType().Name}: {e.Message}");
				App.Exit();
				return;
			}
			if (ConfigManager.Config.Gpg.GpgAgent.Preload)
			{
				Task.Run(() =>
				{
					try
					{
						gpg.StartAgent();
					}
					catch (GpgError err)
					{
						notificationService.ShowErrorWindow(err.Message);
					}
					// Ignore other exceptions. If it turns out GPG is misconfigured,
					// these errors will surface upon decryption/encryption.
					// The reason we catch GpgErrors here is so we can notify the user
					// if we don't detect any decryption keys.
				});
			}
		}

		private void LoadConfigFile(RuntimeConfiguration runtimeConfig)
		{
			LoadResult result;

			string configPath;
			if (!string.IsNullOrEmpty(runtimeConfig.ConfigFileLocation))
			{
				configPath = Path.GetFullPath(runtimeConfig.ConfigFileLocation);
			}
			else
			{
				var executableDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
				configPath = Path.Combine(executableDirectory!, ConfigFileName);
			}
			try
			{

				result = ConfigManager.Load(configPath);
			}
			catch (Exception e) when (e.InnerException != null)
			{
				if (e is YamlException)
				{
					notificationService.ShowErrorWindow(
						$"The configuration file could not be loaded: {e.Message}\n\n" +
						$"{e.InnerException.GetType().Name}: {e.InnerException.Message}",
						"Unable to load configuration file.");
				}
				else
				{
					notificationService.ShowErrorWindow(
						$"The configuration file could not be loaded. An unhandled exception occurred.\n" +
						$"{e.InnerException.GetType().Name}: {e.InnerException.Message}",
						"Unable to load configuration file.");
				}

				App.Exit();
				return;
			}
			catch (SemanticErrorException e)
			{
				notificationService.ShowErrorWindow(
					$"The configuration file could not be loaded, a YAML error was encountered.\n" +
					$"{e.GetType().Name}: {e.Message}\n\n" +
					$"File location: {configPath}",
					"Unable to load configuration file.");
				App.Exit();
				return;
			}
			catch (YamlException e)
			{
				notificationService.ShowErrorWindow(
					$"The configuration file could not be loaded. An unhandled exception occurred.\n{e.GetType().Name}: {e.Message}",
					"Unable to load configuration file.");
				App.Exit();
				return;
			}

			switch (result)
			{
				case LoadResult.FileCreationFailure:
					notificationService.Raise("A default configuration file was generated, but could not be saved.\nPass-winmenu will fall back to its default settings.", Severity.Error);
					break;
				case LoadResult.NewFileCreated:
					var open = MessageBox.Show("A new configuration file has been generated. Please modify it according to your preferences and restart the application.\n\n" + 
					                                          "Would you like to open it now?", "New configuration file created", MessageBoxButton.YesNo);
					if (open == MessageBoxResult.Yes) Process.Start(ConfigFileName);
					App.Exit();
					return;
				case LoadResult.NeedsUpgrade:
					var backedUpFile = ConfigManager.Backup(ConfigFileName);
					var openBoth = MessageBox.Show("The current configuration file is out of date. A new configuration file has been created, and the old file has been backed up.\n" +
					                                              "Please edit the new configuration file according to your preferences and restart the application.\n\n" +
					                                              "Would you like to open both files now?", "Configuration file out of date", MessageBoxButton.YesNo);
					if (openBoth == MessageBoxResult.Yes)
					{
						Process.Start(ConfigFileName);
						Process.Start(backedUpFile);
					}
					App.Exit();
					return;
			}
			if (ConfigManager.Config.Application.ReloadConfig)
			{
				ConfigManager.EnableAutoReloading(ConfigFileName);
				Log.Send("Config reloading enabled");
			}
		}

		/// <summary>
		/// Loads keybindings from the configuration file and registers them with Windows.
		/// </summary>
		private void AssignHotkeys(ActionDispatcher actionDispatcher)
		{
			try
			{
				var hotkeyManager = container.Resolve<HotkeyManager>();
				hotkeyManager.AssignHotkeys(
					ConfigManager.Config.Hotkeys ?? Array.Empty<HotkeyConfig>(),
					actionDispatcher,
					notificationService);
			}
			catch (Exception e) when (e is ArgumentException || e is HotkeyException)
			{
				Log.Send("Failed to register hotkeys", LogLevel.Error);
				Log.ReportException(e);

				if ((uint?)e.InnerException?.HResult == HResult.HotkeyAlreadyRegistered)
				{
					notificationService.ShowErrorWindow("An error occured in registering the hotkeys.\r\n" +
						"One or more hotkeys are already in use by another application.", "Could not register hotkeys");
				}
				else
				{
					notificationService.ShowErrorWindow(e.Message, "Could not register hotkeys");
				}
				App.Exit();
			}
		}


		public void Dispose()
		{
			notificationService?.Dispose();
			updateChecker?.Dispose();
			container?.Dispose();
		}
	}
}
