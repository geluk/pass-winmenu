namespace PassWinmenu.Configuration
{
	internal class ApplicationConfig
	{
		public bool ReloadConfig { get; set; } = true;
		public UpdateCheckingConfig UpdateChecking { get; set; } = new UpdateCheckingConfig();
	}
}
