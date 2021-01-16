using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PassWinmenu.Configuration;

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
