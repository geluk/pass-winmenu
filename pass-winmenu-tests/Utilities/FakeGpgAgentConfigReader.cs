using PassWinmenu.ExternalPrograms.Gpg;

namespace PassWinmenuTests.Utilities
{
	class FakeGpgAgentConfigReader : IGpgAgentConfigReader
	{
		private string[] currentLines;

		public FakeGpgAgentConfigReader(string[] startLines)
		{
			currentLines = startLines;
		}

		public string[] ReadConfigLines()
		{
			return currentLines;
		}

		public void WriteConfigLines(string[] lines)
		{
			currentLines = lines;
		}
	}
}
