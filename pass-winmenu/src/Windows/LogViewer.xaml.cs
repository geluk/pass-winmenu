using System;
using System.Windows;

namespace PassWinmenu.Windows
{
	/// <summary>
	/// Interaction logic for LogViewer.xaml
	/// </summary>
	public partial class LogViewer : Window
	{
		public LogViewer(string logText)
		{
			InitializeComponent();
			LogTextBox.Text = logText;
			LogTextBox.SelectionStart = 0;
			LogTextBox.SelectionLength = logText?.Length ?? throw new ArgumentNullException(nameof(logText));
		}
	}
}
