using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBootloaderFlash.Models
{
    public class Logger
    {
        #region Public Fields
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Contains a list of strings for the log
        /// </summary>
        public ObservableCollection<string> Logs { get; private set; }
        #endregion

        #region Public Functions
        /// <summary>
        /// Adds a message to the log
        /// </summary>
        /// <param name="log"></param>
        public void Log(string log)
        {
            Logs.Add(log);
        }

        /// <summary>
        /// Clears the log
        /// </summary>
        public void Clear()
        {
            Logs.Clear();
        }
        #endregion

        #region Constructors
        private Logger()
        {
            Logs = new ObservableCollection<string>();
        }
        #endregion

        #region Private Fields
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static Logger _instance;
        #endregion

        #region Private Functions
        #endregion
    }
}
