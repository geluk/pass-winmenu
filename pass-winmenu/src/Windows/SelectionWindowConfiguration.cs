using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PassWinmenu.Configuration;
using PassWinmenu.Utilities.ExtensionMethods;
using Screen=System.Windows.Forms.Screen;
using Cursor=System.Windows.Forms.Cursor;

namespace PassWinmenu.Windows
{
	internal class SelectionWindowConfiguration
	{
		public Point Position { get; set; }
		public Point Dimensions { get; set; }
		public Orientation Orientation { get; set; }

		/// <summary>
		/// Builds a SelectionWindowConfiguration object according to the config settings.
		/// </summary>
		/// <returns></returns>
		public static SelectionWindowConfiguration ParseMainWindowConfiguration(Config config)
		{
			var activeScreen = Screen.AllScreens.First(screen => screen.Bounds.Contains(Cursor.Position));
			var selectedScreen = config.Interface.FollowCursor ? activeScreen : Screen.PrimaryScreen;

			double left, top, width, height;
			try
			{
				// The menu position may either be specified in pixels or percentage values.
				// ParseSize takes care of parsing both into a double (representing pixel values).
				left = selectedScreen.ParseSize(config.Interface.Style.OffsetLeft, Direction.Horizontal);
				top = selectedScreen.ParseSize(config.Interface.Style.OffsetTop, Direction.Vertical);
			}
			catch (Exception e) when (e is ArgumentNullException || e is FormatException || e is OverflowException)
			{
				throw new ConfigurationParseException($"Unable to parse the menu position from the config file (reason: {e.Message})", e);
			}
			try
			{
				width = selectedScreen.ParseSize(config.Interface.Style.Width, Direction.Horizontal);
				height = selectedScreen.ParseSize(config.Interface.Style.Height, Direction.Vertical);
			}
			catch (Exception e) when (e is ArgumentNullException || e is FormatException || e is OverflowException)
			{
				throw new ConfigurationParseException($"Unable to parse the menu dimensions from the config file (reason: {e.Message})", e);
			}

			Orientation orientation;

			if (!Enum.TryParse(config.Interface.Style.Orientation, true, out orientation))
			{
				throw new ConfigurationParseException("Unable to parse the menu orientation from the config file.");
			}

			return new SelectionWindowConfiguration
			{
				Dimensions = new Point(width, height),
				Position = new Point(left + selectedScreen.Bounds.Left, top + selectedScreen.Bounds.Top),
				Orientation = orientation
			};
		}
	}
}
