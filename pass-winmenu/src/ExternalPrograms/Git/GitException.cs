using System;

namespace PassWinmenu.ExternalPrograms
{
	public class GitException : Exception
	{
		public GitException()
		{
		}

		public string GitError { get; }

		public GitException(string message) : base(message)
		{
		}

		public GitException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public GitException(string message, string gitError) : base(message)
		{
			GitError = gitError;
		}

	}
}
