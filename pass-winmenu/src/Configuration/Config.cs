using System;
using System.Collections.Generic;
using System.IO;
using PassWinmenu.Utilities;
using PassWinmenu.Utilities.ExtensionMethods;
using YamlDotNet.Serialization;

namespace PassWinmenu.Configuration
{
	internal class Config
	{
		private string passwordStore = Environment.ExpandEnvironmentVariables(@"%userprofile%\.password-store");
		public string PasswordStore
		{
			get => passwordStore;
			set
			{
				var expanded = Environment.ExpandEnvironmentVariables(value);
				passwordStore = Path.GetFullPath(Helpers.NormaliseDirectory(expanded));
			}
		}

		public string PasswordFileMatch { get; set; } = ".*\\.gpg$";

		private string gitPath = @"git";
		public string GitPath
		{
			get => gitPath;
			set
			{
				if (value == null) gitPath = null;
				else
				{
					var expanded = Environment.ExpandEnvironmentVariables(value);
					gitPath = Helpers.NormaliseDirectory(expanded);
				}
			}
		}


		public bool UseGit { get; set; } = true;
		public string SshPath { get; set; } = String.Empty;

		[YamlIgnore]
		public SyncMode SyncMode => (SyncMode)Enum.Parse(typeof(SyncMode), SyncModeString.ToPascalCase(), true);
		[YamlMember(Alias = "sync-mode")]
		public string SyncModeString { get; set; } = "builtin";

		public double ClipboardTimeout { get; set; } = 30;
		public string DirectorySeparator { get; set; } = "/";
		public StyleConfig Style { get; set; } = new StyleConfig();
		public OutputConfig Output { get; set; } = new OutputConfig();
		public HotkeyConfig[] Hotkeys { get; set; } =
		{
			new HotkeyConfig
			{
				Hotkey = "ctrl alt p",
				ActionString = "decrypt-password",
				Options = new HotkeyOptions
				{
					CopyToClipboard = true
				}
			},
			new HotkeyConfig
			{
				Hotkey = "ctrl alt shift p",
				ActionString = "decrypt-password",
				Options = new HotkeyOptions
				{
					CopyToClipboard = true,
					TypeUsername = true,
					TypePassword = true
				}
			}
		};

		public GpgConfig Gpg { get; set; } = new GpgConfig();
		public PasswordEditorConfig PasswordEditor { get; set; } = new PasswordEditorConfig();
		public PasswordGenerationConfig PasswordGeneration { get; set; } = new PasswordGenerationConfig();
		public UsernameDetectionConfig UsernameDetection { get; set; } = new UsernameDetectionConfig();
		public NotificationConfig Notifications { get; set; } = new NotificationConfig();
		public bool FirstLineOnly { get; set; } = true;
		public bool FollowCursor { get; set; } = true;
		public bool CreateLogFile { get; set; } = false;
	}

	internal enum SyncMode
	{
		Builtin,
		NativeGit
	}

	internal class GpgConfig
	{
		private string gpgPath = @"C:\Program Files (x86)\GnuPG\bin";
		public string GpgPath
		{
			get => gpgPath;
			set
			{
				if (value == null) gpgPath = null;
				else
				{
					var expanded = Environment.ExpandEnvironmentVariables(value);
					gpgPath = Helpers.NormaliseDirectory(expanded);
				}
			}
		}

		private string gnupghomeOverride;
		public string GnupghomeOverride
		{
			get => gnupghomeOverride;
			set
			{
				if (value == null) gnupghomeOverride = null;
				else
				{
					var expanded = Environment.ExpandEnvironmentVariables(value);
					gnupghomeOverride = Helpers.NormaliseDirectory(expanded);
				}

			}
		}

		public bool PinentryFix { get; set; } = false;
		public GpgAgentConfig GpgAgent { get; set; } = new GpgAgentConfig();
	}

	internal class GpgAgentConfig
	{
		public bool Preload { get; set; } = true;
		public GpgAgentConfigFile Config { get; set; } = new GpgAgentConfigFile();
	}

	internal class GpgAgentConfigFile
	{
		public bool AllowConfigManagement { get; set; }
		public Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();
	}

	internal class PasswordEditorConfig
	{
		public bool UseBuiltin { get; set; } = true;
	}

