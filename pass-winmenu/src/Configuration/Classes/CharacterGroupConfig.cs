using System;
using System.Collections.Generic;
using PassWinmenu.Utilities.ExtensionMethods;
using YamlDotNet.Serialization;

#nullable enable
namespace PassWinmenu.Configuration
{
	internal class CharacterGroupConfig
	{
		public string Name { get; set; } = "No name set";
		public string Characters { get; set; } = string.Empty;
		[YamlIgnore]
		public HashSet<int> CharacterSet => new HashSet<int>(Characters.ToCodePoints());
		public bool Enabled { get; set; }

		// Used by the deserialiser
		public CharacterGroupConfig()
		{
		}

		public CharacterGroupConfig(string name, string characters, bool enabled)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Characters = characters ?? throw new ArgumentNullException(nameof(characters));
			Enabled = enabled;
		}
	}
}
