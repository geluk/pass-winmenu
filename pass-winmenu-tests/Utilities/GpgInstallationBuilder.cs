using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using PassWinmenu.ExternalPrograms.Gpg;

namespace PassWinmenuTests.Utilities
{
	internal class GpgInstallationBuilder
	{

		public GpgInstallation Build()
		{
			var fileSystem = new MockFileSystemBuilder().Build();
			return new GpgInstallation
			{
				InstallDirectory = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\gpg\bin"),
				GpgExecutable = fileSystem.FileInfo.FromFileName(@"C:\gpg\bin\gpg.exe"),
				GpgAgentExecutable = fileSystem.FileInfo.FromFileName(@"C:\gpg\bin\gpg-agent.exe"),
				GpgConnectAgentExecutable = fileSystem.FileInfo.FromFileName(@"C:\gpg\bin\gpg-connect-agent.exe")
			};
		}
	}
}
