using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PassWinmenu.Hotkeys
{
    // Implementation for the Windows hotkey registrar.
    //
    // See main file:
    //      /src/Hotkeys/HotkeyRegistrars.cs
    public static partial class HotkeyRegistrars
    {
        /// <summary>
        /// A registrar for system-wide hotkeys registered through the Windows
        /// API.
        /// </summary>
        private sealed class WindowsHotkeyRegistrar
            : IHotkeyRegistrar, IDisposable
        {
            /// <summary>
            /// Retrieves a <see cref="WindowsHotkeyRegistrar"/> instance,
            /// creating one if one does not already exist.
            /// </summary>
            /// <returns>
            /// A hotkey registrar for the Windows API.
            /// </returns>
            public static WindowsHotkeyRegistrar Retrieve()
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

            /*** IDisposable impl ***/
            void IDisposable.Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
