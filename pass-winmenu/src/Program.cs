using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using LibGit2Sharp;
using McSherry.SemanticVersioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PassWinmenu.Hotkeys;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.UpdateChecking;
using PassWinmenu.UpdateChecking.GitHub;
using PassWinmenu.Windows;
using YamlDotNet.Core;
using Application = System.Windows.Forms.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace PassWinmenu
{
	internal class Program : Form
	{
		public static string Version => EmbeddedResources.Version;
		public const string LastConfigVersion = "1.7";
		private const string EncryptedFileExtension = ".gpg";
		private const string PlaintextFileExtension = ".txt";
		private readonly NotifyIcon icon = new NotifyIcon();
		private readonly HotkeyManager hotkeys;
		private readonly StartupLink startupLink = new StartupLink("pass-winmenu");
		private UpdateChecker updateChecker;
		private Git git;
		private PasswordManager passwordManager;

		public Program()
		{
			hotkeys = new HotkeyManager();
			Name = "pass-winmenu (main window)";

			try
			{
				Initialise();
				RunInitialCheck();
			}
			catch (Exception e)
			{
				ShowErrorWindow($"pass-winmenu failed to start ({e.GetType().Name}: {e.Message})");
				Exit();
			}

		}

		/// <summary>
		/// Loads all required resources.
		/// </summary>
		protected void Initialise()
		{
			EmbeddedResources.Load();
			CreateNotifyIcon();
			LoadConfigFile();

#if DEBUG
			Log.Initialise();
#else
			if (ConfigManager.Config.CreateLogFile)
			{
				Log.Initialise();
			}
#endif
			Log.Send("------------------------------");
			Log.Send($"Starting pass-winmenu {Version}");
			Log.Send("------------------------------");

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			
			var gpg = new GPG();
			gpg.FindGpgInstallation(ConfigManager.Config.Gpg.GpgPath);
			if (ConfigManager.Config.Gpg.GpgAgent.Config.AllowConfigManagement)
			{
				gpg.UpdateAgentConfig(ConfigManager.Config.Gpg.GpgAgent.Config.Keys);
			}
			passwordManager = new PasswordManager(ConfigManager.Config.PasswordStore.Location, EncryptedFileExtension, gpg);
			passwordManager.PinentryFixEnabled = ConfigManager.Config.Gpg.PinentryFix;

			if (ConfigManager.Config.Git.UseGit)
			{
				try
				{
					if (ConfigManager.Config.Git.SyncMode == SyncMode.NativeGit)
					{
						git = new Git(ConfigManager.Config.PasswordStore.Location, ConfigManager.Config.Git.GitPath);
					}
					else
					{
						git = new Git(ConfigManager.Config.PasswordStore.Location);
					}
				}
				catch (RepositoryNotFoundException)
				{
					// Password store doesn't appear to be a Git repository.
					// Git support will be disabled.
				}
				catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
				{
					ShowErrorWindow("The git2 DLL could not be found. Git support will be disabled.");
				}
				catch (Exception e)
				{
					ShowErrorWindow($"Failed to open the password store Git repository ({e.GetType().Name}: {e.Message}). Git support will be disabled.");
				}
			}
			InitialiseUpdateChecker();

			AssignHotkeys(hotkeys);
		}

		private void InitialiseUpdateChecker()
		{
			if (!ConfigManager.Config.Application.UpdateChecking.CheckForUpdates) return;

#if CHOCOLATEY
			var updateSource = new ChocolateyUpdateSource();
#else
			var updateSource = new GitHubUpdateSource();
#endif
			var versionString = Version.Split('-').First();

			updateChecker = new UpdateChecker(updateSource, SemanticVersion.Parse(versionString, ParseMode.Lenient));
			updateChecker.UpdateAvailable += (sender, args) =>
			{
				// If the update contains important vulnerability fixes, always display a notification.
				if (ConfigManager.Config.Notifications.Types.UpdateAvailable || args.Version.Important)
				{
					icon.ContextMenuStrip.Items[0].Visible = true;
					RaiseNotification($"A new update ({args.Version.VersionNumber.ToString(SemanticVersionFormat.Concise)}) is available.", ToolTipIcon.Info);
				}
			};
			updateChecker.Start();
		}

		/// <summary>
		/// Checks if all components are configured correctly.
		/// </summary>
		private void RunInitialCheck()
		{
			if (!Directory.Exists(ConfigManager.Config.PasswordStore.Location))
			{
				ShowErrorWindow($"Could not find the password store at {Path.GetFullPath(ConfigManager.Config.PasswordStore.Location)}. Please make sure it exists.");
				Exit();
				return;
			}
			try
			{
				passwordManager.Gpg.GetVersion();
			}
			catch (Win32Exception)
			{
				ShowErrorWindow("Could not find GPG. Make sure your gpg-path is set correctly.");
				Exit();
				return;
			}
			catch (Exception e)
			{
				ShowErrorWindow($"Failed to initialise GPG. {e.GetType().Name}: {e.Message}");
				Exit();
				return;
			}
			if (ConfigManager.Config.Gpg.GpgAgent.Preload)
			{
				Task.Run(() =>
				{
					try
					{
						passwordManager.Gpg.StartAgent();
					}
					catch (GpgError err)
					{
						ShowErrorWindow(err.Message);
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
			const string configFileName = "pass-winmenu.yaml";
			ConfigManager.LoadResult result;
			try
			{
				result = ConfigManager.Load(configFileName);
			}
			catch (Exception e) when (e.InnerException != null)
			{
				ShowErrorWindow(
					$"The configuration file could not be loaded. An unhandled exception occurred.\n{e.InnerException.GetType().Name}: {e.InnerException.Message}",
					"Unable to load configuration file.");
				Exit();
				return;
			}
			catch (SemanticErrorException e)
			{
				ShowErrorWindow(
					$"The configuration file could not be loaded. An unhandled exception occurred.\n{e.GetType().Name}: {e.Message}",
					"Unable to load configuration file.");
				Exit();
				return;
			}
			catch (YamlException e)
			{
				ShowErrorWindow(
					$"The configuration file could not be loaded. An unhandled exception occurred.\n{e.GetType().Name}: {e.Message}",
					"Unable to load configuration file.");
				Exit();
				return;
			}

			switch (result)
			{
				case ConfigManager.LoadResult.FileCreationFailure:
					RaiseNotification("A default configuration file was generated, but could not be saved.\nPass-winmenu will fall back to its default settings.", ToolTipIcon.Error);
					break;
				case ConfigManager.LoadResult.NewFileCreated:
					var open = MessageBox.Show("A new configuration file has been generated. Please modify it according to your preferences and restart the application.\n\n" +
					                           "Would you like to open it now?", "New configuration file created", MessageBoxButton.YesNo);
					if (open == MessageBoxResult.Yes) Process.Start(configFileName);
					Exit();
					return;
				case ConfigManager.LoadResult.NeedsUpgrade:
					var backedUpFile = ConfigManager.Backup(configFileName);
					var openBoth = MessageBox.Show("The current configuration file is out of date. A new configuration file has been created, and the old file has been backed up.\n" +
					                               "Please edit the new configuration file according to your preferences and restart the application.\n\n" +
					                               "Would you like to open both files now?", "Configuration file out of date", MessageBoxButton.YesNo);
					if (openBoth == MessageBoxResult.Yes)
					{
						Process.Start(configFileName);
						Process.Start(backedUpFile);
					}
					Exit();
					return;
			}
		}

		private void Exit()
		{
			Close();
			Application.Exit();
			Environment.Exit(0);
		}

		protected override void SetVisibleCore(bool value)
		{
			// Do not allow this window to be made visible.
			base.SetVisibleCore(false);
		}

		/// <summary>
		/// Loads keybindings from the configuration file and registers them with Windows.
		/// </summary>
		private void AssignHotkeys(HotkeyManager hotkeyManager)
		{
			try
			{
				foreach (var hotkey in ConfigManager.Config.Hotkeys ?? new HotkeyConfig[]{})
				{
					var keys = KeyCombination.Parse(hotkey.Hotkey);
					HotkeyAction action;
					try
					{
						// Reading the Action variable will cause it to be parsed from hotkey.ActionString.
						// If this fails, an ArgumentException is thrown.
						action = hotkey.Action;
					}
					catch (ArgumentException)
					{
						RaiseNotification($"Invalid hotkey configuration in config.yaml.\nThe action \"{hotkey.ActionString}\" is not known.", ToolTipIcon.Error);
						continue;
					}
					switch (action)
					{
						case HotkeyAction.DecryptPassword:
							hotkeyManager.AddHotKey(keys, () => DecryptPassword(hotkey.Options.CopyToClipboard, hotkey.Options.TypeUsername, hotkey.Options.TypePassword));
							break;
						case HotkeyAction.AddPassword:
							hotkeyManager.AddHotKey(keys, AddPassword);
							break;
						case HotkeyAction.EditPassword:
							hotkeyManager.AddHotKey(keys, EditPassword);
							break;
						case HotkeyAction.GitPull:
							hotkeyManager.AddHotKey(keys, UpdatePasswordStore);
							break;
						case HotkeyAction.GitPush:
							hotkeyManager.AddHotKey(keys, CommitChanges);
							break;
						case HotkeyAction.OpenShell:
							hotkeyManager.AddHotKey(keys, OpenPasswordShell);
							break;
						case HotkeyAction.ShowDebugInfo:
							hotkeyManager.AddHotKey(keys, ShowDebugInfo);
							break;
						case HotkeyAction.CheckForUpdates:
							hotkeyManager.AddHotKey(keys, updateChecker.CheckForUpdates);
							break;
					}
				}
			}
			catch (Exception e) when (e is ArgumentException || e is HotkeyException)
			{
				RaiseNotification(e.Message, ToolTipIcon.Error);
				Exit();
			}
		}

		/// <summary>
		/// Presents a notification to the user, provided notifications are enabled.
		/// </summary>
		/// <param name="message">The message that should be displayed.</param>
		/// <param name="tipIcon">The type of icon that should be displayed next to the message.</param>
		/// <param name="timeout">The time period, in milliseconds, the notification should display.</param>
		private void RaiseNotification(string message, ToolTipIcon tipIcon, int timeout = 5000)
		{
			if (ConfigManager.Config.Notifications.Enabled)
			{
				icon.ShowBalloonTip(timeout, "pass-winmenu", message, tipIcon);
			}
		}

		/// <summary>
		/// Shows an error dialog to the user.
		/// </summary>
		/// <param name="message">The error message to be displayed.</param>
		/// <param name="title">The window title of the error dialog.</param>
		/// <remarks>
		/// It might seem a bit inconsistent that some errors are sent as notifications, while others are
		/// displayed in a MessageBox using the ShowErrorWindow method below.
		/// The reasoning for this is explained here.
		/// ShowErrorWindow is used for any error that results from an action initiated by a user and 
		/// prevents that action for being completed successfully, as well as any error that forces the
		/// application to exit.
		/// Any other errors should be sent as notifications, which aren't as intrusive as an error dialog that
		/// forces you to stop doing whatever you were doing and click OK before you're allowed to continue.
		/// </remarks>
		private void ShowErrorWindow(string message, string title = "An error occurred.")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void ShowDebugInfo()
		{
			var gitData = "";
			if (git != null)
			{
				gitData = $"\tbehind by:\t{git.GetTrackingDetails().BehindBy}\n" +
				          $"\tahead by:\t\t{git.GetTrackingDetails().AheadBy}\n";
			}

			var debugInfo = $"gpg.exe path:\t\t{passwordManager.Gpg.GpgExePath}\n" +
			                $"gpg version:\t\t{passwordManager.Gpg.GetVersion()}\n" +
			                $"gpg homedir:\t\t{passwordManager.Gpg.GetConfiguredHomeDir()}\n" +
			                $"password store:\t\t{passwordManager.GetPasswordFilePath(".").TrimEnd('.')}\n" +
			                $"git enabled:\t\t{git != null}\n{gitData}";
			MessageBox.Show(debugInfo, "Debugging information", MessageBoxButton.OK, MessageBoxImage.None);
		}

		/// <summary>
		/// Opens the password menu and displays it to the user, allowing them to choose an existing password file.
		/// </summary>
		/// <param name="options">A list of options the user can choose from.</param>
		/// <returns>One of the values contained in <paramref name="options"/>, or null if no option was chosen.</returns>
		private string ShowPasswordMenu(IEnumerable<string> options)
		{
			MainWindowConfiguration windowConfig;
			try
			{
				windowConfig = MainWindowConfiguration.ParseMainWindowConfiguration(ConfigManager.Config);
			}
			catch (ConfigurationParseException e)
			{
				RaiseNotification(e.Message, ToolTipIcon.Error);
				return null;
			}

			var menu = new PasswordSelectionWindow(options, windowConfig);
			menu.ShowDialog();
			if (menu.Success)
			{
				return menu.GetSelection();
			}
			else
			{
				return null;
			}
		}

		private string ShowFileSelectionWindow()
		{
			MainWindowConfiguration windowConfig;
			try
			{
				windowConfig = MainWindowConfiguration.ParseMainWindowConfiguration(ConfigManager.Config);
			}
			catch (ConfigurationParseException e)
			{
				RaiseNotification(e.Message, ToolTipIcon.Error);
				return null;
			}

			// Ask the user where the password file should be placed.
			var pathWindow = new FileSelectionWindow(ConfigManager.Config.PasswordStore.Location, windowConfig);
			pathWindow.ShowDialog();
			if (!pathWindow.Success)
			{
				return null;
			}
			return pathWindow.GetSelection();
		}

		/// <summary>
		/// Copies a string to the clipboard. If it still exists on the clipboard after the amount of time
		/// specified in <paramref name="timeout"/>, it will be removed again.
		/// </summary>
		/// <param name="value">The text to add to the clipboard.</param>
		/// <param name="timeout">The amount of time, in seconds, the text should remain on the clipboard.</param>
		private void CopyToClipboard(string value, double timeout)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<string, double>(CopyToClipboard), value, timeout);
				return;
			}
			// Try to save the current contents of the clipboard and restore them after the password is removed.
			var previousText = "";
			if (Clipboard.ContainsText())
			{
				Log.Send("Saving previous clipboard contents before storing the password");
				previousText = Clipboard.GetText();
			}
			//Clipboard.SetText(value);
			try
			{
				Clipboard.SetDataObject(value);
			}
			catch (Exception e)
			{
				Log.Send($"Password could not be copied to clipboard: {e.GetType().Name}: {e.Message}", LogLevel.Error);
				ShowErrorWindow($"Failed to copy your password to the clipboard ({e.GetType().Name}: {e.Message}).");
			}

			Task.Delay(TimeSpan.FromSeconds(timeout)).ContinueWith(_ =>
			{
				Invoke((MethodInvoker)(() =>
				{
					try
					{
						// Only reset the clipboard to its previous contents if it still contains the text we copied to it.
						// If the clipboard did not previously contain any text, it is simply cleared.
						if (Clipboard.ContainsText() && Clipboard.GetText() == value)
						{
							Log.Send("Restoring previous clipboard contents");
							Clipboard.SetText(previousText);
						}
					}
					catch (Exception e)
					{
						Log.Send($"Failed to restore previous clipboard contents ({previousText.Length} chars): An exception occurred ({e.GetType().Name}: {e.Message})", LogLevel.Error);
					}
				}));
			});
		}

		protected override void Dispose(bool disposing)
		{
			git?.Dispose();
			icon?.Dispose();
			hotkeys?.Dispose();
			base.Dispose(disposing);
		}

		/// <summary>
		/// Creates a notification area icon for the application.
		/// </summary>
		private void CreateNotifyIcon()
		{
			icon.Icon = EmbeddedResources.Icon;
			icon.Visible = true;
			var menu = new ContextMenuStrip();
			menu.Items.Add(new ToolStripLabel("pass-winmenu " + Version));
			menu.Items.Add(new ToolStripSeparator());

			var downloadUpdate = new ToolStripMenuItem("Download Update");
			downloadUpdate.Click += (sender, args) => Process.Start(updateChecker.LatestVersion.Value.ReleaseNotes.ToString());
			downloadUpdate.BackColor = Color.Beige;
			//downloadUpdate.Visible = false;

			menu.Items.Add(downloadUpdate);
			menu.Items.Add("Decrypt Password", null, (sender, args) => Task.Run(() => DecryptPassword(true, false, false)));
			menu.Items.Add("Add new Password", null, (sender, args) => Task.Run((Action)AddPassword));
			menu.Items.Add("Edit Password File", null, (sender, args) => Task.Run(() => EditPassword()));
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add("Push to Remote", null, (sender, args) => Task.Run((Action)CommitChanges));
			menu.Items.Add("Pull from Remote", null, (sender, args) => Task.Run((Action)UpdatePasswordStore));
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add("Open Explorer", null, (sender, args) => Process.Start(ConfigManager.Config.PasswordStore.Location));
			menu.Items.Add("Open Shell", null, (sender, args) => Task.Run((Action)OpenPasswordShell));
			menu.Items.Add(new ToolStripSeparator());
			var startWithWindows = new ToolStripMenuItem("Start with Windows")
			{
				Checked = startupLink.Exists
			};
			startWithWindows.Click += (sender, args) =>
			{
				var target = Assembly.GetExecutingAssembly().Location;
				var workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
				startupLink.Toggle(target, workingDirectory);
				startWithWindows.Checked = startupLink.Exists;
			};

			menu.Items.Add(startWithWindows);
			menu.Items.Add("About", null, (sender, args) => Process.Start("https://github.com/Baggykiin/pass-winmenu#readme"));
			menu.Items.Add("Quit", null, (sender, args) => Close());
			icon.ContextMenuStrip = menu;
		}

		private void OpenPasswordShell()
		{
			var powershell = new ProcessStartInfo
			{
				FileName = "powershell",
				WorkingDirectory = ConfigManager.Config.PasswordStore.Location,
				UseShellExecute = false
			};

			var gpgLocation = passwordManager.Gpg.GpgExePath;
			if (gpgLocation.Contains(Path.DirectorySeparatorChar) || gpgLocation.Contains(Path.AltDirectorySeparatorChar))
			{
				// gpgLocation is a path, so ensure it's absolute.
				gpgLocation = Path.GetFullPath(gpgLocation);
			}
			else if (gpgLocation == "gpg")
			{
				// This would conflict with our function name, so rename it to gpg.exe.
				gpgLocation = "gpg.exe";
			}

			var homeDir = passwordManager.Gpg.GetConfiguredHomeDir();
			if (homeDir != null)
			{
				homeDir = $" --homedir \"{Path.GetFullPath(homeDir)}\"";
			}
			powershell.Arguments = $"-NoExit -Command \"function gpg() {{ & '{gpgLocation}'{homeDir} $args }}\"";
			Process.Start(powershell);
		}

		/// <summary>
		/// Commits all local changes and pushes them to remote.
		/// Also pulls any upcoming changes from remote.
		/// </summary>
		private void CommitChanges()
		{
			if (git == null)
			{
				RaiseNotification("Unable to commit your changes: pass-winmenu is not configured to use Git.", ToolTipIcon.Warning);
				return;
			}
			// First, commit any uncommitted files
			git.Commit();
			// Now fetch the latest changes
			try
			{
				git.Fetch();
			}
			catch (LibGit2SharpException e) when (e.Message == "unsupported URL protocol")
			{
				ShowErrorWindow("Unable to push your changes: Remote uses an unknown protocol.\n\n" +
				                "If your remote URL is an SSH URL, try setting sync-mode to native-git in your configuration file.");
				return;
			}
			catch (GitException e)
			{
				if (e.GitError != null)
				{
					ShowErrorWindow($"Unable to fetch the latest changes: Git returned an error.\n\n{e.GitError}");
				}
				else
				{
					ShowErrorWindow($"Unable to fetch the latest changes: {e.Message}");
				}
			}
			var details = git.GetTrackingDetails();
			var local = details.AheadBy;
			var remote = details.BehindBy;
			try
			{
				git.Rebase();
			}
			catch (LibGit2SharpException e)
			{
				ShowErrorWindow($"Unable to rebase your changes onto the tracking branch:\n{e.Message}");
				return;
			}
			git.Push();

			if (!ConfigManager.Config.Notifications.Types.GitPush) return;
			if (local > 0 && remote > 0)
			{
				RaiseNotification($"All changes pushed to remote ({local} pushed, {remote} pulled)", ToolTipIcon.Info);
			}
			else if (local.GetValueOrDefault() == 0 && remote.GetValueOrDefault() == 0)
			{
				RaiseNotification("Nothing to commit.", ToolTipIcon.Info);
			}
			else if (local > 0)
			{
				RaiseNotification($"{local} changes have been pushed.", ToolTipIcon.Info);
			}
			else if (remote > 0)
			{
				RaiseNotification($"Nothing to commit. {remote} changes were pulled from remote.", ToolTipIcon.Info);
			}
		}

		/// <summary>
		/// Adds a new password to the password store.
		/// </summary>
		private void AddPassword()
		{
			if (InvokeRequired)
			{
				Invoke((MethodInvoker)AddPassword);
				return;
			}
			var passwordFileName = ShowFileSelectionWindow();
			// passwordFileName will be null if no file was selected
			if (passwordFileName == null) return;

			// Display the password generation window.
			string password;
			string extraContent;
			using (var passwordWindow = new PasswordWindow(Path.GetFileName(passwordFileName)))
			{
				passwordWindow.ShowDialog();
				if (!passwordWindow.DialogResult.GetValueOrDefault())
				{
					return;
				}
				password = passwordWindow.Password.Text;
				extraContent = passwordWindow.ExtraContent.Text.Replace(Environment.NewLine, "\n");
			}

			try
			{
				passwordManager.EncryptPassword(new PasswordFileContent(password, extraContent), passwordFileName + passwordManager.EncryptedFileExtension);
			}
			catch (GpgException e)
			{
				ShowErrorWindow("Unable to encrypt your password: " + e.Message);
				return;
			}
			catch (ConfigurationException e)
			{
				ShowErrorWindow("Unable to encrypt your password: " + e.Message);
				return;
			}
			// Copy the newly generated password.
			CopyToClipboard(password, ConfigManager.Config.Interface.ClipboardTimeout);

			if (ConfigManager.Config.Notifications.Types.PasswordGenerated)
			{
				RaiseNotification($"The new password has been copied to your clipboard.\nIt will be cleared in {ConfigManager.Config.Interface.ClipboardTimeout:0.##} seconds.", ToolTipIcon.Info);
			}
			// Add the password to Git
			git?.AddPassword(passwordFileName + passwordManager.EncryptedFileExtension);
		}

		/// <summary>
		/// Updates the password store so it's in sync with remote again.
		/// </summary>
		private void UpdatePasswordStore()
		{
			if (git == null)
			{
				RaiseNotification("Unable to update the password store: pass-winmenu is not configured to use Git.", ToolTipIcon.Warning);
				return;
			}
			try
			{
				git.Fetch();
				git.Rebase();
			}
			catch (LibGit2SharpException e) when(e.Message == "unsupported URL protocol")
			{
				ShowErrorWindow("Unable to update the password store: Remote uses an unknown protocol.\n\n" +
				                "If your remote URL is an SSH URL, try setting sync-mode to native-git in your configuration file.");
			}
			catch (LibGit2SharpException e)
			{
				ShowErrorWindow($"Unable to update the password store:\n{e.Message}");
			}
			catch (GitException e)
			{
				if (e.GitError != null)
				{
					ShowErrorWindow($"Unable to fetch the latest changes: Git returned an error.\n\n{e.GitError}");
				}
				else
				{
					ShowErrorWindow($"Unable to fetch the latest changes: {e.Message}");
				}
			}
		}

		/// <summary>
		/// Asks the user to choose a password file.
		/// </summary>
		/// <returns>
		/// The path to the chosen password file (relative to the password directory),
		/// or null if the user didn't choose anything.
		/// </returns>
		private string RequestPasswordFile()
		{
			if (InvokeRequired)
			{
				return (string)Invoke((Func<string>)RequestPasswordFile);
			}
			// Find GPG-encrypted password files
			var passFiles = passwordManager.GetPasswordFiles(ConfigManager.Config.PasswordStore.PasswordFileMatch).ToList();
			if (passFiles.Count == 0)
			{
				MessageBox.Show("Your password store doesn't appear to contain any passwords yet.", "Empty password store", MessageBoxButton.OK, MessageBoxImage.Information);
				return null;
			}
			// Build a dictionary mapping display names to relative paths
			var displayNameMap = passFiles.ToDictionary(val => val.Substring(0, val.Length - EncryptedFileExtension.Length).Replace(Path.DirectorySeparatorChar.ToString(), ConfigManager.Config.Interface.DirectorySeparator));

			var selection = ShowPasswordMenu(displayNameMap.Keys);
			if (selection == null) return null;
			return displayNameMap[selection];
		}

		/// <summary>
		/// Asks the user to choose a password file, decrypts it, and copies the resulting value to the clipboard.
		/// </summary>
		private void DecryptPassword(bool copyToClipboard, bool typeUsername, bool typePassword)
		{
			// We need to be on the main thread for this.
			if (InvokeRequired)
			{
				Invoke((Action<bool, bool, bool>)DecryptPassword, copyToClipboard, typeUsername, typePassword);
				return;
			}

			var selectedFile = RequestPasswordFile();
			// If the user cancels their selection, the password decryption should be cancelled too.
			if (selectedFile == null) return;

			PasswordFileContent passFile;
			try
			{
				passFile = passwordManager.DecryptPassword(selectedFile, ConfigManager.Config.PasswordStore.FirstLineOnly);
			}
			catch (GpgError e)
			{
				ShowErrorWindow("Password decryption failed: " + e.Message);
				return;
			}
			catch (GpgException e)
			{
				ShowErrorWindow("Password decryption failed. " + e.Message);
				return;
			}
			catch (ConfigurationException e)
			{
				ShowErrorWindow("Password decryption failed: " + e.Message);
				return;
			}
			catch (Exception e)
			{
				ShowErrorWindow($"Password decryption failed: An error occurred: {e.GetType().Name}: {e.Message}");
				return;
			}

			if (copyToClipboard)
			{
				CopyToClipboard(passFile.Password, ConfigManager.Config.Interface.ClipboardTimeout);
				if (ConfigManager.Config.Notifications.Types.PasswordCopied)
				{
					RaiseNotification($"The password has been copied to your clipboard.\nIt will be cleared in {ConfigManager.Config.Interface.ClipboardTimeout:0.##} seconds.", ToolTipIcon.Info);
				}
			}
			var usernameEntered = false;
			if (typeUsername)
			{
				var username = GetUsername(selectedFile, passFile.ExtraContent);
				if (username != null)
				{
					EnterText(username, ConfigManager.Config.Output.DeadKeys);
					usernameEntered = true;
				}
			}
			if (typePassword)
			{
				// If a username has also been entered, press Tab to switch to the password field.
				if (usernameEntered) SendKeys.Send("{TAB}");

				EnterText(passFile.Password, ConfigManager.Config.Output.DeadKeys);
			}
		}

		private void EditPassword()
		{
			var selectedFile = RequestPasswordFile();
			if (selectedFile == null) return;

			if (ConfigManager.Config.Interface.PasswordEditor.UseBuiltin)
			{
				EditWithEditWindow(selectedFile);
			}
			else
			{
				EditWithTextEditor(selectedFile);
			}
		}

		private void EditWithEditWindow(string selectedFile)
		{
			if (InvokeRequired)
			{
				Invoke((Action<string>)EditWithEditWindow, selectedFile);
				return;
			}

			var content = passwordManager.DecryptText(selectedFile);
			using (var window = new EditWindow(selectedFile, content))
			{
				if (window.ShowDialog() ?? false)
				{
					try
					{
						File.Delete(passwordManager.GetPasswordFilePath(selectedFile));
						passwordManager.EncryptText(window.PasswordContent.Text, selectedFile);
						git?.EditPassword(selectedFile);
						if (ConfigManager.Config.Notifications.Types.PasswordUpdated)
						{
							RaiseNotification($"Password file \"{selectedFile}\" has been updated.", ToolTipIcon.Info);
						}
					}
					catch (Exception e)
					{
						ShowErrorWindow($"Unable to save your password (encryption failed): {e.Message}");
					}
				}
			}
		}

		private void EditWithTextEditor(string selectedFile)
		{
			string decryptedFile, plaintextFile;
			try
			{
				decryptedFile = passwordManager.DecryptFile(selectedFile);
				plaintextFile = decryptedFile + PlaintextFileExtension;
				// Add a plaintext extension to the decrypted file so it can be opened with a text editor
				File.Move(decryptedFile, plaintextFile);
			}
			catch (Exception e)
			{
				ShowErrorWindow($"Unable to edit your password (decryption failed): {e.Message}");
				return;
			}

			// Open the file in the user's default editor
			try
			{
				Process.Start(plaintextFile);
			}
			catch (Win32Exception e)
			{
				ShowErrorWindow($"Unable to open an editor to edit your password file ({e.Message}).");
				File.Delete(plaintextFile);
				return;
			}

			var result = MessageBox.Show(
				"Please keep this window open until you're done editing the password file.\n" +
				"Then click Yes to save your changes, or No to discard them.",
				$"Save changes to {Path.GetFileName(selectedFile)}?",
				MessageBoxButton.YesNo,
				MessageBoxImage.Information);

			if (result == MessageBoxResult.Yes)
			{
				var selectedFilePath = passwordManager.GetPasswordFilePath(selectedFile);
				File.Delete(selectedFilePath);
				// Remove the plaintext extension again before re-encrypting the file
				File.Move(plaintextFile, decryptedFile);
				passwordManager.EncryptFile(decryptedFile);
				File.Delete(decryptedFile);
				git?.EditPassword(selectedFile);
				if (ConfigManager.Config.Notifications.Types.PasswordUpdated)
				{
					RaiseNotification($"Password file \"{selectedFile}\" has been updated.", ToolTipIcon.Info);
				}
			}
			else
			{
				File.Delete(plaintextFile);
			}
		}

		/// <summary>
		/// Attepts to retrieve the username from a password file.
		/// </summary>
		/// <param name="passwordFile">The name of the password file.</param>
		/// <param name="extraContent">The extra content of the password file.</param>
		/// <returns>A string containing the username if the password file contains one, null if no username was found.</returns>
		private string GetUsername(string passwordFile, string extraContent)
		{
			var options = ConfigManager.Config.PasswordStore.UsernameDetection.Options;
			switch (ConfigManager.Config.PasswordStore.UsernameDetection.Method)
			{
				case UsernameDetectionMethod.FileName:
					return Path.GetFileName(passwordFile)?.Replace(EncryptedFileExtension, "");
				case UsernameDetectionMethod.LineNumber:
					var extraLines = extraContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
					var lineNumber = options.LineNumber - 2;
					if (lineNumber <= 1) RaiseNotification("Failed to read username from password file: username-detection.options.line-number must be set to 2 or higher.", ToolTipIcon.Warning);
					return lineNumber < extraLines.Length ? extraLines[lineNumber] : null;
				case UsernameDetectionMethod.Regex:
					var rgxOptions = options.RegexOptions.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
					rgxOptions = rgxOptions | (options.RegexOptions.Multiline ? RegexOptions.Multiline : RegexOptions.None);
					rgxOptions = rgxOptions | (options.RegexOptions.Singleline ? RegexOptions.Singleline : RegexOptions.None);
					var match = Regex.Match(extraContent, options.Regex, rgxOptions);
					return match.Groups["username"].Success ? match.Groups["username"].Value : null;
				default:
					throw new ArgumentOutOfRangeException("username-detection.method", "Invalid username detection method.");
			}
		}

		/// <summary>
		/// Sends text directly to the topmost window, as if it was entered by the user.
		/// This method automatically escapes all characters with special meaning, 
		/// then calls SendKeys.Send().
		/// </summary>
		/// <param name="text">The text to be sent to the active window.</param>
		/// <param name="escapeDeadKeys">Whether dead keys should be escaped or not. 
		/// If true, inserts a space after every dead key in order to prevent it from being combined with the next character.</param>
		private static void EnterText(string text, bool escapeDeadKeys)
		{
			if (escapeDeadKeys)
			{
				// If dead keys are enabled, insert a space directly after each dead key to prevent
				// it from being combined with the character following it.
				// See https://en.wikipedia.org/wiki/Dead_key
				var deadKeys = new[] { "\"", "'", "`", "~", "^" };
				text = deadKeys.Aggregate(text, (current, key) => current.Replace(key, key + " "));
			}

			// SendKeys.Send expects special characters to be escaped by wrapping them with curly braces.
			var specialCharacters = new[] { '{', '}', '[', ']', '(', ')', '+', '^', '%', '~' };
			var escaped = string.Concat(text.Select(c => specialCharacters.Contains(c) ? $"{{{c}}}" : c.ToString()));
			SendKeys.Send(escaped);
		}

		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Program());
		}
	}
}
