using System.Linq;

namespace PassWinmenu.UpdateChecking
{
	internal static class UpdateSourceExtensions
	{
		/// <summary>
		/// Fetches version information for the latest release.
		/// </summary>
		/// <returns>A <see cref="ProgramVersion"/> describing the latest release.</returns>
		public static ProgramVersion GetLatestVersion(this IUpdateSource updateSource, bool allowPrerelease)
		{
			var releases = updateSource.GetAllReleases();
			var latest = releases.OrderByDescending(r => r.VersionNumber).First(r => allowPrerelease || !r.IsPrerelease);
			return latest;
		}
	}
}
