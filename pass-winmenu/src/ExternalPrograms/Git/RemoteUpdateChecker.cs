using System;
using System.Timers;
using PassWinmenu.Configuration;

namespace PassWinmenu.ExternalPrograms
{
	internal class RemoteUpdateChecker: IDisposable
	{
		private readonly ISyncService syncService;
		private readonly GitConfig gitConfig;
		private readonly ISyncStateTracker syncStateTracker;
		private Timer checkTimer;
		private Timer fetchTimer;
		private TimeSpan CheckInterval { get; } = TimeSpan.FromSeconds(10);
		private TimeSpan InitialFetchDelay { get; } = TimeSpan.FromMinutes(1);

		public RemoteUpdateChecker(ISyncService syncService, GitConfig gitConfig, ISyncStateTracker syncStateTracker)
		{
			this.syncService = syncService;
			this.gitConfig = gitConfig;
			this.syncStateTracker = syncStateTracker;
		}

		/// <summary>
		/// Starts checking for updates.
		/// </summary>
		public void Start()
		{
			checkTimer = new Timer(CheckInterval.TotalMilliseconds)
			{
				AutoReset = true,
			};
			checkTimer.Elapsed += (sender, args) =>
			{
				CheckForUpdates();
			};
			checkTimer.Start();

			if (gitConfig.AutoFetch)
			{
				fetchTimer = new Timer(InitialFetchDelay.TotalMilliseconds)
				{
					AutoReset = true,
				};
				fetchTimer.Elapsed += (sender, args) =>
				{
					fetchTimer.Interval = gitConfig.AutoFetchIntervalTimeSpan.TotalMilliseconds;
					Fetch();
				};
				fetchTimer.Start();
			}
		}

		public void Fetch()
		{
			try
			{
				syncService.Fetch();
			}
			catch (Exception e)
			{
				Log.Send("Failed to fetch latest updates from remote.");
				Log.ReportException(e);
			}
		}

		public void CheckForUpdates()
		{
			var trackingDetails = syncService.GetTrackingDetails();
			if (trackingDetails.BehindBy > 0 && trackingDetails.AheadBy > 0)
			{
				syncStateTracker.SetSyncState(SyncState.Diverged);
			}
			else if (trackingDetails.BehindBy > 0)
			{
				syncStateTracker.SetSyncState(SyncState.Behind);
			} 
			else if (trackingDetails.AheadBy > 0)
			{
				syncStateTracker.SetSyncState(SyncState.Ahead);
			}
			else
			{
				syncStateTracker.SetSyncState(SyncState.UpToDate);
			}
		}

		public void Dispose()
		{
			checkTimer?.Dispose();
			fetchTimer?.Dispose();
		}
	}
}
