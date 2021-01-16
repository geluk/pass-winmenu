using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace PassWinmenu
{
	public static class EmbeddedResources
	{
		public static Icon Icon => new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.pass-winmenu-plain.ico"));
		public static Icon IconAhead => new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.pass-winmenu-ahead.ico"));
		public static Icon IconBehind => new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.pass-winmenu-behind.ico"));
		public static Icon IconDiverged => new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.pass-winmenu-diverged.ico"));
		public static Stream DefaultConfig => Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.default-config.yaml");
		public static string Version { get; private set; }

		public static void Load()
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.version.txt");
			if (stream == null)
			{
				throw new InvalidOperationException("Version number could not be read from the assembly.");
			}
			using (var reader = new StreamReader(stream))
			{
				Version = reader.ReadLine();
			}
		}
	}
}
