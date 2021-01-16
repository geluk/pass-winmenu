using Moq;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Git
{
	public class RemoteUpdateCheckerTests
	{
		[Theory]
		[InlineData(0, 0, SyncState.UpToDate)]
		[InlineData(1, 0, SyncState.Ahead)]
		[InlineData(12, 0, SyncState.Ahead)]
		[InlineData(0, 1, SyncState.Behind)]
		[InlineData(0, 3, SyncState.Behind)]
		[InlineData(1, 1, SyncState.Diverged)]
		[InlineData(10, 1, SyncState.Diverged)]
		[InlineData(1, 3, SyncState.Diverged)]
		internal void CheckForUpdates_SetsCorrectSyncState(int ahead, int behind, SyncState syncState)
		{
			var syncService = new Mock<ISyncService>();
			syncService.Setup(s => s.GetTrackingDetails()).Returns(new TrackingDetails{AheadBy = ahead, BehindBy = behind});
			var tracker = new Mock<ISyncStateTracker>();
			var checker = new RemoteUpdateChecker(syncService.Object, new GitConfig(), tracker.Object);

			checker.CheckForUpdates();

			tracker.Verify(t => t.SetSyncState(syncState));
		}
	}
}
