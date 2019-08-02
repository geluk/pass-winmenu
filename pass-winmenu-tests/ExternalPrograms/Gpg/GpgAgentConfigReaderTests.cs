using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgAgentConfigReaderTests
	{
		[Fact]
		public void ReadConfigLines_ReadsLinesFromConfigFile()
		{
			var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{@"C:\gpg\gpg-agent.conf", new MockFileData("key1=value1\nkey2=value2")}
			});
			var resolver = new StubHomedirResolver(@"C:\gpg");
			var reader = new GpgAgentConfigReader(fileSystem, resolver);

			var lines = reader.ReadConfigLines();

			lines.ShouldBe(new[] {"key1=value1", "key2=value2"});
		}

		[Fact]
		public void ReadConfigLines_NonexistentConfigFile_CreatesFileAndReturnsEmptyList()
		{
			var fileSystem = new MockFileSystem();
			fileSystem.AddDirectory(@"C:\gpg");
			var resolver = new StubHomedirResolver(@"C:\gpg");
			var reader = new GpgAgentConfigReader(fileSystem, resolver);

			var lines = reader.ReadConfigLines();

			fileSystem.File.Exists(@"C:\gpg\gpg-agent.conf").ShouldBeTrue();
			lines.ShouldBe(new string[0]);
		}

		[Fact]
		public void ReadConfigLines_NonexistentHomedir_ThrowsDirectoryNotFoundException()
		{
			var fileSystem = new MockFileSystem();
			var resolver = new StubHomedirResolver(@"C:\gpg");
			var reader = new GpgAgentConfigReader(fileSystem, resolver);

			Should.Throw<DirectoryNotFoundException>(() => reader.ReadConfigLines());
		}

		[Fact]
		public void WriteConfigLines_NonexistentConfigFile_CreatesAndWritesLines()
		{
			var fileSystem = new MockFileSystem();
			fileSystem.AddDirectory(@"C:\gpg");
			var resolver = new StubHomedirResolver(@"C:\gpg");
			var reader = new GpgAgentConfigReader(fileSystem, resolver);
			var linesToWrite = new[] {"key1=value1", "key2=value2", "#comment"};

			reader.WriteConfigLines(linesToWrite);

			fileSystem.ShouldSatisfyAllConditions(
				() => fileSystem.File.Exists(@"C:\gpg\gpg-agent.conf").ShouldBeTrue(),
				() => fileSystem.File.ReadAllLines(@"C:\gpg\gpg-agent.conf").ShouldBe(linesToWrite)
			);
		}

		[Fact]
		public void WriteConfigLines_ExistingConfigFile_OverwritesContent()
		{
			var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
			{
				@"C:\gpg\gpg-agent.conf", new MockFileData("key1=value1\nkey2=value2")
			} });
			var resolver = new StubHomedirResolver(@"C:\gpg");
			var reader = new GpgAgentConfigReader(fileSystem, resolver);
			var linesToWrite = new[] { "key3=value3", "key4=value4", "#comment" };

			reader.WriteConfigLines(linesToWrite);

			fileSystem.File.ReadAllLines(@"C:\gpg\gpg-agent.conf").ShouldBe(linesToWrite);
		}
	}
}
