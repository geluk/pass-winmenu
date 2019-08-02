using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassWinmenu.UpdateChecking;

namespace PassWinmenu.UpdateChecking.Dummy
{
	internal sealed class DummyUpdateSource : IUpdateSource
	{
		public List<ProgramVersion> Versions { get; set; }

		public bool RequiresConnectivity => false;

		public IEnumerable<ProgramVersion> GetAllReleases()
		{
			return Versions;
		}
	}
}
