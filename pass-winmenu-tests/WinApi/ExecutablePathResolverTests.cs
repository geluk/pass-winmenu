using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PassWinmenu.WinApi;
using PassWinmenuTests.Utilities;
using Xunit;

namespace PassWinmenuTests.WinApi
{
		public class ExecutablePathResolverTests
	{
		private const string Category = "Windows API: Executable Path Resolver";

		private readonly IFileSystem fileSystem;

		public ExecutablePathResolverTests()
		{
			var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{ "C:\\file.exe", new MockFileData("") },
				{ "C:\\bin\\file.exe", new MockFileData("") }
			},
			"C:\\program");
			fileSystem = mockFs;
		}

		[Fact, TestCategory(Category)]
		public void Resolve_WithDirectorySeparatorChar_UsesGivenLocation()
		{
			var environment = new Mock<IEnvironment>();
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			var location = resolver.Resolve("C:\\file.exe");

			Assert.Equal("C:\\file.exe", location);
		}

		[Fact, TestCategory(Category)]
		public void Resolve_WithRelativePath_ReturnsAbsolutePath()
		{
			var environment = new Mock<IEnvironment>();
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			var location = resolver.Resolve("..\\file.exe");

			Assert.Equal("C:\\file.exe", location);
		}

		[Fact, TestCategory(Category)]
		public void Resolve_NonexistentFile_ThrowsExecutableNotFoundException()
		{
			var environment = new Mock<IEnvironment>();
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			Assert.Throws<ExecutableNotFoundException>(() => resolver.Resolve("C:\\non-existent-file.exe"));
		}

		[Fact, TestCategory(Category)]
		public void Resolve_WithoutDirectorySeparators_GetsLocationFromPath()
		{
			var environment = new Mock<IEnvironment>();
			environment.Setup(e => e.GetEnvironmentVariable("PATH")).Returns("C:\\bin");
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			var location = resolver.Resolve("file.exe");

			environment.Verify(e => e.GetEnvironmentVariable("PATH"), Times.Once);
			Assert.Equal("C:\\bin\\file.exe", location);
		}

		[Fact, TestCategory(Category)]
		public void Resolve_WithoutDirectorySeparatorsAndExtension_GetsLocationFromPath()
		{
			var environment = new Mock<IEnvironment>();
			environment.Setup(e => e.GetEnvironmentVariable("PATH")).Returns("C:\\bin");
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			var location = resolver.Resolve("file");

			environment.Verify(e => e.GetEnvironmentVariable("PATH"), Times.Once);
			Assert.Equal("C:\\bin\\file.exe", location);
		}

		[Fact, TestCategory(Category)]
		public void Resolve_NotInPath_ThrowsExecutableNotFoundException()
		{
			var environment = new Mock<IEnvironment>();
			environment.Setup(e => e.GetEnvironmentVariable("PATH")).Returns("C:\\bin");
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			Assert.Throws<ExecutableNotFoundException>(() => resolver.Resolve("non-existent-file"));
			environment.Verify(e => e.GetEnvironmentVariable("PATH"), Times.Once);
		}

		[Fact, TestCategory(Category)]
		public void Resolve_WithNullPath_ThrowsExecutableNotFoundException()
		{
			var environment = new Mock<IEnvironment>();
			environment.Setup(e => e.GetEnvironmentVariable("PATH")).Returns((string)null);
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			Assert.Throws<ExecutableNotFoundException>(() => resolver.Resolve("file.exe"));
			environment.Verify(e => e.GetEnvironmentVariable("PATH"), Times.Once);
		}

		[Fact, TestCategory(Category)]
		public void Resolve_WithEmptyPath_ThrowsExecutableNotFoundException()
		{
			var environment = new Mock<IEnvironment>();
			environment.Setup(e => e.GetEnvironmentVariable("PATH")).Returns("");
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			Assert.Throws<ExecutableNotFoundException>(() => resolver.Resolve("file.exe"));
		}

		[Fact, TestCategory(Category)]
		public void Resolve_WithInvalidPathEntries_IgnoresThem()
		{
			var environment = new Mock<IEnvironment>();
			environment.Setup(e => e.GetEnvironmentVariable("PATH"))
				.Returns(@";?;;;/bin;  ;%;'';\\invalidNetworkShare;");
			var resolver = new ExecutablePathResolver(fileSystem, environment.Object);

			var location = resolver.Resolve($"file.exe");

			Assert.Equal("C:\\bin\\file.exe", location);
		}

		[Fact, TestCategory(Category)]
		public void Resolve_GetFullPathFails_IgnoresEntry()
		{
			var environment = new Mock<IEnvironment>();
			environment.Setup(e => e.GetEnvironmentVariable("PATH"))
				.Returns(@"fail;/bin");
			var fileSystemMock = new Mock<IFileSystem>();
			fileSystemMock.Setup(f => f.Path.GetFullPath("/bin")).Returns("C:\\bin");
			fileSystemMock.Setup(f => f.Path.GetFullPath("fail")).Throws<Exception>();
			fileSystemMock.Setup(f => f.Path.Combine("C:\\bin", "file.exe")).Returns("C:\\bin\\file.exe");
			fileSystemMock.Setup(f => f.File.Exists(It.IsAny<string>())).Returns(true);
			var resolver = new ExecutablePathResolver(fileSystemMock.Object, environment.Object);

			var location = resolver.Resolve($"file.exe");

			Assert.Equal("C:\\bin\\file.exe", location);
		}
	}
}
