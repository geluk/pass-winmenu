using System.Diagnostics;
using System.Linq;

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
