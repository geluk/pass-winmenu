using System;
using System.Windows;
using System.Windows.Media;
using PassWinmenu.Utilities;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

#nullable enable
namespace PassWinmenu.Configuration.Types
{
	internal class BrushConverter : IYamlTypeConverter
	{
		public bool Accepts(Type type)
		{
			return type == typeof(Brush);
		}

		public object ReadYaml(IParser parser, Type type)
		{
			// A brush should be represented as a string value.
			var value = parser.Consume<Scalar>();
			return BrushFromColourString(value.Value);
		}

		public void WriteYaml(IEmitter emitter, object? value, Type type)
		{
			throw new NotImplementedException("Brush serialisation is not supported.");
		}

		/// <summary>
		/// Converts an ARGB hex colour code into a SolidColorBrush object.
		/// </summary>
		/// <param name="colour">A hexadecimal colour code string (such as #AAFF00FF)</param>
		/// <returns>A Brush created from a Colour object created from the colour code.</returns>
		public static Brush BrushFromColourString(string colour)
		{
			if (colour == "[accent]")
			{
				return SystemParameters.WindowGlassBrush;
			}
			return new SolidColorBrush(Helpers.ColourFromString(colour));
		}

	}
}
