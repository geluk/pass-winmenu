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
		/// <summary>
		/// Determines the current version of the configuration file.
		/// Config file versions run synchronously with pass-winmenu versions,
		/// but not every pass-winmenu update will also bump the configuration file version.
		/// This only happens when there are changes preventing users from re-using an older
		/// configuration file for a newer version of pass-winmenu. In that case, the new
		/// configuration file version will be set to the latest version of pass-winmenu.
		/// </summary>
		public string ConfigVersion { get; set; }
	}
}
