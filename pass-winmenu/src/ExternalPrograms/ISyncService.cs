using LibGit2Sharp;

namespace PassWinmenu.ExternalPrograms
{
	internal interface ISyncService
	{
		void AddPassword(string passwordFilePath);
		void EditPassword(string passwordFilePath);
		RepositoryStatus Commit();
		void Fetch();
		void Push();
		void Rebase();
		TrackingDetails GetTrackingDetails();
	}
}
