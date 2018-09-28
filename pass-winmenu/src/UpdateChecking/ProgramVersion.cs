using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McSherry.SemanticVersioning;

namespace PassWinmenu.UpdateChecking
{
	internal struct ProgramVersion
	{
		public SemanticVersion VersionNumber { get; set; }
		public Uri ReleaseNotes { get; set; }
		public Uri DownloadLink { get; set; }
		public DateTime ReleaseDate { get; set; }
	}
}
