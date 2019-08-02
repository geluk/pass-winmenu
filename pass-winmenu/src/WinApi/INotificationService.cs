using System;
using PassWinmenu.UpdateChecking;

namespace PassWinmenu.WinApi
{
	internal interface INotificationService : IDisposable
	{
		void Raise(string message, Severity level);
		void ShowErrorWindow(string message, string title = "An error occurred.");
		void HandleUpdateAvailable(UpdateAvailableEventArgs args);
	}
}
