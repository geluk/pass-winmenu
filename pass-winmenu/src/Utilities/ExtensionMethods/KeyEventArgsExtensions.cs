using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

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
        internal static void SetRepeat(this KeyEventArgs keyEventArgs, bool value)
        {
            if (keyEventArgs == null)
                throw new ArgumentNullException(nameof(keyEventArgs));

            _setRepeatInfo.Invoke(keyEventArgs, new object[] { value });
        }
    }
}
