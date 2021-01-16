using System;
using System.Windows;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace PassWinmenu.Configuration
{
	internal class WidthConverter : IYamlTypeConverter
	{
		private readonly ArrayNodeDeserializer arrayNodeDeserializer;
		private readonly ScalarNodeDeserializer scalarNodeDeserializer;

		public WidthConverter()
		{
			arrayNodeDeserializer = new ArrayNodeDeserializer();
			scalarNodeDeserializer = new ScalarNodeDeserializer();
		}

		public bool Accepts(Type type)
		{
			return type == typeof(Thickness);
		}

		public object ReadYaml(IParser parser, Type type)
		{
			// A scalar value will be considered to represent a uniform width.
			if (parser.Accept<Scalar>(out _))
			{
				// Only floating-point values are accepted here.
				((INodeDeserializer)scalarNodeDeserializer).Deserialize(parser, typeof(double), null, out var parsedValue);
				return new Thickness((double)parsedValue);
			}

			// If it's not a scalar, it must be parsed as an array.
			if (((INodeDeserializer)arrayNodeDeserializer).Deserialize(parser, typeof(double[]), NestedObjectDeserializer, out var value))
			{
				var asArray = (double[])value;

				switch (asArray.Length)
				{
					case 0:
						return new Thickness(0);
					case 1:
						return new Thickness(asArray[0]);
					case 2:
						return new Thickness(asArray[1], asArray[0], asArray[1], asArray[0]);
					case 4:
						return new Thickness(asArray[3], asArray[0], asArray[1], asArray[2]);
					default:
						throw new ConfigurationParseException($"Invalid width specified. Width should be an sequence of 1, 2 or 4 elements.");
				}
			}
			else
			{
				throw new ConfigurationParseException("Could not parse width.");
			}
		}

		/// <summary>
		/// Converts a scalar to the given type.
		/// </summary>
		private object NestedObjectDeserializer(IParser parser, Type type)
		{
			((INodeDeserializer)scalarNodeDeserializer).Deserialize(parser, type, null, out var parsedValue);
			return parsedValue;
		}

		public void WriteYaml(IEmitter emitter, object value, Type type)
		{
			throw new NotImplementedException("Width serialisation is not supported yet.");
		}
	}
}
