using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PassWinmenu.Configuration
{
    /// <summary>
    /// Represents a configuration file.
    /// </summary>
    internal sealed class ConfigFile
    {
        /// <summary>
        /// Represents the result from an attempt to open, load, or create a
        /// configuration file.
        /// </summary>
        [Flags]
        public enum Result
        {
            /// <summary>
            /// A configuration file was successfully opened.
            /// </summary>
            Success         = 0b0000_0001,
            /// <summary>
            /// A configuration file was created with the default values.
            /// </summary>
            Created         = 0b0000_0010 | Success,
            /// <summary>
            /// The loaded configuration file uses an older format and requires
            /// an upgrade to the current format.
            /// </summary>
            NeedsUpgrade    = 0b0000_0100 | Success,

            /// <summary>
            /// The configuration file could not be opened.
            /// </summary>
            CouldNotOpen    = 0b0000_1000,
        }


        /// <summary>
        /// Attempts to open a specified configuration file, creating it if it
        /// does not already exist.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        public static Result OpenOrCreate(string filePath, out ConfigFile cf)
        {
            throw new NotImplementedException();
        }


        private readonly FileSystemWatcher _fsWatcher;
        private readonly FileStream        _fileStream;


        /// <summary>
        /// The contents of the configuration file, as a defined list of
        /// settings.
        /// </summary>
        public Config Contents
        {
            get; private set;
        }


        /// <summary>
        /// 
        /// </summary>
        public event Action Updated;
    }
}
