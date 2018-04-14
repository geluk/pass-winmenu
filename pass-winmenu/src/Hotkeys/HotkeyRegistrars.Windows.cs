using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace PassWinmenu.Hotkeys
{
    // Main implementation for the Windows hotkey registrar.
    //
    // See main file:
    //      /src/Hotkeys/HotkeyRegistrars.cs
    //
    // See ancillary file:
    //      /src/Hotkeys/HotkeyRegistrars.Windows.MessageWindow.cs
    public static partial class HotkeyRegistrars
    {
        /// <summary>
        /// A registrar for system-wide hotkeys registered through the Windows
        /// API.
        /// </summary>
        private sealed partial class WindowsHotkeyRegistrar
            : IHotkeyRegistrar, IDisposable
        {
            /// <summary>
            /// Registers a system-wide hotkey.
            /// </summary>
            /// <param name="hWnd">
            /// The handle to the window that is to receive notification of
            /// the hotkey being triggered.
            /// </param>
            /// <param name="id">
            /// A handle-unique identifier for the hotkey.
            /// </param>
            /// <param name="fsModifiers">
            /// The modifier keys to be pressed with the hotkey, and other
            /// behavioural flags.
            /// </param>
            /// <param name="vk">
            /// The virtual-key code of the hotkey to be pressed with the
            /// modifier keys.
            /// </param>
            /// <returns>
            /// True if the hotkey was registered, false if otherwise.
            /// </returns>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool RegisterHotKey(
                IntPtr  hWnd,
                int     id,
                uint    fsModifiers,
                uint    vk
                );

            /// <summary>
            /// Unregisters a hotkey registered through the
            /// <see cref="RegisterHotKey(IntPtr, int, uint, uint)"/> function.
            /// </summary>
            /// <param name="hWnd">
            /// The handle to the window that receives notifications of the
            /// triggering of the hotkey to unregister.
            /// </param>
            /// <param name="id">
            /// The handle-unique identifier of the hotkey to unregister.
            /// </param>
            /// <returns>
            /// True if the hotkey was unregistered, false if otherwise.
            /// </returns>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnregisterHotKey(IntPtr hWnd, int id);



            private static WindowsHotkeyRegistrar _singleton = null;

            /// <summary>
            /// Retrieves a <see cref="WindowsHotkeyRegistrar"/> instance,
            /// creating one if one does not already exist.
            /// </summary>
            /// <returns>
            /// A hotkey registrar for the Windows API.
            /// </returns>
            public static WindowsHotkeyRegistrar Retrieve()
            {
                return _singleton ?? (_singleton = new WindowsHotkeyRegistrar());
            }



            // The window that will receive hotkey notifications for us.
            private readonly MessageWindow _msgWindow;

            private WindowsHotkeyRegistrar()
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
