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
        /// Represents a request to register a hotkey with a
        /// <see cref="IHotkeyRegistrar"/>.
        /// </summary>
        public sealed class RegistrationRequest
        {
            /// <summary>
            /// Converts a request to register a hotkey to a hotkey using
            /// the <see cref="DefaultRegistrar"/>.
            /// </summary>
            /// <param name="rr">
            /// The request to convert to a hotkey.
            /// </param>
            public static implicit operator Hotkey(RegistrationRequest rr)
            {
                return rr.With(DefaultRegistrar);
            }


            internal RegistrationRequest(
                ModifierKeys mods, Key key, bool repeats
                )
            {
                this.Modifiers = mods;
                this.Key = key;
                this.Repeats = repeats;
            }
            

            /// <summary>
            /// The modifier keys which are to be pressed with 
            /// <see cref="Key"/> in order to trigger the hotkey.
            /// </summary>
            public ModifierKeys Modifiers
            {
                get;
            }
            /// <summary>
            /// The key that is to be pressed with <see cref="Modifiers"/> in
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
            /// Specifies a registrar with which to register the hotkey.
            /// </summary>
            /// <param name="registrar">
            /// The <see cref="IHotkeyRegistrar"/> with which the hotkey is
            /// to be registered.
            /// </param>
            /// <returns>
            /// A hotkey registered with the specified registrar.
            /// </returns>
            public Hotkey With(IHotkeyRegistrar registrar)
            {
                Hotkey hotkey = null;

                hotkey = new Hotkey((registrar ?? DefaultRegistrar).Register(
                    modifierKeys: this.Modifiers,
                    key:          this.Key,
                    repeats:      this.Repeats,
                    firedHandler: (s, e) => hotkey._firedHandler(s, e)
                    ), this.Modifiers, this.Key, this.Repeats);

                return hotkey;
            }
        }



        private static IHotkeyRegistrar _defaultRegistrar;

        /// <summary>
        /// The default <see cref="IHotkeyRegistrar"/> to be used when no
        /// registrar is specified with a request to register a hotkey.
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
        public static RegistrationRequest Register(
            ModifierKeys modifiers, Key key, bool repeats = true
            )
        {
            return new RegistrationRequest(modifiers, key, repeats);
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
        public static RegistrationRequest Register(
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
