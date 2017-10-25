using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace CustomBootloaderFlash.Models
{
    public class MainWindowModel : BindableBase
    {
        #region Private members
        private SerialPort _serialport;
        private int[] _baudrates = { 115200 };
        private bool _flashInProgess = false;
        private byte[] _receivedDataBuffer = new byte[2];
        private bool _receivedDataFlag = false;
        private const int _timeoutTimeMilli = 1000;
        #region Commands to Target
        /// <summary>
        /// The erase command
        /// </summary>
        private byte[] Command_Erase = { 0x43 };
        #endregion

        #endregion

        #region Public Fields
        public int[] BaudRates
        {
            get { return _baudrates; }
        }

        public long TotalBytesProcessed { get; set; } = 0;
        #endregion

        #region Constructurors
        public MainWindowModel()
        {
            _serialport = new SerialPort();
        }

        ~MainWindowModel()
        {
            Target_Disconnect();
        }
        #endregion

        #region Private fuctions
        /// <summary>
        /// Data received from the target
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            if(sp.BytesToRead >= 2)
            {
                sp.Read(_receivedDataBuffer, 0, 2);
                _receivedDataFlag = true;
            }       
        }
        /// <summary>
        /// Retrieves the available com ports in the system
        /// </summary>
        public ObservableCollection<string> GetComPorts()
        {
            return new ObservableCollection<string>(SerialPort.GetPortNames());
        }

        /// <summary>
        /// Sends a command to the target
        /// </summary>
        /// <param name="command_msg"></param>
        private void Target_SendCommand(byte[] command_msg)
        {

        }

        /// <summary>
        /// Returns a 1-byte checksum of the message.
        /// The first byte of the message is XOR with a value of 0xFF.
        /// Subsequent messages are XOR with the previous checksum value.
        /// The result is then returned.
        /// </summary>
        /// <param name="message">The message to create the checksum from</param>
        /// <returns>The checksum of the message</returns>
        private byte GetChecksum(byte[] message)
        {
            byte checksum = 0xFF;

            for (int i = 0; i < message.Length; i++)
            {
                checksum ^= message[i];
            }

            return checksum;
        }

        /// <summary>
        /// Validates if the message is uncorrupted
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ValidateMessage(byte[] msg)
        {
            int checksum = 0xFF;
            for(int i = 0; i < msg.Length; i++)
            {
                checksum ^= msg[i];
            }

            if (checksum == 0x00)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks to see if the received message contains an ACK in the first byte of the array
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>
        /// true if a valid msg and ACK is detected
        /// false if a valid msg and ACK is not detected
        /// </returns>
        private bool CheckForACK(byte[] msg)
        {
            if (ValidateMessage(msg) == true) // Validate Checksum
            {
                // Check if ACK is received
                if (msg[0] == 0x06)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        #region Flashing States
        /// <summary>
        /// Attempts to hookup communication to the target
        /// </summary>
        /// <returns></returns>
        private bool Flash_HookupState()
        {
            bool stateResult = false;
            bool timedout = false;
            bool _continue = true;

            Stopwatch timeoutWatch = new Stopwatch();
            timeoutWatch.Start();
            while (timedout == false && _continue == true)
            {
                if (_receivedDataFlag == true)
                {
                    stateResult = CheckForACK(_receivedDataBuffer);
                    _continue = false;
                }

                timeoutWatch.Stop();
                if(timeoutWatch.ElapsedMilliseconds >= _timeoutTimeMilli)
                {
                    timedout = true;
                }
                else
                {
                    timeoutWatch.Start();
                }
            }

            return stateResult;
        }

        private bool Flash_EraseState()
        {
            bool stateResult = false;
            bool _continue = true;
            Target_SendCommand(Command_Erase);

            while(_continue == true)
            {
                if(_receivedDataFlag == true)
                {
                    if (ValidateMessage(_receivedDataBuffer) == true) // Validate Checksum
                    {
                        // Check if ACK is received
                        if (_receivedDataBuffer[0] == 0x06)
                            stateResult = true;
                        else
                            stateResult = false;

                        _continue = false;
                    }
                }
            }


            return stateResult;
        }
        #endregion
        #endregion

        #region Public Functions
        /// <summary>
        /// Connect to the serial port with the specified comport and baudrate.
        /// All other settigns are default:
        /// No parity, one stopbit, 8 data bits, no flow control.
        /// </summary>
        /// <param name="comport">The name of the COM port to connect to</param>
        /// <param name="baudrate">The baud rate to use for communication</param>
        /// <returns>True if connection is succesfful, otherwise returns false</returns>
        public bool Target_Connect(string comport, int baudrate)
        {
            if(!_serialport.IsOpen)
            {
                _serialport.PortName = comport;
                _serialport.BaudRate = baudrate;
                _serialport.Parity = Parity.None;
                _serialport.StopBits = StopBits.One;
                _serialport.DataBits = 8;
                _serialport.Handshake = Handshake.None;
                _serialport.RtsEnable = false;
                _serialport.DataReceived += new SerialDataReceivedEventHandler(Target_DataReceived);
            }
            
            try
            {
                _serialport.Open();
                Target_Erase(0x01);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        

        /// <summary>
        /// Disconnect from the target
        /// </summary>
        public void Target_Disconnect()
        {
            if (_serialport.IsOpen)
                _serialport.Close();
        }

        /// <summary>
        /// Checks to see if the file size fits within the flash size of the target
        /// </summary>
        /// <returns></returns>
        public bool Flash_CheckSize(long filesize)
        {
            bool result = false;

            if (filesize <= 480)
                result = true;

            return result;
        }

        public void Target_Erase(byte sectors)
        {
            int length = Command_Erase.Length;
            byte[] msg = new byte[length + 1];
            for (int i = 0; i < length; i++)
                msg[i] = Command_Erase[i];

            msg[length] = GetChecksum(Command_Erase);
            
            _serialport.Write(msg, 0, msg.Length);
        }

        /// <summary>
        /// Starts the flashing process 
        /// </summary>
        /// <returns>
        /// 0 if flashing process is successful
        /// 1 if there is an error in hooking up communication
        /// 2 if there is an error in erasing target flash space
        /// 3 if there is an error in downloading to target flash space
        /// 4 if there is an error in checking the validity of the application after download
        /// </returns>
        public int Target_FlashStart()
        {
            bool stateResult = false;

            stateResult = Flash_HookupState();

            if(stateResult == true)
            {
                stateResult = Flash_EraseState();
            }
            else
            {
                return 1;
            }

            if(stateResult == true)
            {
                // Download
            }
            else
            {
                return 2 ;
            }

            if(stateResult == true)
            {
                // Check Image state
            }
            else
            {
                return 3;
            }

            if(stateResult != true)
            {
                return 4;
            }

            // Jump to application command
            // return stateResult; // Final flash result
            return 0;
        }
        #endregion



    }
}
