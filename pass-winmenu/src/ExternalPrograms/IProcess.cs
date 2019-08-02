using System;
using System.Diagnostics;
using System.IO;

namespace PassWinmenu.ExternalPrograms
{
	public interface IProcess
	{
		int Id { get; }
		string MainModuleName { get; }
		DateTime StartTime { get; }
		StreamWriter StandardInput { get; }
		StreamReader StandardOutput { get; }
		StreamReader StandardError { get; }
		int ExitCode { get; }

		void Kill();
		bool WaitForExit(TimeSpan timeout);
	}
}
