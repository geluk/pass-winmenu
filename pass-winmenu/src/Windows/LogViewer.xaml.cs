using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PassWinmenu.src.Windows
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
			LogTextBox.SelectionLength = logText.Length;
		}
	}
}
