using PassWinmenu.Utilities.ExtensionMethods;

namespace PassWinmenu.Configuration
{
	internal class Config
	{
		public PasswordStoreConfig PasswordStore { get; set; } = new PasswordStoreConfig();
		public GitConfig Git { get; set; } = new GitConfig();
		public GpgConfig Gpg { get; set; } = new GpgConfig();
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

		public NotificationConfig Notifications { get; set; } = new NotificationConfig();

		public ApplicationConfig Application { get; set; } = new ApplicationConfig();

		public InterfaceConfig Interface { get; set; } = new InterfaceConfig();

		public bool CreateLogFile { get; set; } = false;
		public string ConfigVersion { get; set; }
	}
}
