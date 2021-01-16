using System;
using System.Windows;

namespace PassWinmenu.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public sealed partial class MainWindow : Window, IDisposable
	{
		private Program program = new Program();

		public MainWindow()
		{
			InitializeComponent();
		}

		public void Start()
		{
			program.Start();
		}

		public void Dispose()
		{
			program?.Dispose();
		}
	}
}
