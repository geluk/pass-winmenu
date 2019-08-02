namespace PassWinmenu.ExternalPrograms.Gpg
{
	internal interface IGpgResultVerifier
	{
		void VerifyDecryption(GpgResult result);
		void VerifyEncryption(GpgResult result);
	}
}
