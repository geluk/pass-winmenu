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
        /// its source of input.
        /// </summary>
        public sealed class KeyEventSource
            : IHotkeyRegistrar
        {
            /* Implementation idea:
             * 
             * A trie containing each of the key combinations.
             * 
             * As keys are pressed, each node representing one of the keys
             * pressed is moved into an "active" pool. As the key combination
             * is completed, the nodes in this pool are swapped out for nodes
             * further down the trie branches.
             * 
             * Nodes representing a full combination hold a delegate that is
             * the handler, or some other information for finding the handler.
             * 
             * Nodes store their key value and reference previous and subsequent
             * nodes (like a doubly-linked list) to enable them to be removed
             * from the active pool as required when a key is pressed or released.
             * 
             * This method also enforces the actuation order is the same as is
             * specified in the combination.
             *
             * ---
             * 
             * Idea not viable--only works if keys are pressed in order, and would
             * not handle situation where an intermediate key (e.g. the Shift in a
             * Ctrl-Shift-G) was released before the final key (here, G).
             * 
             * Method also needs to handle ignoring the order of modifiers while
             * still enforcing the order of the final key. Could be achieved in the
             * trie with multiple branches connecting at the final key (i.e. having
             * two separate branches, Ctrl-Shift-G and Shift-Ctrl-G), but throws up
             * the same issues with releasing keys.
             */

            /// <summary>
            /// A node, representing a key, in a trie used to store key
            /// combinations.
            /// </summary>
            private sealed class KeyNode
            {
                /// <summary>
                /// The parent 
                /// </summary>
                public KeyNode Parent { get; }
            }

            // Keeps track of previously-created [KeyEventSource]s but using 
            // weak references to allow garbage collection if consuming code
            // is no longer using the event source.
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
        /// Occurs when a key is released.
        /// </summary>
        event KeyEventHandler KeyUp;
    }
}
