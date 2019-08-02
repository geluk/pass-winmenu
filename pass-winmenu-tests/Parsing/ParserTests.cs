using System.IO;
using System.IO.Abstractions.TestingHelpers;
using PassWinmenu;
using PassWinmenu.PasswordManagement;
using PassWinmenuTests.Utilities;
using Xunit;

namespace PassWinmenuTests.Parsing
{
	public class PasswordParserTests
	{
		private const string Category = "Core: Password File Parsing";

		private readonly PasswordFile dummyFile;

		public PasswordParserTests()
		{
			var fileSystem = new MockFileSystem();
			var dirInfo = new MockDirectoryInfo(fileSystem, "\\password-store");
			var fileInfo = new MockFileInfo(fileSystem, "\\password-store\\dummy-password");
			dummyFile = new PasswordFile(fileInfo, dirInfo);
		}

		[Fact, TestCategory(Category)]
		public void Test_EmptyFile()
		{
			var text = "";
			var p = new PasswordFileParser();
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

			var p = new PasswordFileParser();

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

			var p = new PasswordFileParser();

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

			var p = new PasswordFileParser();

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

			var p = new PasswordFileParser();
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

			var p = new PasswordFileParser();
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
