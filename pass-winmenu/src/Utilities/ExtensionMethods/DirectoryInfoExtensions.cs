using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace PassWinmenu.Utilities.ExtensionMethods
{
	internal static class DirectoryInfoExtensions
	{
		/// <summary>
		/// Checks whether the directory contains the given file.
		/// </summary>
		/// <returns>True if the directory contains the file, false otherwise.</returns>
		internal static bool ContainsFile(this IDirectoryInfo directory, string name)
		{
			return directory.EnumerateFiles(name).Any();
		}

		/// <summary>
		/// Checks for path equality between two DirectoryInfo objects.
		/// Unlike a direct comparison of their FullName properties,
		/// this method ignores trailing slashes.
		/// </summary>
		/// <returns>True if both DirectoryInfo objects reference the same directory, false otherwise.</returns>
		internal static bool PathEquals(this IDirectoryInfo a, IDirectoryInfo b)
		{
			var pathA = Helpers.NormaliseDirectory(a.FullName);
			var pathB = Helpers.NormaliseDirectory(b.FullName);
			return pathA == pathB;
		}
	}
}
