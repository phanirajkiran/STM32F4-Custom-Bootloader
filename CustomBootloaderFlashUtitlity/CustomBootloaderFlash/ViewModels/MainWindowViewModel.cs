using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Prism.Commands;
using System;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CustomBootloaderFlash.Models;
using System.Windows.Data;
using log4net;

namespace CustomBootloaderFlash.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private Members
        /// <summary>
        /// Instance of the Target Flash Logic class
        /// </summary>
        private TargetFlashLogic _targetFlashLogic;
        /// <summary>
        /// Title of the main window
        /// </summary>
        private string _title = "Custom Bootloader Flash Utility by William Huang";

        /// <summary>
        /// Boolean for target connection established
        /// </summary>
        private bool _isTargetConnected = false;

        #region Connect Button
        /// <summary>
        /// Whether the connect button is enabled or not 
        /// </summary>
        private bool _button_Connect_IsEnabled = true;

        private readonly ILog _log4netLogger;
        #endregion

        /// <summary>
        /// The Selected Comport Value
        /// </summary>
        private string _selectedComPort;

        private string _filepath;

        #region Progress Bar
        /// <summary>
        /// The maximum value for the progress bar
        /// </summary>
        private long _progressbar_maximum = 256;
        /// <summary>
        /// the minimum value for the progress bar
        /// </summary>
        private long _progressbar_minimum = 0;

        /// <summary>
        /// the current value of the progress bar
        /// </summary>
        private long _progressbar_current = 0;
        #endregion

        /// <summary>
        /// Set to force the flash button to be disabled at all times. Clear to allow logic of
        /// enable/disable to resume normal operation
        /// </summary>
        private bool _forceFlashButtonDisable;
        #endregion

        #region Public Fields
        /// <summary>
        /// Instance of the Target Flash Logic class
        /// </summary>
        public TargetFlashLogic TargetFlashLogic
        {
            get { return _targetFlashLogic; }
            set { SetProperty(ref _targetFlashLogic, value); }
        }

        /// <summary>
        /// Title of the main window
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        #region Connect Button
        /// <summary>
        /// Whether the connect button is enabled or not 
        /// </summary>
        public bool Button_Connect_IsEnabled
        {
            get { return _button_Connect_IsEnabled; }
            set
            {
                SetProperty(ref _button_Connect_IsEnabled, value);
            }
        }

        /// <summary>
        /// Command for the connect button
        /// </summary>
        public DelegateCommand TestConnect_Command { get; private set; }
        #endregion

        #region Comport and Baud Rate
        /// <summary>
        /// List of the available com ports
        /// </summary>
        public List<string> ComPorts { get; set; }
        /// <summary>
        /// List of the available baud rates
        /// </summary>
        public List<int> BaudRates { get; set; }
        /// <summary>
        /// The Selected Baud Rate
        /// </summary>
        public int SelectedBaudRate { get; set; }

        public string SelectedComPort
        {
            get { return _selectedComPort; }
            set
            {
                SetProperty(ref _selectedComPort, value);
                TestConnect_Command.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Progress Bar
        /// <summary>
        /// the current value of the progress bar
        /// </summary>
        public long ProgressBar_Current
        {
            get { return _progressbar_current; }    
            set { SetProperty(ref _progressbar_current, value); }
        }
        #endregion

        #region Browse File
        /// <summary>
        /// Delegate command for the browse file
        /// </summary>
        public DelegateCommand BrowseFile_Command { get; private set; }
        /// <summary>
        /// Whether or not the browse file command is enabled
        /// </summary>
        public bool BrowseFile_IsEnabled { get; set; } = true;
        /// <summary>
        /// The file path of the selected file
        /// </summary>
        public string FilePath
        {
            get { return _filepath; }
            private set
            {
                SetProperty(ref _filepath, value);
                UpdateFileSize();
            }
        }
        #endregion

        #region Flash Button
        /// <summary>
        /// Delegate command for the flash button
        /// </summary>
        public DelegateCommand Flash_Command { get; private set; }
        #endregion

        #endregion

        #region Constructors
        /// <summary>
        /// the default constructor
        /// </summary>
        public MainWindowViewModel()
        {
            TargetFlashLogic = new TargetFlashLogic();
            ComPorts = TargetFlashLogic.GetPorts();
            BaudRates = TargetFlashLogic.GetBaudRates();

            #region Delegate Commands
            TestConnect_Command = new DelegateCommand(TestConnect_CommandExecute, TestConnect_CommandCanExecute);
            BrowseFile_Command = new DelegateCommand(BrowseFile_CommandExecute).ObservesCanExecute(() => BrowseFile_IsEnabled);
            Flash_Command = new DelegateCommand(Flash_CommandExecute, Flash_CommandCanExecute);
            #endregion

            #region Dispatch Timer
            // Dispatch timer for polling purposes
            System.Windows.Threading.DispatcherTimer dispatchTimer = new System.Windows.Threading.DispatcherTimer();
            dispatchTimer.Tick += (sender, e) => 
            {
                Flash_Command.RaiseCanExecuteChanged();
            };
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatchTimer.Start();
            #endregion

        }

        #endregion

        #region Private functions
        #region Test Connect Button
        private void TestConnect_CommandExecute()
        {
            TargetFlashLogic.TestConnection(SelectedComPort, SelectedBaudRate);
        }

        private bool TestConnect_CommandCanExecute()
        {
            if (string.IsNullOrEmpty(SelectedComPort))
                return false;

            return true;
        }
        #endregion

        #region Browse File Button
        private void BrowseFile_CommandExecute()
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = ".bin | *.bin"
            };

            if(fileDialog.ShowDialog() == true)
            {
                FilePath = fileDialog.FileName;
            }
        }

        private void UpdateFileSize()
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            TargetFlashLogic.FlashSize = (fileInfo.Length >> 10);
        }
        #endregion

        #region Flash Command

        /// <summary>
        /// Determines if the flash command is enabled
        /// </summary>
        /// <returns></returns>
        private bool Flash_CommandCanExecute()
        {
            if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(SelectedComPort) || TargetFlashLogic.IsFlashInProgress == true)
                return false;

            return true;
        }

        /// <summary>
        /// Command to execute when the flash button is clicked
        /// </summary>
        private void Flash_CommandExecute()
        {
            TargetFlashLogic.StartFlash(SelectedComPort, SelectedBaudRate);
        }
        #endregion

        #endregion

        
    }
}
