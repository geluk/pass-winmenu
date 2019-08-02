using System;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	/// <summary>
	/// Represents an error type that is recognised by <see cref="GPG"/>.
	/// The exception message contains useful information that can be displayed directly to the user.
	/// </summary>
	[Serializable]
	internal class GpgError : GpgException
	{
		public GpgError(string message) : base(message) { }
	}
}
