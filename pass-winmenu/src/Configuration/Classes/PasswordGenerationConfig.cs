namespace PassWinmenu.Configuration
{
	internal class PasswordGenerationConfig
	{
		public int Length { get; set; } = 20;
		public CharacterGroupConfig[] CharacterGroups { get; set; } =
		{
			new CharacterGroupConfig("Symbols", "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~", true),
			new CharacterGroupConfig("Numeric", "0123456789", true),
			new CharacterGroupConfig("Lowercase", "abcdefghijklmnopqrstuvwxyz", true),
			new CharacterGroupConfig("Uppercase", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", true),
			new CharacterGroupConfig("Whitespace", " ", false), 
		};
		public string DefaultContent { get; set; } = "Username: \n";
	}
}
