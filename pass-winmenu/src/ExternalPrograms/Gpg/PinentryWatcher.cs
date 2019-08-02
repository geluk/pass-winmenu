using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using PassWinmenu.Utilities;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	// As it turns out, the Windows version of pinentry is slightly bugged. If you have a multi-monitor setup,
	// it often fails to bring its window to the foreground, which means you have to click the pinentry
	// window first before you can enter your master password. That's not very nice -- luckily, we can do 
	// something about it. This class will register itself to listen for process creation events,
	// and whenever pinentry is started, it'll look for its window and bump it to the foreground.
	internal class PinentryWatcher
	{
		private readonly TimeSpan maxWaitTime = TimeSpan.FromSeconds(2);
		private readonly TimeSpan waitInterval = TimeSpan.FromMilliseconds(50);

		private static Process GetPinentry() => Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "pinentry-basic" && p.MainWindowTitle == "Pinentry");

		/// <summary>
		/// Waits for a pinentry window to open, and tries to send it to the foreground.
		/// </summary>
		public void BumpPinentryWindow()
		{
			Task.Run(() => BumpPinentryWindowInternal());
		}

		private void BumpPinentryWindowInternal()
		{
			var pinentry = GetPinentry();
			// If the pinentry window has not been created yet, try to wait for it, for up to two seconds.
			for (var i = 0; pinentry == null && i < maxWaitTime.TotalMilliseconds / waitInterval.TotalMilliseconds; i++)
			{
				Thread.Sleep(waitInterval);
				pinentry = GetPinentry();
			}
			// If we still don't have a process, pinentry was most likely not needed, so we don't have to do anything.
			if (pinentry == null)
			{
				return;
			}
			
			Thread.Sleep(100);
			
			// We have the window handle, see if it's the foreground window.
			var foregroundHandle = NativeMethods.GetForegroundWindow();
			
			if (pinentry.MainWindowHandle != foregroundHandle)
			{
				// Looks like pinentry has failed to bring itself to the foreground, let's help it out.
				var windowSet = NativeMethods.SetForegroundWindow(pinentry.MainWindowHandle);
				if (windowSet)
				{
					Log.Send("Sent pinentry window to foreground");
				}
				else
				{
					Log.Send("Failed to send pinentry window to foreground");
				}
			}
			else
			{
				Log.Send($"Pinentry is already the foreground window (pid: {pinentry.Id}, handle: {foregroundHandle})");
			}
		}
	}
}
