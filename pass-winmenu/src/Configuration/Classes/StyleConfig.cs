using System.Windows;
using PassWinmenu.Configuration.Types;

using Brush=System.Windows.Media.Brush;

namespace PassWinmenu.Configuration
{
	internal class StyleConfig
	{
		public LabelStyleConfig Search { get; set; } = new LabelStyleConfig
		{
			TextColour = BrushConverter.BrushFromColourString("#FFDDDDDD"),
			BackgroundColour = BrushConverter.BrushFromColourString("#00FFFFFF"),
			BorderWidth = new Thickness(0),
			BorderColour = BrushConverter.BrushFromColourString("#FF000000"),
			Margin = new Thickness(0)
		};
		public LabelStyleConfig Options { get; set; } = new LabelStyleConfig
		{
			TextColour = BrushConverter.BrushFromColourString("#FFDDDDDD"),
			BackgroundColour = BrushConverter.BrushFromColourString("#00FFFFFF"),
			BorderWidth = new Thickness(0),
			BorderColour = BrushConverter.BrushFromColourString("#FF000000"),
			Margin = new Thickness(0)
		};
		public LabelStyleConfig Selection { get; set; } = new LabelStyleConfig
		{
			TextColour = BrushConverter.BrushFromColourString("#FFFFFFFF"),
			BackgroundColour = BrushConverter.BrushFromColourString("[accent]"),
			BorderWidth = new Thickness(0),
			BorderColour = BrushConverter.BrushFromColourString("#FF000000"),
			Margin = new Thickness(0)
		};
		public TextStyleConfig SearchHint { get; set; } = new TextStyleConfig
		{
			TextColour = BrushConverter.BrushFromColourString("#FF999999"),
		};
		public int ScrollBoundary { get; set; } = 0;
		public string Orientation { get; set; } = "vertical";
		public double FontSize { get; set; } = 14;
		public string FontFamily { get; set; } = "Consolas";
		public Brush BackgroundColour { get; set; } = BrushConverter.BrushFromColourString("#FF202020");
		public Brush BorderColour { get; set; } = BrushConverter.BrushFromColourString("[accent]");
		public Thickness BorderWidth { get; set; } = new Thickness(1);
		public Brush CaretColour { get; set; } = BrushConverter.BrushFromColourString("#FFDDDDDD");
		// These have to be strings, because they need to support percentage values.
		public string OffsetLeft { get; set; } = "40%";
		public string OffsetTop { get; set; } = "40%";
		public string Width { get; set; } = "20%";
		public string Height { get; set; } = "20%";
		public bool ScaleToFit { get; set; } = true;
	}
}
