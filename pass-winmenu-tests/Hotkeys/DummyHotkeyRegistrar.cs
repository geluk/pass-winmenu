using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PassWinmenu.TestUtils;

namespace PassWinmenu.Hotkeys
{
    /// <summary>
    /// A dummy, manipulable <see cref="IHotkeyRegistrar"/> for use in testing.
    /// </summary>
    public sealed class DummyHotkeyRegistrar
        : IHotkeyRegistrar
    {
        private readonly IDictionary<(ModifierKeys, Key), EventHandler> _hotkeys;


        /// <summary>
        /// Creates a new dummy hotkey registrar.
        /// </summary>
        public DummyHotkeyRegistrar()
        {
            _hotkeys = new Dictionary<(ModifierKeys, Key), EventHandler>();
        }


        /// <summary>
        /// Whether requests to the registrar are to succeed.
        /// </summary>
        public bool WillSucceed { get; set; } = true;
        /// <summary>
        /// Whether the registrar will accept a request to register multiple
        /// hotkeys with the same key combinations.
        /// </summary>
        /// <remarks>
        /// Changing this property will not affect hotkeys which are already
        /// registered with the registrar.
        /// </remarks>
        public bool AllowMulticast { get; set; } = true;

        /// <summary>
        /// All hotkeys registered with the dummy registrar.
        /// </summary>
        public IReadOnlyDictionary<(ModifierKeys, Key), EventHandler> Hotkeys
        {
            get;
        }

        /// <summary>
        /// Occurs when an <see cref="IDisposable"/> provided to unregister a
        /// hotkey is disposed.
        /// </summary>
        public event EventHandler<(ModifierKeys, Key)> Disposal;


        /// <summary>
        /// Convenience method for triggering all hotkeys registered with the
        /// dummy registrar.
        /// </summary>
        public void FireAll() => Hotkeys.Values.ToList()
                                               .ForEach(eh => eh(this, null));


        IDisposable IHotkeyRegistrar.Register(
            ModifierKeys modifierKeys, Key key, bool repeats,
            EventHandler firedHandler
            )
        {
            // Fail if we're configured to do so
            if (!this.WillSucceed)
            {
                throw new HotkeyException(
                    "The dummy hotkey registrar is configured to fail."
                    );
            }

            // If multicast hotkeys are disabled, check that a hotkey with the
            // specified combination is not already present.
            if (!this.AllowMulticast && this.Hotkeys.ContainsKey((modifierKeys, key)))
            {
                throw new HotkeyException(
                    "Multicast hotkeys are disabled and a hotkey is already " +
                    "registered with this key combination."
                    );
            }

            // If multicast hotkeys are enabled and a hotkey with the same
            // combination is already registered, use its registered delegate
            // as a multicast delegate to call all handlers at once.
            if (!this.Hotkeys.TryGetValue((modifierKeys, key), out var handler))
                handler += firedHandler;
            // Otherwise, we need to use the provided handler as the new value
            // to be inserted into the dictionary.
            else
                handler = firedHandler;

            // Delegates are immutable, so creating a multicast delegate from
            // one already registered will not affect the registered copy. We
            // need to explicitly replace it in the dictionary.
            _hotkeys[(modifierKeys, key)] = handler;

            bool isDisposed = false;
            return new DummyDisposable(() => {
                // Do nothing if we've already been disposed.
                if (isDisposed)
                    return;

                // We don't need to do what we did above as we know that the
                // dictionary will always contain a delegate for us to work
                // with, and because we assign the newly-created multicast
                // delegate right back to the dictionary member.
                var h = (_hotkeys[(modifierKeys, key)] -= firedHandler);

                // Removing the last delegate from a multicast delegate causes
                // the value of that multicast delegate to be null, so we'll 
                // want to remove it from the dictionary to prevent an NRE.
                if (h == null)
                    _hotkeys.Remove((modifierKeys, key));

                isDisposed = true;
            });

            // We ignore the repeat parameter as it doesn't really factor into
            // testing the [Hotkey] class. Repeated triggering as a result of
            // a continuously held down combination would appear no different
            // to the [Hotkey] class than the hotkey being triggered many
            // times very quickly by other means.
        }
    }
}
