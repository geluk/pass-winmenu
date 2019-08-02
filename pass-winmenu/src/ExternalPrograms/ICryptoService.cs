namespace PassWinmenu.ExternalPrograms
{
	internal interface ICryptoService
	{
		string Decrypt(string file);
		void Encrypt(string data, string outputFile, params string[] recipients);
		string GetVersion();
	}
}
