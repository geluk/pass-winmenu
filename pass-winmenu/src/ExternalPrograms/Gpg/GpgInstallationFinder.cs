using System;
using System.IO;
using System.IO.Abstractions;
using PassWinmenu.WinApi;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	internal class GpgInstallationFinder
	{
		private readonly IFileSystem fileSystem;
		private readonly IExecutablePathResolver executablePathResolver;
		private readonly IDirectoryInfo gpgDefaultInstallDir;

		public const string GpgExeName = "gpg.exe";
		public const string GpgAgentExeName = "gpg-agent.exe";
		public const string GpgConnectAgentExeName = "gpg-connect-agent.exe";


		public GpgInstallationFinder(IFileSystem fileSystem, IExecutablePathResolver executablePathResolver)
		{
			this.fileSystem = fileSystem;
			this.executablePathResolver = executablePathResolver;

			gpgDefaultInstallDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\Program Files (x86)\gnupg\bin");
		}

		/// <summary>
		/// Tries to find the GPG installation directory from the given path.
		/// </summary>
		/// <param name="gpgPathSpec">Path to the GPG executable. When set to null,
		/// the default location will be used.</param>
		public GpgInstallation FindGpgInstallation(string gpgPathSpec = null)
		{
			Log.Send("Attempting to detect the GPG installation directory");
			if (gpgPathSpec == string.Empty)
			{
				throw new ArgumentException("The GPG installation path is invalid.");
			}

			if (gpgPathSpec == null)
			{
				Log.Send("No GPG executable path set, assuming GPG to be in its default installation directory.");
				return new GpgInstallation
				{
					InstallDirectory = gpgDefaultInstallDir,
					GpgExecutable = ChildOf(gpgDefaultInstallDir, GpgExeName),
					GpgAgentExecutable = ChildOf(gpgDefaultInstallDir, GpgAgentExeName),
					GpgConnectAgentExecutable = ChildOf(gpgDefaultInstallDir, GpgConnectAgentExeName)
				};
			}

			return ResolveFromPath(gpgPathSpec);
		}

		private GpgInstallation ResolveFromPath(string gpgPathSpec)
		{
			var resolved = executablePathResolver.Resolve(gpgPathSpec);
			var executable = fileSystem.FileInfo.FromFileName(resolved);

			Log.Send("GPG executable found at the configured path. Assuming installation dir to be " + executable.Directory);

			return new GpgInstallation
			{
				InstallDirectory = executable.Directory,
				GpgExecutable = executable,
				GpgAgentExecutable = ChildOf(executable.Directory, GpgAgentExeName),
				GpgConnectAgentExecutable = ChildOf(executable.Directory, GpgConnectAgentExeName)
			};
		}

		private IFileInfo ChildOf(IDirectoryInfo parent, string childName)
		{
			var fullPath = Path.Combine(parent.FullName, childName);
			return fileSystem.FileInfo.FromFileName(fullPath);
		}
	}

	internal class GpgInstallation
	{
		public IDirectoryInfo InstallDirectory { get; set; }
		public IFileInfo GpgExecutable { get; set; }
		public IFileInfo GpgAgentExecutable { get; set; }
		public IFileInfo GpgConnectAgentExecutable { get; set; }
	}
}
