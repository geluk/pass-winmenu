using System;
using System.IO;
using System.Linq;

namespace PassWinmenu.Utilities
{
	internal class DirectoryAutocomplete
	{
		private readonly string baseDirectory;

		public DirectoryAutocomplete(string baseDirectory)
		{
			// Ensure consistency of directory separators.
			// We can't use Path.Combine() here because it doesn't concatenate drive letters properly.
			this.baseDirectory = string.Join(Path.DirectorySeparatorChar.ToString(), baseDirectory.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries));
			// Append a directory separator so Path.GetDirectoryName(baseDirectory) will correctly return the full base directory, instead of its parent directory.
			this.baseDirectory = this.baseDirectory + Path.DirectorySeparatorChar;
		}

		public string[] GetCompletionList(string input)
		{
			// Ensure the directory separators in the input string are correct
			input = Path.Combine(input.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries));

			var fullPath = Path.Combine(baseDirectory, input);
			var directory = Path.GetDirectoryName(fullPath);
			var file = Path.GetFileName(fullPath);
			
			// If the directory to look in doesn't exist, we can't suggest anything for it.
			if (!Directory.Exists(directory))
			{
				return new string[0];
			}
			
			// Dotfiles should be filtered out.
			var suggestions = Directory.GetFileSystemEntries(directory, file + "*")
				.Where(suggestion => !Path.GetFileName(suggestion).StartsWith("."));

			// If we have no suggestions, try showing suggestions for just the parent directory.
			if (!suggestions.Any())
			{
				suggestions = Directory.GetFileSystemEntries(directory, "*")
					.Where(suggestion => !Path.GetFileName(suggestion).StartsWith("."));
			}
			// If we have only one suggestion and that suggestion is a directory, 
			// add suggestions for the files inside that directory.
			else if (suggestions.Count() == 1 && Directory.Exists(suggestions.First()))
			{
				// Again, ignoring dotfiles.
				suggestions = suggestions.Concat(Directory.GetFileSystemEntries(suggestions.First())
					.Where(suggestion => !Path.GetFileName(suggestion).StartsWith(".")));
			}
			// Append a directory separator char to all directories to make it clear we're suggesting a directory, not a file.
			suggestions = suggestions.Select(suggestion => Directory.Exists(suggestion) ? suggestion + Path.DirectorySeparatorChar : suggestion);

			// Finally, transform directory suggestions to relative paths for convenience.
			return suggestions.Select(suggestion => MakeRelativePath(baseDirectory, suggestion)).ToArray();
		}

		private string MakeRelativePath(string baseDir, string absoluteDir)
		{
			if (!baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				baseDir = baseDir + Path.DirectorySeparatorChar;
			}
			if (string.IsNullOrEmpty(baseDir)) throw new ArgumentNullException(nameof(baseDir));
			if (string.IsNullOrEmpty(absoluteDir)) throw new ArgumentNullException(nameof(absoluteDir));

			var baseUri = new Uri(baseDir);
			var absoluteUri = new Uri(absoluteDir);

			if (baseUri.Scheme != absoluteUri.Scheme) { return absoluteDir; } // path can't be made relative.

			var relativeUri = baseUri.MakeRelativeUri(absoluteUri);
			var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			return relativePath;
		}
	}
}
