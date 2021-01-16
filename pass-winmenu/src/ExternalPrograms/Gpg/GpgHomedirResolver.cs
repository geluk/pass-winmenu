using System;
using System.IO.Abstractions;
using PassWinmenu.Configuration;
using PassWinmenu.WinApi;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	class GpgHomedirResolver : IGpgHomedirResolver
	{
		private const string defaultHomeDirName = "gnupg";
		private const string homedirEnvironmentVariableName = "GNUPGHOME";

		private readonly GpgConfig config;
		private readonly IEnvironment environment;
		private readonly IFileSystem fileSystem;

		public GpgHomedirResolver(GpgConfig config, IEnvironment environment, IFileSystem fileSystem)
		{
			this.config = config;
			this.environment = environment;
			this.fileSystem = fileSystem;
		}

		/// <summary>
		/// Returns the path GPG will use as its home directory.
		/// </summary>
		/// <returns></returns>
		public string GetHomeDir() => GetConfiguredHomeDir() ?? GetDefaultHomeDir();

		/// <summary>
		/// Returns the home directory as configured by the user, or null if no home directory has been defined.
		/// </summary>
		/// <returns></returns>
		public string GetConfiguredHomeDir()
		{
			if (config.GnupghomeOverride != null)
			{
				return config.GnupghomeOverride;
			}
			return environment.GetEnvironmentVariable(homedirEnvironmentVariableName);
		}

		/// <summary>
		/// Returns the default home directory used by GPG when no user-defined home directory is available.
		/// </summary>
		/// <returns></returns>
		public string GetDefaultHomeDir()
		{
			var appData = environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			return fileSystem.Path.Combine(appData, defaultHomeDirName);
		}
	}
}
