using System.Collections.Generic;
using PassWinmenu.ExternalPrograms.Gpg;

namespace PassWinmenuTests.Utilities
{
	class GpgResultBuilder
	{
		private List<string> stderrMessages = new List<string>();
		private List<StatusMessage> statusMessages = new List<StatusMessage>();
		private string stdout;
		private int exitCode;

		public GpgResultBuilder WithExitCode(int exitCode)
		{
			this.exitCode = exitCode;
			return this;
		}

		public GpgResultBuilder WithStdout(string stdout)
		{
			this.stdout = stdout;
			return this;
		}

		public GpgResultBuilder WithStdErrMessage(string message)
		{
			stderrMessages.Add(message);
			return this;
		}

		public GpgResultBuilder WithStatusMessage(GpgStatusCode code, string message = null)
		{
			statusMessages.Add(new StatusMessage(code, message));
			return this;
		}

		public GpgResult Build()
		{
			return new GpgResult(exitCode, stdout, statusMessages, stderrMessages);
		}
	}
}
