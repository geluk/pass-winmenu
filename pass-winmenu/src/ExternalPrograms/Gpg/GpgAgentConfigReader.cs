using System.IO;
using System.IO.Abstractions;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	class GpgAgentConfigReader : IGpgAgentConfigReader
	{
		private const string gpgAgentConfigFileName = "gpg-agent.conf";

		private readonly IFileSystem fileSystem;
		private readonly IGpgHomedirResolver homedirResolver;

		public GpgAgentConfigReader(IFileSystem fileSystem, IGpgHomedirResolver homedirResolver)
		{
			this.fileSystem = fileSystem;
			this.homedirResolver = homedirResolver;
		}

		public string[] ReadConfigLines()
		{
			var homeDir = GetHomeDir();
			var agentConf = fileSystem.Path.Combine(homeDir, gpgAgentConfigFileName);

			if (fileSystem.File.Exists(agentConf))
			{
				return fileSystem.File.ReadAllLines(agentConf);
			}

			using (fileSystem.File.Create(agentConf))
			{
				return new string[0];
			}
		}

		public void WriteConfigLines(string[] lines)
		{
			var homeDir = GetHomeDir();

			var agentConf = fileSystem.Path.Combine(homeDir, gpgAgentConfigFileName);

			fileSystem.File.WriteAllLines(agentConf, lines);
		}

		private string GetHomeDir()
		{
			var homeDir = homedirResolver.GetHomeDir();

			if (fileSystem.Directory.Exists(homeDir))
			{
				return homeDir;
			}

			throw new DirectoryNotFoundException("GPG Homedir does not exist");
		}
	}
}
