using System.Collections.Generic;

namespace PassWinmenu.PasswordManagement
{
	internal interface IPasswordManager
	{
		IEnumerable<PasswordFile> GetPasswordFiles(string pattern);

		KeyedPasswordFile DecryptPassword(PasswordFile file, bool passwordOnFirstLine);

		PasswordFile EncryptPassword(DecryptedPasswordFile file);
		
		PasswordFile AddPassword(string path, string password, string metadata);
	}
}
