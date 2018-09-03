using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PassWinmenu.Hotkeys
{
    /// <summary>
    /// Represents a hotkey registered.
    /// </summary>
    public sealed class Hotkey
        : IDisposable
    {
        /// <summary>
        /// A utility for building <see cref="Hotkey"/> instances.
        /// </summary>
        public sealed class Builder
        {

            /// <summary>
            /// Builds a <see cref="Hotkey"/> from the configuration of a
            /// specified <see cref="Builder"/>.
            /// </summary>
            /// <param name="b">
            /// The builder to use in creating the <see cref="Hotkey"/>.
            /// </param>
            /// <exception cref="HotkeyException">
            /// An error occured in building the hotkey.
            /// </exception>
            public static implicit operator Hotkey(Builder b)
            {
                b._retrieve(out var hk);

                return hk;
            }



            /// <summary>
            /// Builds a <see cref="Hotkey"/> instance from the configuration
            /// of this <see cref="Builder"/>, or retrieves one built previously.
            /// </summary>
            /// <param name="hotkey">
            /// The <see cref="Hotkey"/> that was built or retrieved.
            /// </param>
            /// <returns>
            /// True if a new <see cref="Hotkey"/> was built, false if one
            /// built previously was retrieved.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The <see cref="Builder"/> has not been given a value for
            /// <see cref="Hotkey.Key"/>.
            /// </exception>
            private bool _retrieve(out Hotkey hotkey)
            {
                if (_hotkey != null)
                {
                    hotkey = _hotkey;
                    return false;
                }

                if (_key == Key.None)
                    throw new InvalidOperationException(
                        $"The Builder was not provided with a {typeof(Key).FullName} " +
                        "to use when creating a Hotkey.");

                _hotkey = new Hotkey(
                    deregister: _registrar.Register(
                        modifierKeys: _modifierKeys,
                        key:           _key,
                        repeats:       _repeats,
                        firedHandler:  (s, e) => _hotkey._firedHandler(s, e)
                        ),
                    mods:    _modifierKeys,
                    key:     _key,
                    repeats: _repeats
                    );

                hotkey = _hotkey;
                return true;
            }
            /// <summary>
            /// Retrieves whether a <see cref="Hotkey"/> has been built.
            /// </summary>
            /// <returns>
            /// True if a hotkey has been built, false if otherwise.
            /// </returns>
            private bool _canRetrieve() => _hotkey != null;
            /// <summary>
            /// Throws an <see cref="InvalidOperationException"/> if a hotkey
            /// has already been built with this builder.
            /// </summary>
            private void _throwIfBuilt()
            {
                if (_canRetrieve())
                {
                    throw new InvalidOperationException(
                        "A hotkey has already been built using this builder."
                        );
                }
            }

            private Hotkey              _hotkey         = null;
            private IHotkeyRegistrar    _registrar      = DefaultRegistrar;
            private ModifierKeys        _modifierKeys   = ModifierKeys.None;
            private Key                 _key            = Key.None;
            private bool                _repeats        = false;
            private EventHandler        _handlers       = null;

            internal Builder() { }


            /// <summary>
            /// Sets the value of <see cref="Hotkey.ModifierKeys"/> for the hotkey
            /// to be built.
            /// </summary>
            /// <param name="modifierKeys">
            /// The value representing the modifier keys to use.
            /// </param>
            /// <param name="cumulative">
            /// Whether this call is cumulative on previous calls. If false, the
            /// stored value of <see cref="Hotkey.ModifierKeys"/> is replaced; if
            /// true, <paramref name="modifierKeys"/> is OR'd with the stored value.
            /// </param>
            /// <exception cref="InvalidOperationException">
            /// A <see cref="Hotkey"/> has already been built.
            /// </exception>
            public Builder WithModifiers(ModifierKeys modifierKeys, bool cumulative)
            {
                _throwIfBuilt();

                if (cumulative)
                    _modifierKeys |= modifierKeys;
                else
                    _modifierKeys = modifierKeys;

                return this;
            }
            /// <summary>
            /// Sets the value of <see cref="Hotkey.ModifierKeys"/> for the hotkey
            /// to be built.
            /// </summary>
            /// <param name="modifierKeys">
            /// The value representing the modifier keys to use.
            /// </param>
            /// <exception cref="InvalidOperationException">
            /// A <see cref="Hotkey"/> has already been built.
            /// </exception>
            public Builder WithModifiers(ModifierKeys modifierKeys)
                => this.WithModifiers(modifierKeys, cumulative: false);

            /// <summary>
            /// Sets the value of <see cref="Hotkey.Key"/> for the hotkey to be
            /// built.
            /// </summary>
            /// <param name="key">
            /// The <see cref="System.Windows.Input.Key"/> to use for <see cref="Hotkey.Key"/>.
            /// </param>
            /// <exception cref="InvalidOperationException">
            /// A <see cref="Hotkey"/> has already been built.
            /// </exception>
            public Builder WithKey(Key key)
            {
                _throwIfBuilt();

                _key = key;

                return this;
            }

            /// <summary>
            /// Sets the value of <see cref="Hotkey.Repeats"/> for the hotkey to
            /// be built.
            /// </summary>
            /// <param name="repeats">
            /// The value to use for <see cref="Hotkey.Repeats"/>.
            /// </param>
            /// <exception cref="InvalidOperationException">
            /// A <see cref="Hotkey"/> has already been built.
            /// </exception>
            public Builder WhichRepeats(bool repeats)
            {
                _throwIfBuilt();

                _repeats = repeats;

                return this;
            }

            /// <summary>
            /// Sets the event handlers to be bound to <see cref="Hotkey.Triggered"/>
            /// for the hotkey to be built.
            /// </summary>
            /// <param name="handlers">
            /// The handlers to bind to <see cref="Hotkey.Triggered"/>.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="handlers"/>, or a <see cref="EventHandler"/> in that
            /// array, is null.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// A <see cref="Hotkey"/> has already been built.
            /// </exception>
            public Builder WithHandlers(params EventHandler[] handlers)
            {
                if (handlers?.Any(handler => handler is null) ?? true)
                    throw new ArgumentNullException(nameof(handlers));

                _throwIfBuilt();

                foreach (var handler in handlers)
                    _handlers += handler;

                return this;
            }
            /// <summary>
            /// Sets the event handler to be bound to <see cref="Hotkey.Triggered"/>
            /// for the hotkey to be built.
            /// </summary>
            /// <param name="handler">
            /// The handler to bind to <see cref="Hotkey.Triggered"/>.
            /// </param>
            /// <param name="cumulative">
            /// If true, <paramref name="handler"/> is bound in addition to any
            /// handlers already bound. Otherwise, <paramref name="handler"/>
            /// replaces any handlers already bound.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="handler"/> is null.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// A <see cref="Hotkey"/> has already been built.
            /// </exception>
            public Builder WithHandler(EventHandler handler, bool cumulative)
            {
                if (handler == null)
                    throw new ArgumentNullException(nameof(handler));

                _throwIfBuilt();

                if (cumulative)
                    _handlers += handler;
                else
                    _handlers = handler;

                return this;
            }
            /// <summary>
            /// Sets the event handler to be bound to <see cref="Hotkey.Triggered"/>
            /// for the hotkey to be built.
            /// </summary>
            /// <param name="handler">
            /// The handler to bind to <see cref="Hotkey.Triggered"/>.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="handler"/> is null.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// A <see cref="Hotkey"/> has already been built.
            /// </exception>
            public Builder WithHandler(EventHandler handler)
                => this.WithHandler(handler, cumulative: false);

            /// <summary>
            /// Specifies a registrar with which to register the hotkey.
            /// </summary>
            /// <param name="registrar">
            /// The <see cref="IHotkeyRegistrar"/> with which the hotkey is
            /// to be registered. If null, the <see cref="DefaultRegistrar"/>
            /// is used.
            /// </param>
            /// <returns>
            /// A hotkey registered with the specified registrar.
            /// </returns>
            /// <exception cref="HotkeyException">
            /// An error occured in registering the hotkey. Refer to
            /// documentation for the particular <see cref="IHotkeyRegistrar"/>
            /// in use.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// A <see cref="Hotkey"/> has already been built.
            /// </exception>
            public Builder For(IHotkeyRegistrar registrar)
            {
                _throwIfBuilt();

                _registrar = registrar ?? DefaultRegistrar;

                return this;
            }

            /// <summary>
            /// Builds a <see cref="Hotkey"/>, or retrieves a hotkey already
            /// built.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// No value for <see cref="Hotkey.Key"/> was provided.
            /// </exception>
            public Hotkey Assemble() => _retrieve(out var hk) ? hk : _hotkey;
        }

        /// <summary>
        /// Provides part of a fluent interface for registering a hotkey with a
        /// specified registrar.
        /// </summary>
        public interface IHotkeyRegistration
        {
            /// <summary>
            /// Registers a hotkey with the specified registrar.
            /// </summary>
            /// <param name="modifiers">
            /// The modifiers which are to be pressed with 
            /// <paramref name="key"/> in order to trigger the hotkey.
            /// </param>
            /// <param name="key">
            /// The key that is to be pressed with the 
            /// <paramref name="modifiers"/> in order to trigger the hotkey.
            /// </param>
            /// <param name="repeats">
            /// Whether the hotkey is to fire multiple times if the key
            /// combination is held down.
            /// </param>
            /// <returns>
            /// A <see cref="RegistrationRequest"/> object which can be used to
            /// specify a particular <see cref="IHotkeyRegistrar"/> to use.
            /// </returns>
            /// <exception cref="HotkeyException">
            /// An error occured in registering the hotkey. Refer to documentation
            /// for the particular <see cref="IHotkeyRegistrar"/> in use.
            /// </exception>
            Hotkey Register(ModifierKeys modifiers, Key key, bool repeats = true);

            /// <summary>
            /// Registers a hotkey with the default registrar.
            /// </summary>
            /// <param name="key">
            /// The key that is to be pressed with the in order to trigger
            /// the hotkey.
            /// </param>
            /// <param name="repeats">
            /// Whether the hotkey is to fire multiple times if the key
            /// combination is held down.
            /// </param>
            /// <returns>
            /// A <see cref="RegistrationRequest"/> object which can be used to
            /// specify a particular <see cref="IHotkeyRegistrar"/> to use.
            /// </returns>
            /// <exception cref="HotkeyException">
            /// An error occured in registering the hotkey. Refer to documentation
            /// for the particular <see cref="IHotkeyRegistrar"/> in use.
            /// </exception>
            Hotkey Register(Key key, bool repeats = true);
        }

        private sealed class HotkeyRegistration
            : IHotkeyRegistration
        {
            private readonly IHotkeyRegistrar _registrar;

            public HotkeyRegistration(IHotkeyRegistrar registrar)
            {
                _registrar = registrar;
            }

            Hotkey IHotkeyRegistration.Register(ModifierKeys modifiers, Key key, bool repeats)
                => Build().For(_registrar)
                          .WithModifiers(modifiers)
                          .WithKey(key)
                          .WhichRepeats(repeats);

            Hotkey IHotkeyRegistration.Register(Key key, bool repeats)
                => Build().For(_registrar)
                          .WithKey(key)
                          .WhichRepeats(repeats);
        }


        private static IHotkeyRegistrar _defaultRegistrar;


        static Hotkey()
        {
            _defaultRegistrar = HotkeyRegistrars.Windows;
        }


        /// <summary>
        /// The <see cref="IHotkeyRegistrar"/> to be used when no registrar is 
        /// specified with a request to register a hotkey. Initialised to
        /// <see cref="HotkeyRegistrars.Windows"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public static IHotkeyRegistrar DefaultRegistrar
        {
            get => _defaultRegistrar;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _defaultRegistrar = value;
            }
        }


        /// <summary>
        /// Returns a <see cref="Builder"/> which can be used to incrementally
        /// create a <see cref="Hotkey"/>.
        /// </summary>
        public static Builder Build() => new Builder();

        /// <summary>
        /// Registers a hotkey with the default registrar.
        /// </summary>
        /// <param name="modifiers">
        /// The modifiers which are to be pressed with 
        /// <paramref name="key"/> in order to trigger the hotkey.
        /// </param>
        /// <param name="key">
        /// The key that is to be pressed with the 
        /// <paramref name="modifiers"/> in order to trigger the hotkey.
        /// </param>
        /// <param name="repeats">
        /// Whether the hotkey is to fire multiple times if the key
        /// combination is held down.
        /// </param>
        /// <returns>
        /// A <see cref="RegistrationRequest"/> object which can be used to
        /// specify a particular <see cref="IHotkeyRegistrar"/> to use.
        /// </returns>
        /// <exception cref="HotkeyException">
        /// An error occured in registering the hotkey. Refer to documentation
        /// for the particular <see cref="IHotkeyRegistrar"/> in use.
        /// </exception>
        public static Hotkey Register(ModifierKeys modifiers, Key key, bool repeats = true)
            => Build().WithModifiers(modifiers)
                      .WithKey(key)
                      .WhichRepeats(repeats);
        /// <summary>
        /// Registers a hotkey with the default registrar.
        /// </summary>
        /// <param name="key">
        /// The key that is to be pressed with the in order to trigger
        /// the hotkey.
        /// </param>
        /// <param name="repeats">
        /// Whether the hotkey is to fire multiple times if the key
        /// combination is held down.
        /// </param>
        /// <returns>
        /// A <see cref="RegistrationRequest"/> object which can be used to
        /// specify a particular <see cref="IHotkeyRegistrar"/> to use.
        /// </returns>
        /// <exception cref="HotkeyException">
        /// An error occured in registering the hotkey. Refer to documentation
        /// for the particular <see cref="IHotkeyRegistrar"/> in use.
        /// </exception>
        public static Hotkey Register(Key key, bool repeats = true)
            => Build().WithKey(key)
                      .WhichRepeats(repeats);

        /// <summary>
        /// Registers a hotkey with the specified registrar.
        /// </summary>
        /// <param name="registrar">
        /// The registrar with which to register the hotkey.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="registrar"/> is null.
        /// </exception>
        public static IHotkeyRegistration With(IHotkeyRegistrar registrar)
        {
            if (registrar == null)
                throw new ArgumentNullException(nameof(registrar));

            return new HotkeyRegistration(registrar);
        }


        // Provides the callback to the registrar to deregister the hotkey.
        private readonly IDisposable _deregister;
        // Whether we've been disposed.
        private bool _disposed;


        // Passed to the registrar as the handler for hotkey triggering.
        private void _firedHandler(object sender, EventArgs data)
        {
            if (!this.Enabled || _disposed)
                return;

            this.Triggered?.Invoke(this, null);
        }

        
        /// <summary>
        /// Creates a new <see cref="Hotkey"/> instance.
        /// </summary>
        /// <param name="deregister">
        /// An <see cref="IDisposable"/> returned from a call to
        /// <see cref="IHotkeyRegistrar.Register(ModifierKeys, Key, bool, EventHandler)"/>
        /// which is used to deregister the hotkey.
        /// </param>
        private Hotkey(
            IDisposable deregister, ModifierKeys mods, Key key, bool repeats
            )
        {
            _deregister = deregister;

            this.ModifierKeys = mods;
            this.Key = key;
            this.Repeats = repeats;
        }


        /// <summary>
        /// Whether the hotkey is to be triggered on the pressing of its key
        /// combination.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The modifier keys that are to be pressed in combination with
        /// <see cref="Key"/> to trigger the hotkey.
        /// </summary>
        public ModifierKeys ModifierKeys { get; }
        /// <summary>
        /// The key that is to be pressed in combination with 
        /// <see cref="ModifierKeys"/> to trigger the hotkey.
        /// </summary>
        public Key Key { get; }
        /// <summary>
        /// Whether continuously holding down the key combination repeatedly
        /// triggers the hotkey.
        /// </summary>
        public bool Repeats { get; }

        /// <summary>
        /// Occurs when the key combination for the hotkey is pressed.
        /// </summary>
        public event EventHandler Triggered;


        /// <summary>
        /// Unregisters the hotkey.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _deregister.Dispose();
            _disposed = true;
        }
    }
}
