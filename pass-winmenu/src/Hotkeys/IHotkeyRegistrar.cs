using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PassWinmenu.Hotkeys
{
    /// <summary>
    /// Provides methods for the registration of hotkeys.
    /// </summary>
    public interface IHotkeyRegistrar
    {
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
        /// continuously.
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
        /// An error occured in registering the hotkey.
        /// </exception>
        IDisposable Register(
            ModifierKeys modifierKeys,
            Key          key,
            bool         repeats,
            EventHandler firedHandler
            );
    }
}
