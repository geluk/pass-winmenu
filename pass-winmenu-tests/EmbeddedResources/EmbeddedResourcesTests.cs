using PassWinmenuTests.Utilities;
using Xunit;

namespace PassWinmenuTests.EmbeddedResources
{
		public class EmbeddedResourcesTests
	{
		private const string Category = "Core: Embedded resources";

		[Fact, TestCategory(Category)]
		public void EmbeddedResources_ContainsVersionString()
		{
			PassWinmenu.EmbeddedResources.Load();
			Assert.False(string.IsNullOrWhiteSpace(PassWinmenu.EmbeddedResources.Version));
		}
	}
}
