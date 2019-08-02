using System;
using System.IO.Abstractions.TestingHelpers;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgHomedirResolverTests
	{
		[Fact]
		public void Resolve_NullHomeOverrideAndNoEnvVarSet_ReturnsDefaultLocation()
		{
			var config = new GpgConfig {GnupghomeOverride = null};
			var environment = StubEnvironment.Create()
				.WithSpecialFolder(Environment.SpecialFolder.ApplicationData, @"C:\Users\Test\AppData")
				.Build();
			var resolver = new GpgHomedirResolver(config, environment, new MockFileSystem());

			var homeDir = resolver.GetHomeDir();

			homeDir.ShouldBe(@"C:\Users\Test\AppData\gnupg");
		}

		[Fact]
		public void Resolve_NullHomeOverrideWithEnvVarSet_ReturnsEnvVarPath()
		{
			var config = new GpgConfig { GnupghomeOverride = null };
			var environment = StubEnvironment.Create()
				.WithSpecialFolder(Environment.SpecialFolder.ApplicationData, @"C:\Users\Test\AppData")
				.WithEnvironmentVariable("GNUPGHOME", @"C:\gpg")
				.Build();
			var resolver = new GpgHomedirResolver(config, environment, new MockFileSystem());

			var homeDir = resolver.GetHomeDir();

			homeDir.ShouldBe(@"C:\gpg");
		}

		[Fact]
		public void Resolve_HomeOverrideSet_ReturnsHomeOverride()
		{
			var config = new GpgConfig { GnupghomeOverride = @"C:\Users\Test\.gpg" };
			var environment = StubEnvironment.Create()
				.WithSpecialFolder(Environment.SpecialFolder.ApplicationData, @"C:\Users\Test\AppData")
				.WithEnvironmentVariable("GNUPGHOME", @"C:\gpg")
				.Build();
			var resolver = new GpgHomedirResolver(config, environment, new MockFileSystem());

			var homeDir = resolver.GetHomeDir();

			homeDir.ShouldBe(@"C:\Users\Test\.gpg");
		}
	}
}
