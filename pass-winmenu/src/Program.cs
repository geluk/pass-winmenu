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
using LibGit2Sharp;
using PassWinmenu.Actions;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenu.Hotkeys;
using PassWinmenu.PasswordManagement;
using PassWinmenu.UpdateChecking;
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
		public const string ConfigFileName = @".\pass-winmenu.yaml";

		private ActionDispatcher actionDispatcher;
		private HotkeyManager hotkeys;
		private UpdateChecker updateChecker;
		private Notifications notificationService;

		private IContainer container;

		public Program()
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
				}
				Exit();
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

			// Set the security protocol to TLS 1.2 only.
			// We only use this for update checking (Git push over HTTPS is not handled by .NET).
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			// Create the notification service first, so it's available if initialisation fails.
			notificationService = Notifications.Create();
			
			// Initialise the DI Container builder.
			var builder = new ContainerBuilder();

			builder.Register(_ => notificationService)
				.AsImplementedInterfaces()
				.SingleInstance();
			
			// Now load the configuration options that we'll need 
			// to continue initialising the rest of the applications.
			LoadConfigFile();

			builder.Register(_ => ConfigManager.Config).AsSelf();
			builder.Register(_ => ConfigManager.Config.Gpg).AsSelf();
			builder.Register(_ => ConfigManager.Config.Git).AsSelf();
			builder.Register(_ => ConfigManager.Config.PasswordStore).AsSelf();
			builder.Register(_ => ConfigManager.Config.Application.UpdateChecking).AsSelf();

#if DEBUG
			Log.EnableFileLogging();
#else
			if (ConfigManager.Config.CreateLogFile)
			{
				Log.EnableFileLogging();
			}
#endif

			// Register actions
			builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(ActionDispatcher)))
				.InNamespaceOf<ActionDispatcher>()
				.Except<ActionDispatcher>()
				.AsImplementedInterfaces();

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
					typeof(GpgResultVerifier)
				).AsImplementedInterfaces()
				.AsSelf();

			// Register GPG installation
			builder.Register(context => context.Resolve<GpgInstallationFinder>().FindGpgInstallation(ConfigManager.Config.Gpg.GpgPath));

			// Register GPG
			builder.Register(context => new GPG(
					context.Resolve<IGpgTransport>(),
					context.Resolve<IGpgAgent>(),
					context.Resolve<IGpgResultVerifier>(),
					context.Resolve<GpgConfig>().PinentryFix))
				.AsImplementedInterfaces()
				.AsSelf();

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

			// Create the Git wrapper, if enabled.
			builder.Register(RegisterSyncService)
				.AsImplementedInterfaces();

			builder.Register(context => UpdateCheckerFactory.CreateUpdateChecker(context.Resolve<UpdateCheckingConfig>(), context.Resolve<INotificationService>()));

			// Build the container
			container = builder.Build();

			var gpgConfig = container.Resolve<GpgConfig>();
			if (gpgConfig.GpgAgent.Config.AllowConfigManagement)
			{
				container.Resolve<GpgAgentConfigUpdater>().UpdateAgentConfig(gpgConfig.GpgAgent.Config.Keys);
			}

			actionDispatcher = container.Resolve<ActionDispatcher>();

			notificationService.AddMenuActions(actionDispatcher);

			// Assign our hotkeys.
			hotkeys = new HotkeyManager();
			AssignHotkeys(hotkeys);
		}

		private static ISyncService RegisterSyncService (IComponentContext context)
		{
			var config = context.Resolve<GitConfig>();
			var passwordStore = context.ResolveNamed<IDirectoryInfo>("PasswordStore");
			var notificationService = context.Resolve<INotificationService>();

			if (config.UseGit)
			{
				try
				{
					return new SyncServiceFactory().BuildSyncService(config, passwordStore.FullName);
				}
				catch (RepositoryNotFoundException)
				{
					// Password store doesn't appear to be a Git repository.
					// Git support will be disabled.
				}
				catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
				{
					notificationService.ShowErrorWindow("The git2 DLL could not be found. Git support will be disabled.");
				}
				catch (Exception e)
				{
					notificationService.ShowErrorWindow($"Failed to open the password store Git repository ({e.GetType().Name}: {e.Message}). Git support will be disabled.");
				}
			}

			return null;
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
				Exit();
				return;
			}
			try
			{
				Log.Send("Using GPG version " + gpg.GetVersion());
			}
			catch (System.ComponentModel.Win32Exception)
			{
				notificationService.ShowErrorWindow("Could not find GPG. Make sure your gpg-path is set correctly.");
				Exit();
				return;
			}
			catch (Exception e)
			{
				notificationService.ShowErrorWindow($"Failed to initialise GPG. {e.GetType().Name}: {e.Message}");
				Exit();
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

		private void LoadConfigFile()
		{
			LoadResult result;
			try
			{
				result = ConfigManager.Load(ConfigFileName);
			}
			catch (Exception e) when (e.InnerException != null)
			{
				if (e is YamlException)
				{
					notificationService.ShowErrorWindow(
						$"The configuration file could not be loaded: {e.Message}\n\n{e.InnerException.GetType().Name}: {e.InnerException.Message}",
						"Unable to load configuration file.");
				}
				else
				{
					notificationService.ShowErrorWindow(
						$"The configuration file could not be loaded. An unhandled exception occurred.\n{e.InnerException.GetType().Name}: {e.InnerException.Message}",
						"Unable to load configuration file.");
				}

				Exit();
				return;
			}
			catch (SemanticErrorException e)
			{
				notificationService.ShowErrorWindow(
					$"The configuration file could not be loaded. An unhandled exception occurred.\n{e.GetType().Name}: {e.Message}",
					"Unable to load configuration file.");
				Exit();
				return;
			}
			catch (YamlException e)
			{
				notificationService.ShowErrorWindow(
					$"The configuration file could not be loaded. An unhandled exception occurred.\n{e.GetType().Name}: {e.Message}",
					"Unable to load configuration file.");
				Exit();
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
					Exit();
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
					Exit();
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
		private void AssignHotkeys(HotkeyManager hotkeyManager)
		{
			try
			{
				hotkeyManager.AssignHotkeys(
					ConfigManager.Config.Hotkeys ?? new HotkeyConfig[0],
					actionDispatcher,
					notificationService);
			}
			catch (Exception e) when (e is ArgumentException || e is HotkeyException)
			{
				Log.Send("Failed to register hotkeys", LogLevel.Error);
				Log.ReportException(e);

				notificationService.ShowErrorWindow(e.Message, "Could not register hotkeys");
				Exit();
			}
		}

		public static void Exit()
		{
			Log.Send("Shutting down.");
			Environment.Exit(0);
		}

		public void Dispose()
		{
			notificationService?.Dispose();
			hotkeys?.Dispose();
			updateChecker?.Dispose();
		}
	}
}
