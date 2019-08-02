using LibGit2Sharp;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.WinApi;

namespace PassWinmenu.Actions
{
	class CommitChangesAction : IAction
	{
		private readonly ISyncService syncService;
		private readonly INotificationService notificationService;

		public HotkeyAction ActionType => HotkeyAction.GitPush;

		public CommitChangesAction(ISyncService syncService, INotificationService notificationService)
		{
			this.syncService = syncService;
			this.notificationService = notificationService;
		}

		/// <summary>
		/// Commits all local changes and pushes them to remote.
		/// Also pulls any upcoming changes from remote.
		/// </summary>
		public void Execute()
		{
			if (syncService == null)
			{
				notificationService.Raise("Unable to commit your changes: pass-winmenu is not configured to use Git.",
										  Severity.Warning);
				return;
			}

			// First, commit any uncommitted files
			syncService.Commit();
			// Now fetch the latest changes
			try
			{
				syncService.Fetch();
			}
			// FIXME: dependency on derived type
			catch (LibGit2SharpException e) when (e.Message == "unsupported URL protocol")
			{
				notificationService.ShowErrorWindow(
					"Unable to push your changes: Remote uses an unknown protocol.\n\n" +
					"If your remote URL is an SSH URL, try setting sync-mode to native-git in your configuration file.");
				return;
			}
			catch (GitException e)
			{
				if (e.GitError != null)
				{
					notificationService.ShowErrorWindow(
						$"Unable to fetch the latest changes: Git returned an error.\n\n{e.GitError}");
				}
				else
				{
					notificationService.ShowErrorWindow($"Unable to fetch the latest changes: {e.Message}");
				}
			}

			var details = syncService.GetTrackingDetails();
			var local = details.AheadBy;
			var remote = details.BehindBy;
			try
			{
				syncService.Rebase();
			}
			catch (LibGit2SharpException e)
			{
				notificationService.ShowErrorWindow(
					$"Unable to rebase your changes onto the tracking branch:\n{e.Message}");
				return;
			}

			syncService.Push();

			if (!ConfigManager.Config.Notifications.Types.GitPush) return;
			if (local > 0 && remote > 0)
			{
				notificationService.Raise($"All changes pushed to remote ({local} pushed, {remote} pulled)",
										  Severity.Info);
			}
			else if (local.GetValueOrDefault() == 0 && remote.GetValueOrDefault() == 0)
			{
				notificationService.Raise("Nothing to commit.", Severity.Info);
			}
			else if (local > 0)
			{
				notificationService.Raise($"{local} changes have been pushed.", Severity.Info);
			}
			else if (remote > 0)
			{
				notificationService.Raise($"Nothing to commit. {remote} changes were pulled from remote.",
										  Severity.Info);
			}
		}
	}
}
