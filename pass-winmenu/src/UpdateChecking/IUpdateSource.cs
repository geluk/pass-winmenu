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
		/// Fetches version information for the latest release.
		/// </summary>
		/// <returns>A <see cref="ProgramVersion"/> describing the latest release.</returns>
		ProgramVersion GetLatestVersion();

		/// <summary>
		/// Fetches version information for all published releases.
		/// </summary>
		/// <returns>An enumeration of <see cref="ProgramVersion"/> objects describing all published releases.</returns>
		IEnumerable<ProgramVersion> GetAllReleases();
	}
}
