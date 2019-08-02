using System.Collections.Generic;

namespace PassWinmenu.Configuration
{
	internal class GpgAgentConfigFile
	{
		public bool AllowConfigManagement { get; set; }
		public Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();
	}
}
