using System;
using Moq;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.WinApi;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Git
{
	public class GitSyncStrategiesTests
	{
		[Theory]
		[InlineData("native-git", typeof(NativeGitSyncStrategy))]
		[InlineData("builtin", typeof(LibGit2SharpSyncStrategy))]
		public void ChooseSyncStrategy_ChoosesMatchingStrategy(string syncMode, Type type)
		{
			var resolver = new Mock<IExecutablePathResolver>();
			var strategies = new GitSyncStrategies(resolver.Object);
			var config = new GitConfig
			{
				SyncModeString = syncMode,
			};

			var strategy = strategies.ChooseSyncStrategy("test", null, config);

			strategy.ShouldBeOfType(type);
		}

		[Fact]
		public void ChooseSyncStrategy_AutoAndGitFound_ChoosesNativeGit()
		{
			var resolver = new Mock<IExecutablePathResolver>();
			var strategies = new GitSyncStrategies(resolver.Object);
			var config = new GitConfig
			{
				SyncModeString = "auto",
			};

			var strategy = strategies.ChooseSyncStrategy("test", null, config);

			strategy.ShouldBeOfType<NativeGitSyncStrategy>();
		}

		[Fact]
		public void ChooseSyncStrategy_AutoAndGitNotFound_ChoosesLibGit2Sharp()
		{
			var resolver = new Mock<IExecutablePathResolver>();
			resolver.Setup(r => r.Resolve(It.IsAny<string>())).Throws(new ExecutableNotFoundException("Executable not found"));
			var strategies = new GitSyncStrategies(resolver.Object);
			var config = new GitConfig
			{
				SyncModeString = "auto",
			};

			var strategy = strategies.ChooseSyncStrategy("test", null, config);

			strategy.ShouldBeOfType<LibGit2SharpSyncStrategy>();
		}
	}
}
