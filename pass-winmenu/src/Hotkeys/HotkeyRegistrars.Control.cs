using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;

using WControl = System.Windows.Controls.Control;

namespace PassWinmenu.Hotkeys
{
    // Implementation for the window control hotkey registrar.
    //
    // See main file:
    //      /src/Hotkeys/HotkeyRegistrars.cs
    public static partial class HotkeyRegistrars
    {
        /// <summary>
        /// A registrar for registering hotkeys for UI controls.
        /// </summary>
        public sealed class Control
            : IHotkeyRegistrar
        {
            /// <summary>
            /// Retrieves a hotkey registrar for a specified UI control,
            /// creating one if one does not already exist.
            /// </summary>
            /// <param name="control">
            /// The UI control for which to create a registrar.
            /// </param>
            /// <returns>
            /// A registrar for the specifed UI control.
            /// </returns>
            public static IHotkeyRegistrar For(WControl control)
            {
                throw new NotImplementedException();
            }


            /*** IHotkeyRegistrar impl ***/
            IDisposable IHotkeyRegistrar.Register(
                ModifierKeys modifierKeys, Key key, bool repeats,
                EventHandler firedHandler
                )
            {
                throw new NotImplementedException();
            }
        }
    }
}
