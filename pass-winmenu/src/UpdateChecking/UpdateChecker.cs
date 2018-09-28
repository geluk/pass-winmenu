using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassWinmenu.UpdateChecking
{
	internal class UpdateChecker
	{
		public IUpdateSource UpdateSource;

		public UpdateChecker()
		{
#if CHOCOLATEY
			UpdateSource = new ChocolateyUpdateSource();
#else
			UpdateSource = new GitHubUpdateSource();
#endif
		}

		public void CheckForUpdates()
		{
			var version = UpdateSource.GetLatestVersion();

		}

	}
}
