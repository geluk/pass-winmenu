using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassWinmenu.ExternalPrograms
{
	internal class Processes : IProcesses
	{
		public IProcess Start(ProcessStartInfo psi)
		{
			return new ProcessWrapper(Process.Start(psi));
		}

		public IProcess[] GetProcessesByName(string processName)
		{
			return Process.GetProcessesByName(processName).Select(p => (IProcess)new ProcessWrapper(p)).ToArray();
		}

		public IProcess[] GetProcesses()
		{
			return Process.GetProcesses().Select(p => (IProcess)new ProcessWrapper(p)).ToArray();
		}
	}
}
