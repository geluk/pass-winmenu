using PassWinmenu.Configuration;
using PassWinmenu.PasswordGeneration;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.PasswordGeneration
{
	public class PasswordGeneratorTests
	{
		private const string Category = "Core: Password Generation";

		[Fact, TestCategory(Category)]
		public void GeneratePassword_MatchesRequiredLength()
		{
			var options = new PasswordGenerationConfig
			{
				Length = 32
			};
			using (var generator = new PasswordGenerator(options))
			{
				var password = generator.GeneratePassword(options.Length);

				password.Length.ShouldBe(32);
			}

		}

		[Fact, TestCategory(Category)]
		public void GeneratePassword_NoCharacterGroups_Null()
		{
			var options = new PasswordGenerationConfig
			{
				Length = 32,
				CharacterGroups = new CharacterGroupConfig[0]
			};
			using (var generator = new PasswordGenerator(options))
			{
				var password = generator.GeneratePassword(options.Length);

				password.ShouldBeNull();
			}
		}

		[Theory, TestCategory(Category)]
		[InlineData("0123456789")]
		[InlineData("abcABC")]
		[InlineData("1")]
		public void GeneratePassword_OnlyContainsAllowedCharacters(string allowedCharacters)
		{
			var options = new PasswordGenerationConfig
			{
				Length = 32,
				CharacterGroups = new []
				{
					new CharacterGroupConfig("test", allowedCharacters, true), 
				}
			};
			using (var generator = new PasswordGenerator(options))
			{
				var password = generator.GeneratePassword(options.Length);

				password.ShouldBeSubsetOf(allowedCharacters);
			}
		}
	}
}
