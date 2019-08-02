namespace PassWinmenu.Configuration
{
	internal class GpgAgentConfig
	{
		public bool Preload { get; set; } = true;
		public GpgAgentConfigFile Config { get; set; } = new GpgAgentConfigFile();
	}
}
