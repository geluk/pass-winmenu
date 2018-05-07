using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PassWinmenu.Hotkeys
{
    // Implementation for a generic registrar using a key event source.
    //
    // See main file:
    //      /src/Hotkeys/HotkeyRegistrars.cs
    public static partial class HotkeyRegistrars
    {
        /// <summary>
        /// A generic registrar which uses a <see cref="IKeyEventSource"/> as
        /// 
        /// </summary>
        public sealed class KeyEventSource
            : IHotkeyRegistrar
        {
            // Keeps track of previously-created [KeyEventSource]s
            // but using weak references to allow collection if consuming code
            // is no longer using the 
            private static readonly IDictionary<object, WeakReference<KeyEventSource>>
                _registrarCache;


            static KeyEventSource()
            {
                _registrarCache = new Dictionary<object, WeakReference<KeyEventSource>>();
            }


            /// <summary>
            /// Retrieves a <see cref="KeyEventSource"/> for a particular
            /// source of keyboard-related events, creating one if one does not
            /// already exist.
            /// </summary>
            /// <param name="eventSource">
            /// The source of events for which to retrieve a registrar.
            /// </param>
            /// <returns>
            /// A registrar for hotkeys for the specified
            /// <paramref name="eventSource"/>.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="eventSource"/> is null.
            /// </exception>
            public static KeyEventSource Create(IKeyEventSource eventSource)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Retrieves a <see cref="KeyEventSource"/> for a particular
            /// source of keyboard-related events, creating one if one does not
            /// already exist.
            /// </summary>
            /// <typeparam name="TSource">
            /// The type of the key event source.
            /// </typeparam>
            /// <param name="eventSource">
            /// The particular instance of the source for which to create a
            /// registrar.
            /// </param>
            /// <param name="adaptor">
            /// An adaptor which can convert the provided 
            /// <typeparamref name="TSource"/> to a 
            /// <see cref="IKeyEventSource"/>.
            /// </param>
            /// <returns>
            /// A registrar for hotkeys for the specified
            /// <paramref name="eventSource"/>.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="eventSource"/> or <paramref name="adaptor"/>
            /// is null.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// <paramref name="adaptor"/> failed to adapt the provided event
            /// source.
            /// </exception>
            public static KeyEventSource Create<TSource>(
                TSource eventSource, Func<TSource, IKeyEventSource> adaptor
                )
            {
                // Arguments are not null
                if (eventSource == null)
                {
                    throw new ArgumentNullException(nameof(eventSource));
                }

                if (adaptor == null)
                {
                    throw new ArgumentNullException(nameof(adaptor));
                }

                // If a registrar for this event source already exists, return
                // it to the caller.
                if (_registrarCache.TryGetValue(eventSource, out var regRef) &&
                    regRef.TryGetTarget(out var registrar))
                    return registrar;

                // Adaptor does not throw
                IKeyEventSource adaptedSource;
                try
                {
                    adaptedSource = adaptor(eventSource);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"The provided adaptor threw {ex.GetType().Name}.",
                        ex
                        );
                }

                // Adaptor returns non-null
                if (adaptedSource == null)
                {
                    throw new ArgumentException(
                        "The provided adaptor returned a null event source."
                        );
                }

                return KeyEventSource.Create(adaptedSource);
            }



            // Source of key state change events
            private readonly IKeyEventSource _eventSource;
            // The keys currently pressed
            private readonly ISet<Key> _keysDown;
            private readonly ISet<ModifierKeys> _modsDown;


            // Handles [KeyDown] events from the event source
            private void _onKeyDown(object sender, KeyEventArgs eventArgs)
            {
                throw new NotImplementedException();
            }
            // Handles [KeyUp] events from the event source
            private void _onKeyUp(object sender, KeyEventArgs eventArgs)
            {
                throw new NotImplementedException();
            }


            private KeyEventSource(IKeyEventSource eventSource)
            {
                _eventSource = eventSource;
                _eventSource.KeyDown += _onKeyDown;
                _eventSource.KeyUp += _onKeyUp;

                _keysDown = new HashSet<Key>();
                _modsDown = new HashSet<ModifierKeys>();
            }


            /// <summary>
            /// Registers a hotkey with the registrar.
            /// </summary>
            /// <param name="modifierKeys">
            /// The modifiers which are to be pressed with <paramref name="key"/>
            /// in order to trigger the hotkey.
            /// </param>
            /// <param name="key">
            /// The key that is to be pressed with <paramref name="modifierKeys"/>
            /// in order to trigger the hotkey.
            /// </param>
            /// <param name="repeats">
            /// Whether the hotkey is to fire multiple times if held down
            /// continuously. See remarks.
            /// </param>
            /// <param name="firedHandler">
            /// The method to be called when the hotkey fires.
            /// </param>
            /// <returns>
            /// An <see cref="IDisposable"/> which, when disposed, unregisters
            /// the hotkey.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="firedHandler"/> was null.
            /// </exception>
            /// <remarks>
            /// <para>
            /// This registrar supports the registration of multiple hotkeys
            /// with the same key combination but different values for
            /// <paramref name="repeats"/>.
            /// </para>
            /// </remarks>
            public IDisposable Register(
                ModifierKeys modifierKeys, Key key, bool repeats,
                EventHandler firedHandler
                )
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Represents a source of keyboard-related events.
    /// </summary>
    public interface IKeyEventSource
    {
        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        event KeyEventHandler KeyDown;
        /// <summary>
        /// Occurs when a character, space, or backspace key is pressed.
        /// </summary>
        event KeyEventHandler KeyPress;
        /// <summary>
        /// Occurs when a key is released.
        /// </summary>
        event KeyEventHandler KeyUp;
    }
}
