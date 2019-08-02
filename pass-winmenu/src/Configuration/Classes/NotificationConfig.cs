namespace PassWinmenu.Configuration
{
	internal class NotificationConfig
	{
		public bool Enabled { get; set; } = true;
		public NotificationTypesConfig Types { get; set; } = new NotificationTypesConfig();
	}
}
