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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PassWinmenu.Configuration;
using PassWinmenu.Utilities;

namespace PassWinmenu.Windows
{
	/// <summary>
	/// Interaction logic for SelectionLabel.xaml
	/// </summary>
	internal partial class SelectionLabel : UserControl
	{
		public string Text
		{
			get => LabelText.Text;
			set => LabelText.Text = value;
		}

		public SelectionLabel(string           content,
		                      LabelStyleConfig labelStyle,
		                      double           fontSize,
		                      FontFamily       fontFamily)
		{
			InitializeComponent();

			LabelBorder.BorderThickness = labelStyle.BorderWidth;
			LabelBorder.BorderBrush = labelStyle.BorderColour;
			Background = labelStyle.BackgroundColour;

			LabelText.Text = content;
			LabelText.FontSize = fontSize;
			LabelText.FontFamily = fontFamily;
			LabelText.Foreground = labelStyle.TextColour;
			Cursor = Cursors.Hand;
		}
	}
}
