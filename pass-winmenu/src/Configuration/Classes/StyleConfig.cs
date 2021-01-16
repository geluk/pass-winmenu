using System.Windows;
using System.Windows.Media;
using PassWinmenu.Utilities;

namespace PassWinmenu.Configuration
{
	internal class StyleConfig
	{
		public LabelStyleConfig Search { get; set; } = new LabelStyleConfig
		{
			TextColour = Helpers.BrushFromColourString("#FFDDDDDD"),
			BackgroundColour = Helpers.BrushFromColourString("#00FFFFFF"),
			BorderWidth = new Thickness(0),
			BorderColour = Helpers.BrushFromColourString("#FF000000"),
			Margin = new Thickness(0)
		};
		public LabelStyleConfig Options { get; set; } = new LabelStyleConfig
		{
			TextColour = Helpers.BrushFromColourString("#FFDDDDDD"),
			BackgroundColour = Helpers.BrushFromColourString("#00FFFFFF"),
			BorderWidth = new Thickness(0),
			BorderColour = Helpers.BrushFromColourString("#FF000000"),
			Margin = new Thickness(0)
		};
		public LabelStyleConfig Selection { get; set; } = new LabelStyleConfig
		{
			TextColour = Helpers.BrushFromColourString("#FFFFFFFF"),
			BackgroundColour = Helpers.BrushFromColourString("[accent]"),
			BorderWidth = new Thickness(0),
			BorderColour = Helpers.BrushFromColourString("#FF000000"),
			Margin = new Thickness(0)
		};
		public int ScrollBoundary { get; set; } = 0;
		public string Orientation { get; set; } = "vertical";
		public double FontSize { get; set; } = 14;
		public string FontFamily { get; set; } = "Consolas";
		public Brush BackgroundColour { get; set; } = Helpers.BrushFromColourString("#FF202020");
		public Brush BorderColour { get; set; } = Helpers.BrushFromColourString("[accent]");
		public Thickness BorderWidth { get; set; } = new Thickness(1);
		public Brush CaretColour { get; set; } = Helpers.BrushFromColourString("#FFDDDDDD");
		// These have to be strings, because they need to support percentage values.
		public string OffsetLeft { get; set; } = "40%";
		public string OffsetTop { get; set; } = "40%";
		public string Width { get; set; } = "20%";
		public string Height { get; set; } = "20%";
		public bool ScaleToFit { get; set; } = true;
	}
}
