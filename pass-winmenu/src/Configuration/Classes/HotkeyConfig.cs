using System;
using PassWinmenu.Utilities.ExtensionMethods;
using YamlDotNet.Serialization;

namespace PassWinmenu.Configuration
{
	internal class HotkeyConfig
	{
		public string Hotkey { get; set; }
		[YamlIgnore]
		public HotkeyAction Action => (HotkeyAction)Enum.Parse(typeof(HotkeyAction), ActionString.ToPascalCase(), true);
		[YamlMember(Alias = "action")]
		public string ActionString { get; set; }
		public HotkeyOptions Options { get; set; } = new HotkeyOptions();
	}
}
