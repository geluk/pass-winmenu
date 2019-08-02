using LibGit2Sharp;

using PassWinmenu.Configuration;
using PassWinmenu.WinApi;

namespace PassWinmenu.ExternalPrograms
{
	internal class SyncServiceFactory
	{
		public ISyncService BuildSyncService(GitConfig config, string passwordStorePath)
		{
			var repository = new Repository(passwordStorePath);

			IGitSyncStrategy strategy;
			if (config.SyncMode == SyncMode.NativeGit)
			{
				strategy = new NativeGitSyncStrategy(config.GitPath, passwordStorePath);
			}
			else
			{
				strategy = new LibGit2SharpSyncStrategy(repository);
			}

			return new Git(repository, strategy);
		}
	}
}
