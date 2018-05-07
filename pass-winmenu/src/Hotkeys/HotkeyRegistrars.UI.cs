using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace PassWinmenu.Hotkeys
{
    // Implementation for the UI element hotkey registrar.
    //
    // See main file:
    //      /src/Hotkeys/HotkeyRegistrars.cs
    public static partial class HotkeyRegistrars
    {
        /// <summary>
        /// A registrar for registering hotkeys for UI elements.
        /// </summary>
        public sealed class UI
            : IHotkeyRegistrar
        {
            /// <summary>
            /// Retrieves a hotkey registrar for a particular UI element,
            /// creating one if one does not already exist.
            /// </summary>
            /// <typeparam name="T">
            /// The type of <see cref="UIElement"/> for which to create a
            /// registrar.
            /// </typeparam>
            /// <param name="element">
            /// The particular <see cref="UIElement"/> for which to create
            /// a registrar.
            /// </param>
            /// <returns>
            /// A registrar for the specified UI element.
            /// </returns>
            public static IHotkeyRegistrar For<T>(T element)
                where T : UIElement
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
