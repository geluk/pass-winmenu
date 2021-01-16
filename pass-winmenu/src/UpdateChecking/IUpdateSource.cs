using System.Collections.Generic;

namespace PassWinmenu.UpdateChecking
{
	internal interface IUpdateSource
	{
		/// <summary>
		/// Does the update source require an internet connection to check for updates?
		/// </summary>
		bool RequiresConnectivity { get; }
		
		/// <summary>
		/// Fetches version information for all published releases.
		/// </summary>
		/// <returns>An enumeration of <see cref="ProgramVersion"/> objects describing all published releases.</returns>
		IEnumerable<ProgramVersion> GetAllReleases();
	}
}
