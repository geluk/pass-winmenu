using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using PassWinmenu.Utilities.ExtensionMethods;

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
			private enum Direction { Up, Down };

			/// <summary>
			/// Consumes key events to determine whether the handler for a key
			/// combination should be fired.
			/// </summary>
			private sealed class ComboMachine
			{
				// Currently-actuated modifier keys, reset on trigger
				private ModifierKeys _modsState;
				// Whether [Key] is currently actuated, reset on trigger
				private bool _keyState;


				/// <summary>
				/// Creates a new <see cref="ComboMachine"/> for the specified
				/// modifiers and key.
				/// </summary>
				public ComboMachine(
					ModifierKeys modifiers, Key key, bool isRepeat,
					EventHandler handler
					)
				{
					this.Modifiers = modifiers;
					this.Key = key;
					this.Repeats = isRepeat;
					this.Triggered = handler;

					_modsState = ModifierKeys.None;
					_keyState = false;
				}


				/// <summary>
				/// The modifier keys which will trigger the firing of the handler
				/// when pressed with <see cref="Key"/>.
				/// </summary>
				public ModifierKeys Modifiers { get; }
				/// <summary>
				/// The key which will trigger the firing of the handler when pressed
				/// with <see cref="Modifiers"/>.
				/// </summary>
				public Key Key { get; }
				/// <summary>
				/// Whether the firing of the handler will be triggered repeatedly
				/// when the key combination is continuously held down.
				/// </summary>
				public bool Repeats { get; }


				/// <summary>
				/// The event fired when the key combination is pressed.
				/// </summary>
				public event EventHandler Triggered;
				/// <summary>
				/// The handlers currently registered for <see cref="Triggered"/>.
				/// </summary>
				public EventHandler Handlers => Triggered;


				/// <summary>
				/// Updates the state of the machine and reports whether the
				/// handler for the combination should be fired.
				/// </summary>
				/// <param name="direction">
				/// The direction of the key event.
				/// </param>
				/// <param name="key">
				/// The event arguments for the actuation of the key.
				/// </param>
				/// <returns></returns>
				public void Update(Direction direction, KeyEventArgs eventArgs)
				{
					switch (direction)
					{
						case Direction.Down:
						{
							// If we don't fire multiple times when a key is
							// held down, ignore repeated keys
							if (!this.Repeats && eventArgs.IsRepeat)
								return;

							// If the key pressed is the same as [Key], we want to
							// indicate that it is pressed, but only if the modifier
							// keys have already been pressed.
							if (eventArgs.Key == this.Key &&
								(_modsState & this.Modifiers) == this.Modifiers)
								_keyState = true;
							// Add the modifier key if it is actuated
							else if (eventArgs.AsModifierKey(out var mods))
								_modsState |= mods;

							// If we have a match, trigger and reset
							if (_keyState && (_modsState & this.Modifiers) == this.Modifiers)
							{
								// We only reset the final key (and not the modifier keys)
								// state as this allows keypresses of [Ctrl A A A] for a
								// combination of [Ctrl+A] to trigger three times, in line
								// with the Windows API behaviour.
								//
								// Erroneous firing is mitigated by the modifier key state
								// being cleared anyway on key-up, and the enforcement of
								// actuation order.
								_keyState = false;

								this.Triggered?.Invoke(this, eventArgs);
							}
						} break;

						case Direction.Up:
						{
							// We don't test for repeats because it logically
							// doesn't make sense that a key-release event could
							// be repeated without a corresponding key down event,
							// and in any case it shouldn't make a difference if
							// they were.

							if (eventArgs.Key == this.Key)
								_keyState = false;
							else if (eventArgs.AsModifierKey(out var mods))
								_modsState &= ~mods;
						} break;
					}
				}
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
			/// Creates a <see cref="KeyEventSource"/> for a particular source of 
			/// keyboard-related events.
			/// </summary>
			/// <param name="eventSource">
			/// The source of events for which to create a registrar.
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
				if (eventSource == null)
					throw new ArgumentNullException(nameof(eventSource));

				return new KeyEventSource(eventSource);
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
			// Machines for all current key combinations
			private readonly IList<ComboMachine> _combos;


			// Generic handler for events from the event source.
			private void _onKey(Direction dir, object sender, KeyEventArgs eventArgs)
			{
				foreach (var cm in _combos)
				{
					cm.Update(dir, eventArgs);
				}
			}
			// Relays [KeyDown] events from the event source
			private void _onKeyDown(object sender, KeyEventArgs eventArgs)
			{
				_onKey(Direction.Down, sender, eventArgs);
			}
			// Relays [KeyUp] events from the event source
			private void _onKeyUp(object sender, KeyEventArgs eventArgs)
			{
				_onKey(Direction.Up, sender, eventArgs);
			}


			private KeyEventSource(IKeyEventSource eventSource)
			{
				_eventSource = eventSource;
				_eventSource.KeyDown += _onKeyDown;
				_eventSource.KeyUp += _onKeyUp;

				_combos = new List<ComboMachine>();
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
			/// <exception cref="HotkeyException">
			/// A hotkey with the same <paramref name="modifierKeys"/> and
			/// <paramref name="key"/> values but a different <paramref name="repeats"/>
			/// value has already been registered.
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
				if (firedHandler == null)
					throw new ArgumentNullException(nameof(firedHandler));

				ComboMachine combo;

				// Do we already have a hotkey with this combination registered?
				if (default(ComboMachine) != (combo = _combos.SingleOrDefault(
					                            cm => cm.Modifiers == modifierKeys &&
					                                  cm.Key       == key)))
				{
					// If we do but its "repeat" configuration is not the same,
					// then we must throw.
					if (repeats != combo.Repeats)
					{
						throw new HotkeyException(
							"A hotkey with this combination but a different keyboard " +
							"auto-repeat configuration is already registered."
							);
					}

					// If we do but its "repeat" configuration is the same, then
					// we add its handler and continue
					combo.Triggered += firedHandler;
				}
				else
				{
					// If we don't, then we create one.
					combo = new ComboMachine(modifierKeys, key, repeats, firedHandler);
					_combos.Add(combo);
				}

				// Return a disposable that will remove our handler and deregister
				// the hotkey (if appropriate).
				return new Utilities.Disposable(() =>
				{
					// Do nothing if the hotkey has been deregistered.
					if (combo == null)
						return;

					// Otherwise, deregister our handler
					combo.Triggered -= firedHandler;

					// If no handlers remain, then we want to remove the instance
					// from our list of hotkeys
					if (combo.Handlers == null)
					{
						_combos.Remove(combo);

						// Null the reference so we can tell we've deregistered
						// the hotkey
						combo = null;
					}
				});
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
