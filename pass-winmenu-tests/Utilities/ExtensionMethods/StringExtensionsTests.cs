using System;
using PassWinmenu.Utilities.ExtensionMethods;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.Utilities.ExtensionMethods
{
	public class StringExtensionsTests
	{
		[Fact]
		public void RemoveEnd_DoesNotEndWithStringToRemove_ThrowsArgumentException()
		{
			var baseString = "base string";
			var toRemove = "remove";

			Should.Throw<ArgumentException>(() => baseString.RemoveEnd(toRemove));
		}

		[Fact]
		public void RemoveEnd_EmptyBaseString_ThrowsArgumentException()
		{
			var baseString = "";
			var toRemove = "remove";

			Should.Throw<ArgumentException>(() => baseString.RemoveEnd(toRemove));
		}
	}
}
