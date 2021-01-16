using System;
using LibGit2Sharp;

using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenu.WinApi;

namespace PassWinmenu.ExternalPrograms
{
	internal class SyncServiceFactory
	{
		private readonly GitConfig config;
		private readonly string passwordStorePath;
		private readonly ISignService signService;
		private readonly GitSyncStrategies gitSyncStrategies;

		public SyncServiceStatus Status { get; private set; }
		public Exception Exception { get; private set; }

		public SyncServiceFactory(GitConfig config, string passwordStorePath, ISignService signService, GitSyncStrategies gitSyncStrategies)
		{
			this.config = config;
			this.passwordStorePath = passwordStorePath;
			this.signService = signService;
			this.gitSyncStrategies = gitSyncStrategies;
		}
		
		public ISyncService BuildSyncService()
		{
			if (config.UseGit)
			{
				try
				{
					var repository = new Repository(passwordStorePath);

					var strategy = gitSyncStrategies.ChooseSyncStrategy(passwordStorePath, repository, config);
					var git = new Git(repository, strategy, signService);
					Status = SyncServiceStatus.GitSupportEnabled;
					return git;
				}
				catch (RepositoryNotFoundException)
				{
					Log.Send("The password store does not appear to be a Git repository; " +
					         "Git support will be disabled");
				}
				catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
				{
					Status = SyncServiceStatus.GitLibraryNotFound;
				}
				catch (Exception e)
				{
					Exception = e;
					Status = SyncServiceStatus.GitRepositoryNotFound;
				}
			}
			else
			{
				Status = SyncServiceStatus.GitSupportDisabled;
			}

			return null;
		}
	}

	internal enum SyncServiceStatus
	{
		GitSupportEnabled,
		GitLibraryNotFound,
		GitRepositoryNotFound,
		GitSupportDisabled,
	}
}
