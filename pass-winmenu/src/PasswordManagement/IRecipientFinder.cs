namespace PassWinmenu.PasswordManagement
{
	internal interface IRecipientFinder
	{
		string[] FindRecipients(PasswordFile file);
	}
}
