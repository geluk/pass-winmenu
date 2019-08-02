using System;
using System.Windows;
using System.Windows.Media;
using PassWinmenu.Utilities;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PassWinmenu.Configuration
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
			var value = parser.Expect<Scalar>();
			return Helpers.BrushFromColourString(value.Value);
		}

		public void WriteYaml(IEmitter emitter, object value, Type type)
		{
			throw new NotImplementedException("Width serialisation is not supported yet.");
		}
	}
}
