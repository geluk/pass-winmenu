using System;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	/// <summary>
	/// Represents a generic GPG error.
	/// The exception message does not necessarily contain information useful to the user,
	/// and may contain cryptic error messages directly passed on from GPG.
	/// </summary>
	[Serializable]
	internal class GpgException : Exception
	{
		public GpgException(string message) : base(message) { }
	}
}
