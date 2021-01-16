using System;
using McSherry.SemanticVersioning;

namespace PassWinmenu.UpdateChecking
{
	internal struct ProgramVersion
	{
		public SemanticVersion VersionNumber { get; set; }
		public Uri ReleaseNotes { get; set; }
		public Uri DownloadLink { get; set; }
		public DateTime ReleaseDate { get; set; }
		public bool IsPrerelease { get; set; }

		/// <summary>
		/// True if the update to this version includes one or more important vulnerability fixes.
		/// </summary>
		public bool Important { get; set; }

		public override string ToString()
		{
			return VersionNumber.ToString(SemanticVersionFormat.PrefixedDefault);
		}
	}
}
