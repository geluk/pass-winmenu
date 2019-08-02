using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Input;

namespace PassWinmenu.Utilities.ExtensionMethods
{
	using KeyEventArgs = System.Windows.Input.KeyEventArgs;

	public static class KeyEventArgsExtensions
	{
		private static readonly MethodInfo _setRepeatInfo;

		static KeyEventArgsExtensions()
		{
			_setRepeatInfo = typeof(KeyEventArgs).GetMethod(
				"SetRepeat", BindingFlags.NonPublic | BindingFlags.Instance
				);
		}

		/// <summary>
		/// Sets the value of the <see cref="KeyEventArgs.IsRepeat"/> property.
		/// </summary>
		/// <param name="keyEventArgs">
		/// The instance the value of the property of which to set.
		/// </param>
		/// <param name="value">
		/// The value <see cref="KeyEventArgs.IsRepeat"/> is to be set to.
		/// </param>
		/// <exception cref="ArgumentNullException"></exception>
		internal static KeyEventArgs SetRepeat(this KeyEventArgs keyEventArgs, bool value)
		{
			if (keyEventArgs == null)
				throw new ArgumentNullException(nameof(keyEventArgs));

			_setRepeatInfo.Invoke(keyEventArgs, new object[] { value });

			return keyEventArgs;
		}

		/// <summary>
		/// Converts a <see cref="KeyEventArgs"/> to a <see cref="ModifierKeys"/> if
		/// <see cref="KeyEventArgs.Key"/> is a modifier key.
		/// </summary>
		/// <param name="modifier">
		/// If <see cref="KeyEventArgs.Key"/> is a modifier key, that modifier key.
		/// Otherwise, the value is undefined.
		/// </param>
		/// <returns>
		/// True if <see cref="KeyEventArgs.Key"/> is a modifier key, false if
		/// otherwise.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="keyEventArgs"/> is null.
		/// </exception>
		public static bool AsModifierKey(this KeyEventArgs keyEventArgs, out ModifierKeys modifier)
		{
			if (keyEventArgs == null)
				throw new ArgumentNullException(nameof(keyEventArgs));

			switch (keyEventArgs.Key)
			{
				case Key.LeftAlt:
				case Key.RightAlt:
					modifier = ModifierKeys.Alt;
					break;

				case Key.LeftCtrl:
				case Key.RightCtrl:
					modifier = ModifierKeys.Control;
					break;

				case Key.LeftShift:
				case Key.RightShift:
					modifier = ModifierKeys.Shift;
					break;

				// I don't think these are sent to application windows, but
				// we might as well include them for completeness.
				case Key.LWin:
				case Key.RWin:
					modifier = ModifierKeys.Windows;
					break;

				default:
					modifier = default(ModifierKeys);
					break;
			}

			return modifier != default(ModifierKeys);
		}
	}
}
