using System;
using System.Collections.Generic;
using PassWinmenu.Utilities.ExtensionMethods;

namespace PassWinmenu.Configuration
{
	internal class CharacterGroupConfig
	{
		public string Name { get; set; }
		public string Characters { get; set; }
		public HashSet<int> CharacterSet => new HashSet<int>(Characters.ToCodePoints());
		public bool Enabled { get; set; }

		public CharacterGroupConfig()
		{
			Characters = string.Empty;
		}

		public CharacterGroupConfig(string name, string characters, bool enabled)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Characters = characters ?? throw new ArgumentNullException(nameof(characters));
			Enabled = enabled;
		}
	}
}
