using LibGit2Sharp;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.Utilities;
using PassWinmenu.WinApi;

namespace PassWinmenu.Actions
{
	class RetrieveChangesAction : IAction
	{
		private readonly ISyncService syncService;
		private readonly INotificationService notificationService;

		public HotkeyAction ActionType => HotkeyAction.GitPull;

		public RetrieveChangesAction(Option<ISyncService> syncService, INotificationService notificationService)
		{
			this.syncService = syncService.Value;
			this.notificationService = notificationService;
		}

		public void Execute()
		{
			if (syncService == null)
			{
				notificationService.Raise(
					"Unable to update the password store: pass-winmenu is not configured to use Git.",
					Severity.Warning);
				return;
			}

			try
			{
				syncService.Fetch();
				var details = syncService.GetTrackingDetails();
				if (details.BehindBy > 0)
				{
					syncService.Rebase();
					notificationService.Raise($"Pulled {details.BehindBy} new changes.", Severity.Info);
				}
				else
				{
					notificationService.Raise($"Your local repository already contains the latest changes.", Severity.Info);
				}
			}
			catch (LibGit2SharpException e) when (e.Message == "unsupported URL protocol")
			{
				notificationService.ShowErrorWindow(
					"Unable to update the password store: Remote uses an unknown protocol.\n\n" +
					"If your remote URL is an SSH URL, try setting sync-mode to native-git in your configuration file.");
			}
			catch (LibGit2SharpException e)
			{
				notificationService.ShowErrorWindow($"Unable to update the password store:\n{e.Message}");
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
		}
	}
}
