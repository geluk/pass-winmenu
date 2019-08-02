namespace PassWinmenu.ExternalPrograms.Gpg
{
	public interface IGpgHomedirResolver
	{
		/// <summary>
		/// Returns the path GPG will use as its home directory.
		/// </summary>
		/// <returns></returns>
		string GetHomeDir();

		/// <summary>
		/// Returns the home directory as configured by the user, or null if no home directory has been defined.
		/// </summary>
		/// <returns></returns>
		string GetConfiguredHomeDir();

		/// <summary>
		/// Returns the default home directory used by GPG when no user-defined home directory is available.
		/// </summary>
		/// <returns></returns>
		string GetDefaultHomeDir();
	}
}
