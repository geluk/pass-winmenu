namespace PassWinmenu.Configuration
{
	internal class InterfaceConfig
	{
		public bool FollowCursor { get; set; } = true;
		public string DirectorySeparator { get; set; } = "/";
		public double ClipboardTimeout { get; set; } = 30;
		public bool RestoreClipboard { get; set; } = true;

		public HotkeyConfig[] Hotkeys { get; set; } =
		{
			new HotkeyConfig
			{
				Hotkey = "tab",
				ActionString = "select-next"
			},
			new HotkeyConfig
			{
				Hotkey = "shift tab",
				ActionString = "select-previous"
			}
		};
		public PasswordEditorConfig PasswordEditor { get; set; } = new PasswordEditorConfig();
		public StyleConfig Style { get; set; } = new StyleConfig();
	}
}
