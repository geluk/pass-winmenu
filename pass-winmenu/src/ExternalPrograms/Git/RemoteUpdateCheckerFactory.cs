using PassWinmenu.Configuration;
using PassWinmenu.Utilities;

namespace PassWinmenu.ExternalPrograms
{
	class RemoteUpdateCheckerFactory
	{
		private readonly Option<ISyncService> syncService;
		private readonly GitConfig gitConfig;
		private readonly ISyncStateTracker syncStateTracker;

		public RemoteUpdateCheckerFactory(Option<ISyncService> syncService, GitConfig gitConfig, ISyncStateTracker syncStateTracker)
		{
			this.syncService = syncService;
			this.gitConfig = gitConfig;
			this.syncStateTracker = syncStateTracker;
		}

		public Option<RemoteUpdateChecker> Build()
		{
			return syncService.Select(s => new RemoteUpdateChecker(s, gitConfig, syncStateTracker));
		}
	}
}
