namespace PassWinmenu.PasswordManagement
{
	/// <summary>
	/// Represents a decrypted password file of the 'password store' format,
	/// with a password on the first line, and metadata on the other lines.
	/// </summary>
	internal class ParsedPasswordFile : DecryptedPasswordFile
	{
		public string Password { get; }
		public string Metadata { get; }

		public ParsedPasswordFile(PasswordFile original, string password, string metadata) : base(original)
		{
			Password = password;
			Metadata = metadata;

			if (string.IsNullOrEmpty(metadata))
			{
				// TODO: do we want to include a newline here?
				Content = password;
			}
			else
			{
				Content = $"{Password}\n{Metadata}";
			}
		}
	}
}
