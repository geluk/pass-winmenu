using System.IO.Abstractions;

namespace PassWinmenu.PasswordManagement
{
	/// <summary>
	/// Represents a decrypted password file of an unknown format.
	/// </summary>
	internal class DecryptedPasswordFile : PasswordFile
	{
		// Allow derived types to set the content.
		public string Content { get; protected set; }

		public DecryptedPasswordFile(PasswordFile original, string content) : base(original)
		{
			Content = content;
		}

		// Only derived types may call this constructor, since they may want to determine the content themselves.
		protected DecryptedPasswordFile(PasswordFile original) : base(original)
		{
		}
	}
}
