using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PassWinmenu.Utilities
{
	internal class NativeMethods
	{
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		public static Process GetWindowProcess(IntPtr hWnd)
		{
			GetWindowThreadProcessId(hWnd, out uint pid);
			return Process.GetProcessById((int)pid);
		}
	}
}
