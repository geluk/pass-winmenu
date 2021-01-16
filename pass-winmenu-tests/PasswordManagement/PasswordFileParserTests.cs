using System.IO.Abstractions.TestingHelpers;
using PassWinmenu.Configuration;
using PassWinmenu.PasswordManagement;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.PasswordManagement
{
	public class PasswordFileParserTests
	{
		private const string Category = "Core: Password File Parsing";
		private readonly PasswordFile dummyFile;
		private PasswordFileParser p = new PasswordFileParser(new UsernameDetectionConfig());

		public PasswordFileParserTests()
		{
			var fileSystem = new MockFileSystem();
			var dirInfo = new MockDirectoryInfo(fileSystem, "\\password-store");
			var fileInfo = new MockFileInfo(fileSystem, "\\password-store\\dummy-password");
			dummyFile = new PasswordFile(fileInfo, dirInfo);
		}

		[Theory]
		[InlineData(-999, false)]
		[InlineData(-1, false)]
		[InlineData(-0, false)]
		[InlineData(1, false)]
		[InlineData(2, true)]
		[InlineData(3, true)]
		[InlineData(4, true)]
		[InlineData(100, true)]
		public void GetUsername_LineNumberInvalid_ThrowsException(int lineNumber, bool isValid)
		{
			var passwordFile = new ParsedPasswordFile(new PasswordFile(null, null), "password", "username");
			var parser = new PasswordFileParser(new UsernameDetectionConfig
			{
				MethodString = "line-number",
				Options = new UsernameDetectionOptions
				{
					LineNumber = lineNumber
				}
			});

			if (isValid)
			{
				Should.NotThrow(() => parser.GetUsername(passwordFile));
			}
			else
			{
				Should.Throw<PasswordParseException>(() => parser.GetUsername(passwordFile));
			}
		}

		[Fact, TestCategory(Category)]
		public void Test_EmptyFile()
		{
			var text = "";
			var parsed = p.Parse(dummyFile, text, false);

			Assert.Equal(parsed.Password, string.Empty);
			Assert.Equal(parsed.Metadata, string.Empty);
		}

		[Fact, TestCategory(Category)]
		public void Test_LineEndings_Metadata()
		{
			var crlf = "password\r\nmeta-data";
			var cr = "password\rmeta-data";
			var lf = "password\nmeta-data";

			var parsedCrlf = p.Parse(dummyFile, crlf, false);
			Assert.Equal("password", parsedCrlf.Password);
			Assert.Equal("meta-data", parsedCrlf.Metadata);

			var parsedCr = p.Parse(dummyFile, cr, false);
			Assert.Equal("password", parsedCr.Password);
			Assert.Equal("meta-data", parsedCr.Metadata);

			var parsedLf = p.Parse(dummyFile, lf, false);
			Assert.Equal("password", parsedLf.Password);
			Assert.Equal("meta-data", parsedLf.Metadata);
		}

		[Fact, TestCategory(Category)]
		public void Test_LineEndings_PasswordOnly()
		{
			var crlf = "password\r\n";
			var cr = "password\r";
			var lf = "password\n";
			var none = "password";

			var parsedCrlf = p.Parse(dummyFile, crlf, false);
			Assert.Equal("password", parsedCrlf.Password);
			Assert.Equal(parsedCrlf.Metadata, string.Empty);

			var parsedCr = p.Parse(dummyFile, cr, false);
			Assert.Equal("password", parsedCr.Password);
			Assert.Equal(parsedCr.Metadata, string.Empty);

			var parsedLf = p.Parse(dummyFile, lf, false);
			Assert.Equal("password", parsedLf.Password);
			Assert.Equal(parsedLf.Metadata, string.Empty);

			var parsedNone = p.Parse(dummyFile, none, false);
			Assert.Equal("password", parsedNone.Password);
			Assert.Equal(parsedNone.Metadata, string.Empty);
		}

		[Fact, TestCategory(Category)]
		public void Test_Metadata_LineEndings()
		{
			const string crlf = "password\r\n" +
			                    "Username: user\r\n" +
			                    "Key: value";
			const string cr = "password\r" +
			                  "Username: user\r" +
			                  "Key: value";
			const string lf = "password\n" +
			                  "Username: user\n" +
			                  "Key: value";
			const string mixed = "password\r\n" +
			                     "Username: user\n" +
			                     "Key: value\r";

			var parsedCrlf = p.Parse(dummyFile, crlf, false);
			Assert.True(parsedCrlf.Keys[0].Key == "Username");
			Assert.True(parsedCrlf.Keys[0].Value == "user");
			Assert.True(parsedCrlf.Keys[1].Key == "Key");
			Assert.True(parsedCrlf.Keys[1].Value == "value");

			var parsedCr = p.Parse(dummyFile, cr, false);
			Assert.True(parsedCr.Keys[0].Key == "Username");
			Assert.True(parsedCr.Keys[0].Value == "user");
			Assert.True(parsedCr.Keys[1].Key == "Key");
			Assert.True(parsedCr.Keys[1].Value == "value");

			var parsedLf = p.Parse(dummyFile, lf, false);
			Assert.True(parsedLf.Keys[0].Key == "Username");
			Assert.True(parsedLf.Keys[0].Value == "user");
			Assert.True(parsedLf.Keys[1].Key == "Key");
			Assert.True(parsedLf.Keys[1].Value == "value");

			var parsedMixed = p.Parse(dummyFile, mixed, false);
			Assert.True(parsedMixed.Keys[0].Key == "Username");
			Assert.True(parsedMixed.Keys[0].Value == "user");
			Assert.True(parsedMixed.Keys[1].Key == "Key");
			Assert.True(parsedMixed.Keys[1].Value == "value");
		}

		[Fact, TestCategory(Category)]
		public void Test_Metadata_KeyFormat()
		{
			var duplicate = "password\r\n" +
			                "Username: user\r\n" +
			                "With-Dash: value\r\n" +
			                "_WithUnderline: value2\r\n";

			var parsed = p.Parse(dummyFile, duplicate, false);

			Assert.True(parsed.Keys[0].Key == "Username");
			Assert.True(parsed.Keys[0].Value == "user");
			Assert.True(parsed.Keys[1].Key == "With-Dash");
			Assert.True(parsed.Keys[1].Value == "value");
			Assert.True(parsed.Keys[2].Key == "_WithUnderline");
			Assert.True(parsed.Keys[2].Value == "value2");
		}

		[Fact, TestCategory(Category)]
		public void Test_Metadata_Multiple_Keys()
		{
			var duplicate = "password\r\n" +
			                "Username: user\r\n" +
			                "Duplicate: value1\r\n" +
			                "Duplicate: value2\r\n" +
			                "Duplicate: value3\r\n";

			var parsed = p.Parse(dummyFile, duplicate, false);

			Assert.True(parsed.Keys[0].Key == "Username");
			Assert.True(parsed.Keys[0].Value == "user");
			Assert.True(parsed.Keys[1].Key == "Duplicate");
			Assert.True(parsed.Keys[1].Value == "value1");
			Assert.True(parsed.Keys[2].Key == "Duplicate");
			Assert.True(parsed.Keys[2].Value == "value2");
			Assert.True(parsed.Keys[3].Key == "Duplicate");
			Assert.True(parsed.Keys[3].Value == "value3");
		}
	}
}
