namespace PassWinmenu.ExternalPrograms.Gpg
{
	internal interface IGpgAgentConfigReader
	{
		string[] ReadConfigLines();
		void WriteConfigLines(string[] lines);
	}
}