	internal class PasswordGenerationConfig
	{
		public string DefaultUsername { get; set; } = null;
		public string DefaultContent { get; set; } = "Username: \n";
	}

	internal class OutputConfig
	{
		public bool DeadKeys { get; set; } = false;
	}

	internal class UsernameDetectionConfig
	{
		[YamlIgnore]
		public UsernameDetectionMethod Method => (UsernameDetectionMethod)Enum.Parse(typeof(UsernameDetectionMethod), MethodString.ToPascalCase(), true);
		[YamlMember(Alias = "method")]
		public string MethodString { get; set; } = "regex";
		public UsernameDetectionOptions Options { get; set; } = new UsernameDetectionOptions();
	}

	internal class UsernameDetectionOptions
	{
		public int LineNumber { get; set; } = 2;
		public string Regex { get; set; } = @"^[Uu]sername: ((?<username>.*)\r|(?<username>.*))$";
		public UsernameDetectionRegexOptions RegexOptions { get; set; } = new UsernameDetectionRegexOptions();
	}

	internal class UsernameDetectionRegexOptions
	{
		public bool IgnoreCase { get; set; } = false;
		public bool Multiline { get; set; } = true;
		public bool Singleline { get; set; } = false;
	}

	internal enum UsernameDetectionMethod
	{
		FileName,
		LineNumber,
		Regex
	}

	internal enum HotkeyAction
	{
		DecryptPassword,
		AddPassword,
		EditPassword,
		GitPull,
		GitPush,
		OpenShell,
		ShowDebugInfo,
	}

	internal class HotkeyConfig
	{
		public string Hotkey { get; set; }
		[YamlIgnore]
		public HotkeyAction Action => (HotkeyAction)Enum.Parse(typeof(HotkeyAction), ActionString.ToPascalCase(), true);
		[YamlMember(Alias = "action")]
		public string ActionString { get; set; }
		public HotkeyOptions Options { get; set; } = new HotkeyOptions();
	}

	internal class HotkeyOptions
	{
		public bool CopyToClipboard { get; set; }
		public bool TypeUsername { get; set; }
		public bool TypePassword { get; set; }
	}

	internal class NotificationConfig
	{
		public bool Enabled { get; set; } = true;
		public NotificationTypesConfig Types { get; set; } = new NotificationTypesConfig();
	}

	internal class NotificationTypesConfig
	{
		public bool PasswordCopied { get; set; } = true;
		public bool PasswordGenerated { get; set; } = false;
		public bool PasswordUpdated { get; set; } = true;
		public bool GitPush { get; set; } = true;
		public bool GitPull { get; set; } = true;
		public bool NoSshKeyFound { get; set; } = true;
	}

	internal class StyleConfig
	{
		public LabelStyleConfig Search { get; set; } = new LabelStyleConfig { TextColour = "#FFDDDDDD", BackgroundColour = "#00FFFFFF", BorderWidth = 0, BorderColour = "#FF000000" };
		public LabelStyleConfig Options { get; set; } = new LabelStyleConfig { TextColour = "#FFDDDDDD", BackgroundColour = "#00FFFFFF", BorderWidth = 0, BorderColour = "#FF000000" };
		public LabelStyleConfig Selection { get; set; } = new LabelStyleConfig { TextColour = "#FFFFFFFF", BackgroundColour = "#FFD88900", BorderWidth = 0, BorderColour = "#FF000000" };
		public string Orientation { get; set; } = "vertical";
		public double FontSize { get; set; } = 14;
		public string FontFamily { get; set; } = "Consolas";
		public string BackgroundColour { get; set; } = "#FF202020";
		public string BorderColour { get; set; } = "#FFD88900";
		public string CaretColour { get; set; } = "#FFDDDDDD";
		// These have to be strings, because they need to support percentage values.
		public string OffsetLeft { get; set; } = "40%";
		public string OffsetTop { get; set; } = "40%";
		public string Width { get; set; } = "20%";
		public string Height { get; set; } = "20%";
	}

	internal class LabelStyleConfig
	{
		public string TextColour { get; set; }
		public string BackgroundColour { get; set; }
		public int BorderWidth { get; set; }
		public string BorderColour { get; set; }
	}
}
