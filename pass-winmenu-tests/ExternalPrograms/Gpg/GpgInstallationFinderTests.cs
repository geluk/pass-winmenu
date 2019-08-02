using System;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenu.WinApi;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgInstallationFinderTests
	{
		private const string Category = "External Programs: Gpg";

		[Fact, TestCategory(Category)]
		public void FindGpgInstallation_AbsolutePath_FindsInstallation()
		{
			var resolverMock = new Mock<IExecutablePathResolver>();
			resolverMock.Setup(r => r.Resolve(@"C:\Gpg\gpg.exe")).Returns(@"C:\Gpg\gpg.exe");
			var fs = new MockFileSystem();
			var finder = new GpgInstallationFinder(fs, resolverMock.Object);

			var installation = finder.FindGpgInstallation(@"C:\Gpg\gpg.exe");

			installation.ShouldSatisfyAllConditions(
				() => installation.InstallDirectory.FullName.ShouldBe(@"C:\Gpg"),
				() => installation.GpgExecutable.FullName.ShouldBe(@"C:\Gpg\gpg.exe"),
				() => installation.GpgAgentExecutable.FullName.ShouldBe(@"C:\Gpg\gpg-agent.exe"),
				() => installation.GpgConnectAgentExecutable.FullName.ShouldBe(@"C:\Gpg\gpg-connect-agent.exe")
			);
		}

		[Fact, TestCategory(Category)]
		public void FindGpgInstallation_EmptyPath_ThrowsArgumentException()
		{
			var resolverMock = new Mock<IExecutablePathResolver>();
			var fs = new MockFileSystem();
			var finder = new GpgInstallationFinder(fs, resolverMock.Object);

			Should.Throw<ArgumentException>(() =>finder.FindGpgInstallation(@""));

		}

		[Fact, TestCategory(Category)]
		public void FindGpgInstallation_FileNotFound_ThrowsArgumentException()
		{
			var resolverMock = new Mock<IExecutablePathResolver>();
			resolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Throws<ExecutableNotFoundException>();
			var fs = new MockFileSystem();
			var finder = new GpgInstallationFinder(fs, resolverMock.Object);

			Should.Throw<ExecutableNotFoundException>(() => finder.FindGpgInstallation("C:\\Gpg\\gpg.exe"));
		}

		[Fact, TestCategory(Category)]
		public void FindGpgInstallation_NullExePath_ReturnsDefaultLocation()
		{
			var resolverMock = new Mock<IExecutablePathResolver>();
			var fs = new MockFileSystem();
			var finder = new GpgInstallationFinder(fs, resolverMock.Object);

			var installation = finder.FindGpgInstallation();

			installation.ShouldSatisfyAllConditions(
				() => installation.InstallDirectory.FullName.ShouldBe(@"C:\Program Files (x86)\gnupg\bin"),
				() => installation.GpgExecutable.FullName.ShouldBe(@"C:\Program Files (x86)\gnupg\bin\gpg.exe"),
				() => installation.GpgAgentExecutable.FullName.ShouldBe(@"C:\Program Files (x86)\gnupg\bin\gpg-agent.exe"),
				() => installation.GpgConnectAgentExecutable.FullName.ShouldBe(@"C:\Program Files (x86)\gnupg\bin\gpg-connect-agent.exe")
				);
		}
	}
}
