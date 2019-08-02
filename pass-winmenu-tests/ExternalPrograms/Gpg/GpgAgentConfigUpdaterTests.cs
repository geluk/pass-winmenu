using System;
using System.Collections.Generic;
using Moq;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgAgentConfigUpdaterTests
	{
		[Fact]
		public void Update_EmptyCollection_WritesKeysToSet()
		{
			var reader = new FakeGpgAgentConfigReader(new string[0]);
			var updater = new GpgAgentConfigUpdater(reader);
			var keysToSet = new Dictionary<string, string>
			{
				{"key_1", "value_1"},
				{"key_2", "value_2"},
			};

			updater.UpdateAgentConfig(keysToSet);
			var lines = reader.ReadConfigLines();

			lines.ShouldBe(new[]
			{
				GpgAgentConfigUpdater.ManagedByPassWinmenuComment,
				"key_1 value_1",
				GpgAgentConfigUpdater.ManagedByPassWinmenuComment,
				"key_2 value_2"
			});
		}

		[Fact]
		public void Update_KeysAlreadyExistWithRequiredValues_DoesNotWriteKeys()
		{
			var readerMock = new Mock<IGpgAgentConfigReader>();
			readerMock.Setup(r => r.ReadConfigLines()).Returns(new[]
			{
				"key_1 value_1",
				"key_2 value_2"
			});
			var updater = new GpgAgentConfigUpdater(readerMock.Object);
			var keysToSet = new Dictionary<string, string>
			{
				{"key_1", "value_1"},
				{"key_2", "value_2"},
			};

			updater.UpdateAgentConfig(keysToSet);

			readerMock.Verify(m => m.WriteConfigLines(It.IsAny<string[]>()), Times.Never);
		}

		[Fact]
		public void Update_ConfigContainsUnrelatedKeys_AddsNewKeysAfterThem()
		{
			var reader = new FakeGpgAgentConfigReader(new[]
			{
				"key_3 value_3",
				"key_4 value_4"
			});
			var updater = new GpgAgentConfigUpdater(reader);
			var keysToSet = new Dictionary<string, string>
			{
				{"key_1", "value_1"},
				{"key_2", "value_2"},
			};

			updater.UpdateAgentConfig(keysToSet);
			var lines = reader.ReadConfigLines();

			lines.ShouldBe(new[]
			{
				"key_3 value_3",
				"key_4 value_4",
				GpgAgentConfigUpdater.ManagedByPassWinmenuComment,
				"key_1 value_1",
				GpgAgentConfigUpdater.ManagedByPassWinmenuComment,
				"key_2 value_2"
			});
		}

		[Fact]
		public void Update_KeysExistWithWrongValue_OverwritesThem()
		{
			var reader = new FakeGpgAgentConfigReader(new[]
			{
				"key_1 value_2",
				"key_2 value_1"
			});

			var updater = new GpgAgentConfigUpdater(reader);
			var keysToSet = new Dictionary<string, string>
			{
				{"key_1", "value_1"},
				{"key_2", "value_2"},
			};

			updater.UpdateAgentConfig(keysToSet);
			var lines = reader.ReadConfigLines();

			lines.ShouldBe(new[]
			{
				GpgAgentConfigUpdater.ManagedByPassWinmenuComment,
				"key_1 value_1",
				GpgAgentConfigUpdater.ManagedByPassWinmenuComment,
				"key_2 value_2"
			});
		}

		[Fact]
		public void Update_FileContainsNonKeyLines_PreservesThem()
		{
			var reader = new FakeGpgAgentConfigReader(new[]
			{
				"#this is a comment",
				"key_1 value_2",
				"",
				"key_2 value_2"
			});

			var updater = new GpgAgentConfigUpdater(reader);
			var keysToSet = new Dictionary<string, string>
			{
				{"key_1", "value_1"},
				{"key_3", "value_3"},
			};

			updater.UpdateAgentConfig(keysToSet);
			var lines = reader.ReadConfigLines();

			lines.ShouldBe(new[]
			{
				"#this is a comment",
				GpgAgentConfigUpdater.ManagedByPassWinmenuComment,
				"key_1 value_1",
				"",
				"key_2 value_2",
				GpgAgentConfigUpdater.ManagedByPassWinmenuComment,
				"key_3 value_3"
			});
		}

		[Fact]
		public void Update_ReaderThrowsOnRead_SwallowsException()
		{
			var readerMock = new Mock<IGpgAgentConfigReader>();
			readerMock.Setup(r => r.ReadConfigLines()).Throws<Exception>();
			var updater = new GpgAgentConfigUpdater(readerMock.Object);

			Should.NotThrow(() => updater.UpdateAgentConfig(new Dictionary<string, string>()));
		}

		[Fact]
		public void Update_ReaderThrowsOnWrite_SwallowsException()
		{
			var readerMock = new Mock<IGpgAgentConfigReader>();
			readerMock.Setup(r => r.ReadConfigLines()).Returns(new string[0]);
			readerMock.Setup(r => r.WriteConfigLines(It.IsAny<string[]>())).Throws<Exception>();
			var updater = new GpgAgentConfigUpdater(readerMock.Object);

			Should.NotThrow(() => updater.UpdateAgentConfig(new Dictionary<string, string> {{"key", "value"}}));
		}
	}
}
