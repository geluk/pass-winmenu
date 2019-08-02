using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McSherry.SemanticVersioning;

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
