using System.IO.Abstractions;
using PassWinmenu.ExternalPrograms;

namespace PassWinmenuTests.Utilities
{
	class FakeCryptoService : ICryptoService
	{
		private readonly IFileSystem fileSystem;

		public FakeCryptoService(IFileSystem fileSystem)
		{
			this.fileSystem = fileSystem;
		}

		public string Decrypt(string file)
		{
			return fileSystem.File.ReadAllText(file);
		}

		public void Encrypt(string data, string outputFile, params string[] recipients)
		{
			fileSystem.File.WriteAllText(outputFile, data);
		}

		public string GetVersion()
		{
			return "FakeCryptoService";
		}
	}
}
