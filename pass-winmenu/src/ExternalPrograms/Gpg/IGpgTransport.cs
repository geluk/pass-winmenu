namespace PassWinmenu.ExternalPrograms.Gpg
{
	internal interface IGpgTransport
	{
		GpgResult CallGpg(string arguments, string input = null);
	}
}