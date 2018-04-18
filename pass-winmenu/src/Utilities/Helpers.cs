using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PassWinmenu.Utilities
{
	internal static class Helpers
	{
		/// <summary>
		/// Normalises a directory, replacing all AltDirectorySeparatorChars with DirectorySeparatorChars
		/// and stripping any trailing directory separators.
		/// </summary>
		/// <param name="directory">The directory to be normalised.</param>
		/// <returns>The normalised directory.</returns>
		internal static string NormaliseDirectory(string directory)
		{
			var normalised = directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			var stripped = normalised.TrimEnd(Path.DirectorySeparatorChar);
			return stripped;
		}
		
		/// <summary>
		/// Returns the path of a file relative to a specified root directory.
		/// </summary>
		/// <param name="filespec">The path to the file for which the relative path should be calculated.</param>
		/// <param name="root">The root directory relative to which the relative path should be calculated.</param>
		/// <returns></returns>
		internal static string GetRelativePath(string filespec, string root)
		{
			var pathUri = new Uri(filespec);

			// The directory URI must end with a directory separator char.
			if (!root.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				root += Path.DirectorySeparatorChar;
			}
			var directoryUri = new Uri(root);
			return Uri.UnescapeDataString(directoryUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}

		/// <summary>
		/// Takes an executable name and attempts to resolve it to a full path to that executable.
		/// </summary>
		internal static string ResolveExecutableName(string executable)
		{
			if (executable.Contains(Path.DirectorySeparatorChar) || executable.Contains(Path.AltDirectorySeparatorChar))
			{
				return Path.GetFullPath(executable);
			}
			else
			{
				return FindInPath(executable);
			}
		}

		/// <summary>
		///  Searches all directories in the PATH environment variables for a given executable, returning the first match.
		/// </summary>
		internal static string FindInPath(string fileName)
		{
			// The filename must end with with .exe
			if (!fileName.EndsWith(".exe")) fileName = fileName + ".exe";

			var path = Environment.GetEnvironmentVariable("PATH");
			if (path == null) return null;

			var directories = path.Split(';').Select(p => Path.GetFullPath(p.Trim()));

			foreach (var dir in directories)
			{
				var nameToTest = Path.Combine(dir, fileName);
				if (File.Exists(nameToTest)) return nameToTest;
			}

			return null;
		}

        /// <summary>
        /// Retrieves an <see cref="Exception"/> representing the last Win32
        /// error.
        /// </summary>
        internal static Exception LastWin32Exception()
        {
            return Marshal.GetExceptionForHR(
                Marshal.GetHRForLastWin32Error()
                );
        }
	}
}
