namespace PassWinmenu.ExternalPrograms.Gpg
{
	public interface ISignService
	{
		string Sign(string message, string keyId);
	}
}
