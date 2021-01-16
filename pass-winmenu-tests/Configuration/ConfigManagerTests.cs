using System.IO;
using PassWinmenu.Configuration;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.Configuration
{
	public class ConfigManagerTests
	{
		private const string Category = "Integration: Configuration File";

		[Fact, TestCategory(Category)]
		public void Load_EmptyFile_NeedsUpgrade()
		{
			var tempFile = Path.GetTempFileName();
			var result = ConfigManager.Load(tempFile);
			File.Delete(tempFile);

			result.ShouldBe(LoadResult.NeedsUpgrade);
		}

		[Fact, TestCategory((Category))]
		public void Load_NonexistentFile_Created()
		{
			var tempFile = Path.GetTempFileName();
			File.Delete(tempFile);

			var result = ConfigManager.Load(tempFile);
			File.Delete(tempFile);

			result.ShouldBe(LoadResult.NewFileCreated);
		}
	}
}
