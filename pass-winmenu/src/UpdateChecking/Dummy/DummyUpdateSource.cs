using System.Collections.Generic;

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
