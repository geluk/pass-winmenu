using PassWinmenu.Configuration;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.Configuration
{
	public class RuntimeConfigurationTests
	{
		[Fact]
		public void Parse_NoArgs_ConfigFileLocationIsNull()
		{
			var config = RuntimeConfiguration.Parse(new[] { "app.exe" });

			config.ConfigFileLocation.ShouldBeNull();
		}

		[Fact]
		public void Parse_InvalidArgs_ThrowsRuntimeConfigurationError()
		{
			Should.Throw<RuntimeConfigurationError>(() => RuntimeConfiguration.Parse(new[] { "app.exe", "--not-an-option" }));
		}

		[Fact]
		public void Parse_MissingArg_ThrowsRuntimeConfigurationError()
		{
			Should.Throw<RuntimeConfigurationError>(() => RuntimeConfiguration.Parse(new[] { "app.exe", "--config-file" }));
		}

		[Fact]
		public void Parse_ValidArg_SetsConfigFileLocation()
		{
			var config = RuntimeConfiguration.Parse(new[]
			{
				"app.exe",
				"--config-file",
				"test.yaml"
			});

			config.ConfigFileLocation.ShouldBe("test.yaml");
		}
	}
}
