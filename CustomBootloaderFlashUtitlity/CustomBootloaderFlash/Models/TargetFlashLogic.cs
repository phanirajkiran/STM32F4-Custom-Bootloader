using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;
using System.IO;

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
        public double FlashedBytes
        {
            get { return _flashedBytes; }
            set { SetProperty(ref _flashedBytes, value); }
        }

        /// <summary>
        /// Boolean for if the flash is in progress
        /// </summary>
        public bool IsFlashInProgress { get; set; } = false;

        public string FileLocation { get; set; }
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
            FlashedBytes = 0;
            TargetConnect(portName, baud);
            while (IsFlashInProgress == true)
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

            _stateAction = new Action[7, 2]
            {
                //Next Success, Next Fail
                { Hookup, TargetDisconnectFailure},  // Connect State
                { Erase, TargetDisconnectFailure},  // Hookup State
                { Write, TargetDisconnectFailure},  // Erase State
                { Check, TargetDisconnectFailure},  // Write State
                { TargetDisconnectSuccess, TargetDisconnectFailure }, // Check state
                { null, null}, // Target Disconnect Success State
                { null, null}, // Target Disconnect Failure State
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
        private double _flashedBytes;

        /// <summary>
        /// Possible responses from the target
        /// </summary>
        private enum TargetResponse
        {
            ACK = 0x06,
            NACK = 0x16
        };

        /// <summary>
        /// Possible commands for the target
        /// </summary>
        private enum TargetCommands
        {
            Erase = 0x43,
            Write = 0x31,
            Check = 0x51,
            Jump = 0xA1,
        };

        private enum TargetSectors
        {
            SECTOR_0 = 0,
            SECTOR_1,
            SECTOR_2,
            SECTOR_3,
            SECTOR_4,
            SECTOR_5,
            SECTOR_6,
            SECTOR_7
        };
        #endregion

        #region Private Functions
        /// <summary>
        /// Connects to the target
        /// </summary>
        private void TargetConnect(string portName, int baud)
        {
            TargetDisconnect();
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baud;
            _currentState = ProcessState.Connect;

            try
            {
                _serialPort.Open();
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                Logger.Log("Target connected");
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
            IsFlashInProgress = false;
        }

        /// <summary>
        /// Communication hookup state between utility and target
        /// Let target know to move into booloader state
        /// </summary>
        private void Hookup()
        {
            _currentState = ProcessState.Hookup;
            Logger.Log("Hooking up communication...");

            byte[] tx = new byte[2];
            byte[] tmp = new byte[2];

            // TODO: Send reset command

            // Wait for ACK from target device
            SerialRead(tmp, 0, 2);
            if (tmp[0] != (byte)TargetResponse.ACK)
            {
                _command = Command.Next_Fail;
            }
            else
            {
                tx[0] = (byte)TargetResponse.ACK;
                tx[1] = CalculateChecksum(tx, 1);
                SerialWrite(tx, 0, 2);
                _command = Command.Next_Sucess;
            }
        }

        /// <summary>
        /// Communicates to the target device to perform a FLASH erase operation
        /// </summary>
        private void Erase()
        {
            _currentState = ProcessState.Erase;
            Logger.Log("Erasing flash...");

            //return;

            byte[] tx = new byte[3];
            byte[] tmp = new byte[2];

            // Send the Erase command
            tx[0] = (byte)TargetCommands.Erase;
            tx[1] = CalculateChecksum(tx, 1);
            SerialWrite(tx, 0, 2);

            // Wait for ACK or NACK
            SerialRead(tmp, 0, 2);
            if (tmp[0] != (byte)TargetResponse.ACK)
            {
                // Invalid ACK received
                Logger.Log("Error erasing flash!");
                _command = Command.Next_Fail;
                return;
            }

            tx[0] = 6;                              // Erase 6 sectors (Sector 2 - 7)
            tx[1] = (byte)TargetSectors.SECTOR_2;   // Initial sector to begin erase
            tx[2] = CalculateChecksum(tx, 2);       // Checksum
            SerialWrite(tx, 0, 3);

            // Wait for ACK or NACK
            SerialRead(tmp, 0, 2);
            if (tmp[0] != (byte)TargetResponse.ACK)
            {
                // Invalid ACK received
                Logger.Log("Error erasing flash!");
                _command = Command.Next_Fail;
            }
            else
            {
                Logger.Log("Flash erase success!");
                _command = Command.Next_Sucess;
            }

        }

        // Write to the target device
        private void Write()
        {
            _currentState = ProcessState.Write;
            Logger.Log("Writing to flash...");

            //return;

            // Read bin file
            byte[] bin = ReadFile();
            int totalBytes = bin.Length; // the total number of bytes to flash
            int totalBytesFlashed = 0;     // the total number of bytes flashed to the target

            byte[] tx = new byte[5];
            byte[] tmp = new byte[2];

            Int32 startAddress = 0x08008000;

            while (totalBytesFlashed < totalBytes)
            {
                #region Establishing Write Command
                // Send the Write command
                tx[0] = (byte)TargetCommands.Write;
                tx[1] = CalculateChecksum(tx, 1);
                SerialWrite(tx, 0, 2);

                // Wait for ACK or NACK
                SerialRead(tmp, 0, 2);
                if (tmp[0] != (byte)TargetResponse.ACK)
                {
                    // Invalid ACK received
                    Logger.Log("Error writing to flash!");
                    _command = Command.Next_Fail;
                    return;
                }
                #endregion

                #region Establishing and sending write address
                //Send start address and checksum
                byte[] startAddressByte = BitConverter.GetBytes(startAddress);
                startAddressByte.CopyTo(tx, 0);
                tx[4] = CalculateChecksum(tx, 4);

                SerialWrite(tx, 0, 5);
                #endregion

                #region Sending Number of Bytes to send
                tx[0] = 4;
                tx[1] = CalculateChecksum(tx, 1);
                SerialWrite(tx, 0, 2);
                #endregion

                #region Establishing and sending data to be sent
                // Will be sending 4 bytes of data at a time
                for (int i = 0; i < 4; i++)
                {
                    tx[i] = bin[i + totalBytesFlashed];
                }
                tx[4] = CalculateChecksum(tx, 4); //Get the checksum

                SerialWrite(tx, 0, 5);
                #endregion

                #region Determining if write was successful or not
                // Wait for ACK or NACK
                SerialRead(tmp, 0, 2);
                if (tmp[0] != (byte)TargetResponse.ACK)
                {
                    // Invalid ACK received
                    // Write was not successful
                    Logger.Log("Error writing to flash!");
                    _command = Command.Next_Fail;
                    return;
                }
                else
                {
                    // Successful: update the starting address and the totalbytesflashed
                    startAddress += 4;
                    totalBytesFlashed += 4;
                    FlashedBytes = totalBytesFlashed;
                }
                #endregion
            }

            Logger.Log("Flash write success!");
            _command = Command.Next_Sucess;

        }

        private void Check()
        {
            byte[] tx = new byte[5];
            byte[] tmp = new byte[2];

            _currentState = ProcessState.Check;
            Logger.Log("Checking flash...");

            #region Establishing Check Command
            // Send the Write command
            tx[0] = (byte)TargetCommands.Check;
            tx[1] = CalculateChecksum(tx, 1);
            SerialWrite(tx, 0, 2);

            // Wait for ACK or NACK
            SerialRead(tmp, 0, 2);
            if (tmp[0] != (byte)TargetResponse.ACK)
            {
                // Invalid ACK received
                Logger.Log("Error checking flash!");
                _command = Command.Next_Fail;
                return;
            }
            #endregion

            #region Establishing and sending start address
            Int32 startAddress = 0x08008000;
            //Send start address and checksum
            byte[] startAddressByte = BitConverter.GetBytes(startAddress);
            startAddressByte.CopyTo(tx, 0);
            tx[4] = CalculateChecksum(tx, 4);

            SerialWrite(tx, 0, 5);

            // Wait for ACK or NACK
            SerialRead(tmp, 0, 2);
            if (tmp[0] != (byte)TargetResponse.ACK)
            {
                // Invalid ACK received
                Logger.Log("Error checking flash!");
                _command = Command.Next_Fail;
                return;
            }
            #endregion

            #region Establishing and ending start address
            byte[] bin = ReadFile();
            Int32 endAddress = startAddress + bin.Length;
            //Send start address and checksum
            byte[] endAddressByte = BitConverter.GetBytes(endAddress);
            endAddressByte.CopyTo(tx, 0);
            tx[4] = CalculateChecksum(tx, 4);

            SerialWrite(tx, 0, 5);

            // Wait for ACK or NACK
            SerialRead(tmp, 0, 2);
            if (tmp[0] != (byte)TargetResponse.ACK)
            {
                // Invalid ACK received
                Logger.Log("Error checking flash!");
                _command = Command.Next_Fail;
                return;
            }
            #endregion

            #region Waiting for CRC Check Result
            // Wait for ACK or NACK
            SerialRead(tmp, 0, 2);
            if (tmp[0] != (byte)TargetResponse.ACK)
            {
                // Invalid ACK received
                Logger.Log("Error checking flash!");
                _command = Command.Next_Fail;
            }
            else
            {
                Logger.Log("Flash check successful!");
                _command = Command.Next_Sucess;
            }
            #endregion
        }

        /// <summary>
        /// Tells target to jump to main application
        /// </summary>
        private void Jump()
        {
            byte[] tx = new byte[2];
            // Send the Erase command
            tx[0] = (byte)TargetCommands.Jump;
            tx[1] = CalculateChecksum(tx, 1);
            SerialWrite(tx, 0, 2);
        }

        // Reads the firmware file and returns it
        private byte[] ReadFile()
        {
            byte[] bin;
            using (var s = new FileStream(FileLocation, FileMode.Open, FileAccess.Read))
            {
                /* allocate memory */
                bin = new byte[s.Length];

                /* read file contents */
                s.Read(bin, 0, bin.Length);
            }

            return bin;
        }

        /// <summary>
        /// Asynchronous read from serial port
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private void SerialRead(byte[] buffer, int offset, int count)
        {
            var bs = _serialPort.BaseStream;
            int br = 0;

            while (br < count)
            {
                br += bs.Read(buffer, offset + br, count - br);
            }
        }

        /// <summary>
        /// Asycnhronously writes to the target
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private void SerialWrite(byte[] data, int offset, int count)
        {
            var bs = _serialPort.BaseStream;
            bs.Write(data, offset, count);
        }

        /// <summary>
        /// Returns an 8-bit two's complement XOR checksum
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte CalculateChecksum(byte[] data, int count)
        {
            byte checksum = 0xFF;
            for (int i = 0; i < count; i++)
            {
                checksum ^= data[i];
            }

            checksum ^= 0xFF;

            return checksum;
        }
        #endregion

        #region State Machine Related
        /// <summary>
        /// Defines the states of the state machine
        /// </summary>
        private enum ProcessState
        {
            Connect,
            Hookup,
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
