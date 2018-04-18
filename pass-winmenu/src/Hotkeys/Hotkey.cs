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
            : IDisposable
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
            private bool _retrieve(out Hotkey hotkey)
            {
                if (_hotkey != null)
                {
                    hotkey = _hotkey;
                    return false;
                }

                _hotkey = new Hotkey(
                    deregister: _registrar.Register(
                        modifierKeys:   this.ModifierKeys,
                        key:            this.Key,
                        repeats:        this.Repeats,
                        firedHandler:   (s, e) => _hotkey._firedHandler(s, e)
                        ),
                    mods:       this.ModifierKeys,
                    key:        this.Key,
                    repeats:    this.Repeats
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

            private Hotkey _hotkey;
            private IHotkeyRegistrar _registrar = DefaultRegistrar;

            internal Builder(
                ModifierKeys mods, Key key, bool repeats
                )
            {
                this.ModifierKeys = mods;
                this.Key = key;
                this.Repeats = repeats;
            }
            

            /// <summary>
            /// The modifier keys which are to be pressed with 
            /// <see cref="Key"/> in order to trigger the hotkey.
            /// </summary>
            public ModifierKeys ModifierKeys
            {
                get;
            }
            /// <summary>
            /// The key that is to be pressed with <see cref="ModifierKeys"/> in
            /// order to trigger the hotkey.
            /// </summary>
            public Key Key
            {
                get;
            }
            /// <summary>
            /// Whether the hotkey is to fire multiple times if the key
            /// combination is held down continuously.
            /// </summary>
            public bool Repeats
            {
                get;
            }

            /// <summary>
            /// Provides access to <see cref="Hotkey.Triggered"/>. Operations
            /// on this event will trigger hotkey building.
            /// </summary>
            public event EventHandler Triggered
            {
                add
                {
                    _retrieve(out var hk);

                    hk.Triggered += value;
                }
                remove
                {
                    _retrieve(out var hk);

                    hk.Triggered -= value;
                }
            }

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
            public Builder With(IHotkeyRegistrar registrar)
            {
                _throwIfBuilt();

                _registrar = registrar ?? DefaultRegistrar;

                return this;

                //Hotkey hotkey = null;

                //hotkey = new Hotkey((registrar ?? DefaultRegistrar).Register(
                //    modifierKeys: this.Modifiers,
                //    key:          this.Key,
                //    repeats:      this.Repeats,
                //    firedHandler: (s, e) => hotkey._firedHandler(s, e)
                //    ), this.Modifiers, this.Key, this.Repeats);

                //return hotkey;
            }

            /// <summary>
            /// Unregisters the hotkey. Calling this method will trigger hotkey
            /// building.
            /// </summary>
            public void Dispose()
            {
                _retrieve(out var hk);

                hk.Dispose();
            }
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
        /// Registers a hotkey.
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
        public static Builder Register(
            ModifierKeys modifiers, Key key, bool repeats = true
            )
        {
            return new Builder(modifiers, key, repeats);
        }
        /// <summary>
        /// Registers a hotkey.
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
        public static Builder Register(
            Key key, bool repeats = true
            )
        {
            return Hotkey.Register(ModifierKeys.None, key, repeats);
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
