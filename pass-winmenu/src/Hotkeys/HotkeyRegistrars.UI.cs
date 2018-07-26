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
        {
            // Simple utility class for adapting a [UIElement] into the
            // [IKeyEventSource] required by the generic registrar.
            private sealed class Adaptor<T> : IKeyEventSource
                where T : UIElement
            {
                public static Adaptor<T> Create<T>(T element)
                    where T : UIElement
                {
                    return new Adaptor<T>(element);
                }


                private readonly T _element;

                private Adaptor(T element) => _element = element;

                event KeyEventHandler IKeyEventSource.KeyDown
                {
                    add    => _element.KeyDown += value;
                    remove => _element.KeyDown -= value;
                }

                event KeyEventHandler IKeyEventSource.KeyUp
                {
                    add    => _element.KeyUp += value;
                    remove => _element.KeyUp -= value;
                }
            }

            /// <summary>
            /// Retrieves a hotkey registrar for a particular UI element,
            /// creating one if one does not already exist.
            /// </summary>
            /// <typeparam name="TElem">
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
            public static IHotkeyRegistrar For<TElem>(TElem element)
                where TElem : UIElement
            {
                return KeyEventSource.Create(element, Adaptor<TElem>.Create);
            }
        }
    }
}
