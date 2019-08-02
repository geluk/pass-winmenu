using System;
using System.Windows;
using System.Windows.Input;

namespace PassWinmenu.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public sealed partial class MainWindow : Window, IDisposable
	{
		private readonly Program program;

		public MainWindow()
		{
			InitializeComponent();
			program = new Program();
		}

		public void Dispose()
		{
			program?.Dispose();
		}
	}
}
