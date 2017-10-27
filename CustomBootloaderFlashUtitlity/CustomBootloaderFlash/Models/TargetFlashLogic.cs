using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace CustomBootloaderFlash.Models
{
    public class TargetFlashLogic : BindableBase
    {
        #region Public Fields
        /// <summary>
        /// Boolean for target connection
        /// </summary>
        public bool IsTargetConnected
        {
            get { return _isTargetConnected; }
            set { _isTargetConnected = value; }
        }

        /// <summary>
        /// Logger class for simple logging
        /// </summary>
        public Logger Logger
        {
            get { return _logger; }
            set { SetProperty(ref _logger, value); }
        }

        /// <summary>
        /// Size of the flash file in kb
        /// </summary>
        public long FlashSize
        {
            get { return _flashSize; }
            set { SetProperty(ref _flashSize, value); }
        }

        /// <summary>
        /// The size of the flash that has been downloaded to target, in kb
        /// </summary>
        public long FlashedBytes
        {
            get { return _flashedBytes; }
            set { SetProperty(ref _flashedBytes, value); }
        }

        /// <summary>
        /// Boolean for if the flash is in progress
        /// </summary>
        public bool IsFlashInProgress { get; set; }
        #endregion

        #region Public Functions

        public void TestConnection(string portName, int baud)
        {
            Logger.Clear();
            Logger.Log("Testing connection...");
            Logger.Log($"Port: {portName}    Baud: {baud}");

            TargetConnect(portName, baud);
            TargetDisconnect();
        }

        /// <summary>
        /// Returns a list of port names for the serial port
        /// </summary>
        /// <returns></returns>
        public List<string> GetPorts()
        {
            return new List<string>(SerialPort.GetPortNames().ToList());
        }

        /// <summary>
        /// Returns a list of the available bauds to connect to the target
        /// </summary>
        public List<int> GetBaudRates()
        {

            return new List<int>()
            {
                115200
            };
        }

        public void StartFlash(string portName, int baud)
        {
            IsFlashInProgress = true;
            Logger.Logs.Clear();
            TargetConnect(portName, baud);
            while(IsFlashInProgress == true)
            {
                ExecuteState(_command);
            }
        }
        #endregion

        #region Constructors
        public TargetFlashLogic()
        {
            _serialPort = new SerialPort()
            {
                Parity = Parity.None,
                DataBits = 8,
                Handshake = Handshake.None,
                RtsEnable = false
            };

            Logger = Logger.Instance;

            _stateAction = new Action[6, 2]
            {
                //Next Success, Next Fail
                { Erase, TargetDisconnectFailure},  // Connect State
                { Write, TargetDisconnectFailure},  // Erase State
                { Check, TargetDisconnectFailure},  // Write State
                {TargetDisconnectSuccess, TargetDisconnectFailure }, // Check state
                {null, null}, // Target Disconnect Success State
                {null, null}, // Target Disconnect Failure State
            };
        }
        #endregion

        #region Private Fields
        /// <summary>
        /// Serial port oject to connect to target via serial port
        /// </summary>
        private SerialPort _serialPort;
        /// <summary>
        /// Boolean for target connection
        /// </summary>
        private bool _isTargetConnected = false;

        /// <summary>
        /// Logger class for simple logging
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// Size of the flash file
        /// </summary>
        private long _flashSize;

        /// <summary>
        /// The size of the flash that has been downloaded to target, in kb
        /// </summary>
        private long _flashedBytes;
        #endregion

        #region Private Functions
        /// <summary>
        /// Connects to the target
        /// </summary>
        private void TargetConnect(string portName, int baud)
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baud;
            _currentState = ProcessState.Connect;

            try
            {
                _serialPort.Open();
                Logger.Log("Connected to target.");
                IsTargetConnected = true;
                _command = Command.Next_Sucess;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to connect to target.");
                _command = Command.Next_Fail;
            }
            
        }

        /// <summary>
        /// Disconnects from the target device
        /// </summary>
        private void TargetDisconnect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            if (_serialPort.IsOpen == false)
            {
                IsTargetConnected = false;
                Logger.Log("Disconnected from target.");
            }
        }

        private void TargetDisconnectSuccess()
        {
            _currentState = ProcessState.Disconnect_Sucess;
            Logger.Log("Flash Success!");
            TargetDisconnect();
            IsFlashInProgress = false;
        }

        private void TargetDisconnectFailure()
        {
            _currentState = ProcessState.Disconnect_Failure;
            Logger.Log("Flash failed!");
            TargetDisconnect();
        }

        private void Erase()
        {
            _currentState = ProcessState.Erase;
            Logger.Log("Erasing flash...");
        }

        private void Write()
        {
            _currentState = ProcessState.Write;
            Logger.Log("Writing to flash...");
        }

        private void Check()
        {
            _currentState = ProcessState.Check;
            Logger.Log("Checking flash...");
        }
        #endregion

        #region State Machine Related
        /// <summary>
        /// Defines the states of the state machine
        /// </summary>
        private enum ProcessState
        {
            Connect,
            Erase,
            Write,
            Check,
            Disconnect_Sucess,
            Disconnect_Failure,
        }

        /// <summary>
        /// Defines the possible state processes for the state machines
        /// </summary>
        private enum Command
        {
            Next_Sucess,   // Move to next state with success result
            Next_Fail,   // Move to the next state with failed result
        }

        /// <summary>
        /// The current state
        /// </summary>
        private ProcessState _currentState;

        private Command _command;

        /// <summary>
        /// Table for FSM
        /// </summary>
        private Action[,] _stateAction;

        private void ExecuteState(Command command)
        {
            _stateAction[(int)_currentState, (int)command].Invoke();
        }
        #endregion
    }
}
