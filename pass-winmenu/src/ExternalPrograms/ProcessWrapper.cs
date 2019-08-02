using System;
using System.Diagnostics;
using System.IO;

namespace PassWinmenu.ExternalPrograms
{
	internal class ProcessWrapper : IProcess
	{
		private readonly Process process;

		public ProcessWrapper(Process process)
		{
			this.process = process;
		}

		public StreamWriter StandardInput => process.StandardInput;
		public StreamReader StandardOutput => process.StandardOutput;
		public StreamReader StandardError => process.StandardError;
		public int ExitCode => process.ExitCode;
		public string MainModuleName => process.MainModule.FileName;
		public int Id => process.Id;
		public DateTime StartTime => process.StartTime;

		public void Kill() => process.Kill();

		public bool WaitForExit(TimeSpan timeout) => process.WaitForExit((int)timeout.TotalMilliseconds);
	}
}
