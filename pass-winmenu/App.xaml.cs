using System;
using System.Windows;
using PassWinmenu.Windows;

namespace PassWinmenu
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public sealed partial class App : Application, IDisposable
	{
		private static MainWindow mainWindow;

		private void App_Startup(object sender, StartupEventArgs e)
		{
			mainWindow = new MainWindow();
			mainWindow.Start();
		}

		public void Dispose()
		{
			DisposeApplication();
		}

		private static void DisposeApplication()
		{
			mainWindow?.Dispose();
		}

		public static new void Exit()
		{
			mainWindow.Close();
			DisposeApplication();
		}
	}
}
