using System.IO;
using PassWinmenu.Configuration.Types;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PassWinmenu.Configuration
{
	public static class ConfigurationDeserialiser
	{
		private static readonly IDeserializer Deserialiser = new DeserializerBuilder()
			.WithNamingConvention(HyphenatedNamingConvention.Instance)
			.WithTypeConverter(new WidthConverter())
			.WithTypeConverter(new BrushConverter())
			.Build();

		public static T Deserialise<T>(StreamReader reader)
		{
			return Deserialiser.Deserialize<T>(reader);
		}

	}
}
