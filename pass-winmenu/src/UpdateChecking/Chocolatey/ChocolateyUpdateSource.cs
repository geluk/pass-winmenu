using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
