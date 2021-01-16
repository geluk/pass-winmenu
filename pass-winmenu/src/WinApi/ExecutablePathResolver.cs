using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace PassWinmenu.WinApi
{
	internal class ExecutablePathResolver : IExecutablePathResolver
	{
		private readonly IFileSystem fileSystem;
		private readonly IEnvironment environment;

		public ExecutablePathResolver(IFileSystem fileSystem, IEnvironment environment)
		{
			this.fileSystem = fileSystem;
			this.environment = environment;
		}

		/// <summary>
		/// Takes an executable name and attempts to resolve it to a full path to that executable.
		/// </summary>
		public string Resolve(string executable)
		{
			if (executable.Contains(fileSystem.Path.DirectorySeparatorChar) || executable.Contains(fileSystem.Path.AltDirectorySeparatorChar))
			{
				var executablePath = fileSystem.Path.GetFullPath(executable);
				if (fileSystem.File.Exists(executablePath))
				{
					return executablePath;
				}

				throw new ExecutableNotFoundException("Executable not found at specified location.");
			}
			else
			{
				return FindInPath(executable);
			}
		}

		/// <summary>
		///  Searches all directories in the PATH environment variables for a given executable, returning the first match.
		/// </summary>
		private string FindInPath(string fileName)
		{
			// The filename must end with with .exe
			if (!fileName.EndsWith(".exe", StringComparison.Ordinal)) fileName = fileName + ".exe";

			var path = environment.GetEnvironmentVariable("PATH");
			if (path == null)
			{
				throw new ExecutableNotFoundException("PATH appears to be empty.");
			}

			var directories = ParsePathList(path);
			foreach (var dir in directories)
			{
				var nameToTest = fileSystem.Path.Combine(dir, fileName);
				if (fileSystem.File.Exists(nameToTest)) return nameToTest;
			}

			throw new ExecutableNotFoundException("Executable not found in PATH.");
		}

		private IEnumerable<string> ParsePathList(string pathList)
		{
			var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
			foreach (var path in pathList.Split(';'))
			{
				if (string.IsNullOrWhiteSpace(path)) continue;

				if (path.Any(c => invalidPathChars.Contains(c)))
				{
					Log.Send($"PATH location \"{path}\" contains invalid characters.", LogLevel.Warning);
					continue;
				}

				string fullPath;
				try
				{
					fullPath = fileSystem.Path.GetFullPath(path);
				}
				catch (Exception e)
				{
					Log.Send($"PATH location \"{path}\" appears to be invalid ({e.GetType().Name}: {e.Message}).", LogLevel.Warning);
					continue;
				}

				yield return fullPath;
			}
		}
	}
}
