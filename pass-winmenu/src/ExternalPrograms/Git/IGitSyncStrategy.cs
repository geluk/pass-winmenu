using LibGit2Sharp;

namespace PassWinmenu.ExternalPrograms
{
	public interface IGitSyncStrategy
	{
		void Fetch(Branch branch);

		void Push();
	}
}
