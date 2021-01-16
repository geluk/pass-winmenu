using System;
using System.Collections.Generic;

namespace PassWinmenu.UpdateChecking.Chocolatey
{
	internal class ChocolateyUpdateSource : IUpdateSource
	{
		public bool RequiresConnectivity => true;

		public IEnumerable<ProgramVersion> GetAllReleases()
		{
			throw new NotImplementedException();
		}
	}
}
