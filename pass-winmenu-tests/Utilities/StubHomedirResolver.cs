using PassWinmenu.ExternalPrograms.Gpg;

namespace PassWinmenuTests.Utilities
{
	public class StubHomedirResolver : IGpgHomedirResolver
	{
		private readonly string homeDir;

		public StubHomedirResolver(string homeDir)
		{
			this.homeDir = homeDir;
		}

		public string GetHomeDir()
		{
			return homeDir;
		}

		public string GetConfiguredHomeDir()
		{
			return homeDir;
		}

		public string GetDefaultHomeDir()
		{
			return homeDir;
		}
	}
}
