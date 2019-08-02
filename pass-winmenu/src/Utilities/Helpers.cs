using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace PassWinmenu.Utilities
{
	internal static class Helpers
	{
		/// <summary>
		/// Normalises a directory, replacing all AltDirectorySeparatorChars with DirectorySeparatorChars
		/// and stripping any trailing directory separators.
		/// </summary>
		/// <param name="directory">The directory to be normalised.</param>
		/// <returns>The normalised directory.</returns>
		internal static string NormaliseDirectory(string directory)
		{
			var normalised = directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			var stripped = normalised.TrimEnd(Path.DirectorySeparatorChar);
			return stripped;
		}

		/// <summary>
		/// Retrieves an <see cref="Exception"/> representing the last Win32
		/// error.
		/// </summary>
		internal static Exception LastWin32Exception()
		{
			return Marshal.GetExceptionForHR(
				Marshal.GetHRForLastWin32Error()
				);
		}

		/// <summary>
		/// Converts an ARGB hex colour code into a Color object.
		/// </summary>
		/// <param name="str">A hexadecimal colour code string (such as #AAFF00FF)</param>
		/// <returns>A colour object created from the colour code.</returns>
		internal static Color ColourFromString(string str)
		{
			return (Color)ColorConverter.ConvertFromString(str);
		}

		/// <summary>
		/// Converts an ARGB hex colour code into a SolidColorBrush object.
		/// </summary>
		/// <param name="colour">A hexadecimal colour code string (such as #AAFF00FF)</param>
		/// <returns>A Brush created from a Colour object created from the colour code.</returns>
		internal static Brush BrushFromColourString(string colour)
		{
			if (colour == "[accent]")
			{
				return SystemParameters.WindowGlassBrush;
			}
			return new SolidColorBrush(ColourFromString(colour));
		}

		/// <summary>
		/// Ensures that the thread calling this method is a UI thread.
		/// </summary>
		internal static void AssertOnUiThread()
		{
			var currentThread = Thread.CurrentThread;

			if (currentThread.IsBackground)
			{
				throw new NotOnUiThreadException("Current thread is a background thread.");
			}
			if (currentThread.GetApartmentState() != ApartmentState.STA)
			{
				throw new NotOnUiThreadException("Current thread is not a single-threaded apartment thread.");
			}
			if (currentThread != Application.Current.Dispatcher.Thread)
			{
				throw new NotOnUiThreadException("Current thread is not the current application's dispatcher thread.");
			}
		}

		[Serializable]
		class NotOnUiThreadException : Exception
		{
			public NotOnUiThreadException(string message) : base(message)
			{
			}

			public NotOnUiThreadException(string message, Exception innerException) : base(message, innerException)
			{
			}
		}
	}
}
