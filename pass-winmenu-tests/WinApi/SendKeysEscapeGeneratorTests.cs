using PassWinmenu.WinApi;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.WinApi
{
	public class SendKeysEscapeGeneratorTests
	{
		[Theory]
		[InlineData("text")]
		[InlineData("text and 14 numbers")]
		[InlineData(@"_these should not be escaped/\")]
		[InlineData(@"ünicøde 文字")]
		public void Escape_NoSpecialSequence_SameText(string text)
		{
			var generator = new SendKeysEscapeGenerator();

			var escaped = generator.Escape(text, false);

			escaped.ShouldBe(text);
		}

		[Theory]
		[InlineData("{", "{{}")]
		[InlineData("%text and+ _special_ characters~", "{%}text and{+} _special_ characters{~}")]
		[InlineData("+^%~(){}[]", "{+}{^}{%}{~}{(}{)}{{}{}}{[}{]}")]
		public void Escape_SpecialSequence_Escapes(string text, string expectedEscape)
		{
			var generator = new SendKeysEscapeGenerator();

			var escaped = generator.Escape(text, false);

			escaped.ShouldBe(expectedEscape);
		}

		[Theory]
		[InlineData("`","` ")]
		[InlineData("\"what's a dead key?\"", "\" what' s a dead key?\" ")] // See https://en.wikipedia.org/wiki/Dead_key
		[InlineData("`~^'\"", "` {~} {^} ' \" ")]
		public void Escape_DeadKeys_EscapesDeadKeys(string text, string expectedEscape)
		{
			var generator = new SendKeysEscapeGenerator();

			var escaped = generator.Escape(text, true);

			escaped.ShouldBe(expectedEscape);
		}

		[Fact]
		public void Escape_DeadKeys_DoesNotEscapeRegularKeys()
		{
			var text = "do not escape these: !@#$.";
			var generator = new SendKeysEscapeGenerator();

			var escaped = generator.Escape(text, true);

			escaped.ShouldBe(text);
		}

		[Theory]
		[InlineData("don't do anything `here")]
		public void Escape_NoDeadKeys_IgnoresDeadKeys(string text)
		{
			var generator = new SendKeysEscapeGenerator();

			var escaped = generator.Escape(text, false);

			escaped.ShouldBe(text);
		}
	}
}
