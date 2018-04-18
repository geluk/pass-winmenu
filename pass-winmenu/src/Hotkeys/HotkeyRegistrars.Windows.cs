using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Disposable = PassWinmenu.Utilities.Disposable;
using Helpers    = PassWinmenu.Utilities.Helpers;

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



            /// <summary>
            /// The flag passed to <see cref="RegisterHotKey(IntPtr, int, uint, uint)"/>
            /// to indicate that continuously holding the combination down should
            /// trigger the hotkey multiple times.
            /// </summary>
            private const ushort MOD_NOREPEAT = 0x4000;


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



            // The window procedure for handling hotkey messages.
            private IntPtr? _windowProcedure(
                IntPtr hWnd, WindowMessage msg, UIntPtr wParam, IntPtr lParam
                )
            {
                // We only care about hotkey messages
                if (msg == WindowMessage.Hotkey)
                {
                    // If we don't recognise the hotkey ID, ignore it.
                    if (!_hotkeys.TryGetValue((int)wParam, out var handler))
                    {
                        // TODO: Trace here?
                        return null;
                    }

                    // The logic in the rest of the class should prevent this
                    // from being null. If it doesn't, we want the error, as
                    // it means we aren't doing something properly.
                    handler.Invoke(this, null);

                    // Indicate success
                    return IntPtr.Zero;
                }

                // We didn't handle it; defer.
                return null;
            }

            // The window that will receive hotkey notifications for us.
            private readonly MessageWindow _msgWindow;
            // Event handlers for the hotkey being triggered, keyed by the ID
            // provided when registering the hotkey.
            private readonly IDictionary<int, EventHandler> _hotkeys;
            // Whether we're disposed
            private bool _disposed = false;

            private WindowsHotkeyRegistrar()
            {
                _msgWindow = new MessageWindow(_windowProcedure);

                _hotkeys = new Dictionary<int, EventHandler>();
            }


            /*** IHotkeyRegistrar impl ***/
            IDisposable IHotkeyRegistrar.Register(
                ModifierKeys modifierKeys, Key key, bool repeats,
                EventHandler firedHandler
                )
            {
                if (firedHandler == null)
                {
                    throw new ArgumentNullException(nameof(firedHandler));
                }

                // If a hotkey for this combination is already registered, then
                // we can use a multicast delegate instead of re-registering.
                //
                // ID mirrors the [lParam] for the [WM_HOTKEY] message, but with
                // the [MOD_NOREPEAT] flag bit included where appropriate.
                var virtualKey = KeyInterop.VirtualKeyFromKey(key);
                var hotkeyId = ((int)modifierKeys) << 16             |
                               (!repeats ? (MOD_NOREPEAT << 16) : 0) |
                               virtualKey                            ;

                if (_hotkeys.ContainsKey(hotkeyId))
                {
                    _hotkeys[hotkeyId] += firedHandler;
                }
                // Otherwise, the hotkey is not yet registered.
                else
                {
                    var success = RegisterHotKey(
                        hWnd:           _msgWindow.Handle,
                        id:             hotkeyId,
                        fsModifiers:    (uint)modifierKeys |
                                        (!repeats ? MOD_NOREPEAT : 0U),
                        vk:             (uint)virtualKey
                        );

                    if (success)
                    {
                        // Will fail if the ID is already in the collection
                        _hotkeys.Add(hotkeyId, firedHandler);
                    }
                    else
                    {
                        throw new HotkeyException(
                            message:        "An error occured in registering the hotkey.",
                            innerException: Helpers.LastWin32Exception()
                            );
                    }
                }

                return new Disposable(() =>
                {
                    var handler = (_hotkeys[hotkeyId] -= firedHandler);

                    // A multicast delegate becomes null when all of its member
                    // delegates are removed. If there are no handlers, we want
                    // to unregister the hotkey.
                    if (handler == null)
                    {
                        var unreg = UnregisterHotKey(
                            hWnd: _msgWindow.Handle,
                            id:   hotkeyId
                            );

                        if (!unreg)
                        {
                            throw Helpers.LastWin32Exception();
                        }

                        _hotkeys.Remove(hotkeyId);
                    }
                });
            }

            /*** IDisposable impl ***/
            void IDisposable.Dispose()
            {
                if (_disposed)
                    return;

                // Attempt to unregister all of our hotkeys
                foreach (var hk in _hotkeys)
                {
                    if (!UnregisterHotKey(_msgWindow.Handle, hk.Key))
                    {
                        throw Helpers.LastWin32Exception();
                    }
                }

                _hotkeys.Clear();

                _msgWindow.Dispose();

                // Next call to [Retrieve] will create a new registrar.
                _singleton = null;

                _disposed = true;
            }
        }
    }
}
