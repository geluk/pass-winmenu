using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace PassWinmenuTests.Utilities
{
	public class MockFileSystemBuilder
	{
		private readonly Dictionary<string, MockFileData> files;

		public MockFileSystemBuilder()
		{
			files = new Dictionary<string, MockFileData>
			{
				{@"C:\gpg\bin\gpg.exe", MockFileData.NullObject },
				{@"C:\gpg\bin\gpg-agent.exe", MockFileData.NullObject },
				{@"C:\gpg\bin\gpg-connect-agent.exe", MockFileData.NullObject },
			};
		}

		public MockFileSystemBuilder WithEmptyFile(string path)
		{
			files[path] = MockFileData.NullObject;
			return this;
		}

		public MockFileSystemBuilder WithFile(string path, string content)
		{
			files[path] = new MockFileData(content);
			return this;
		}

		public MockFileSystem Build()
		{
			return new MockFileSystem(files);
		}
	}
}
