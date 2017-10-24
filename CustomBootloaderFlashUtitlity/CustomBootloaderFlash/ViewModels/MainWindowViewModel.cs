using Prism.Mvvm;
using CustomBootloaderFlash.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Prism.Commands;
using System;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CustomBootloaderFlash.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private Members
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
        /// String to be shown on the Connect button
        /// </summary>
        private string _button_Connect_Text = "Connect!";

        /// <summary>
        /// Whether the connect button is enabled or not 
        /// </summary>
        private bool _button_Connect_IsEnabled = true;
        
        #endregion

        /// <summary>
        /// The Selected Comport Value
        /// </summary>
        private string _selectedComPort;

        private string _filepath;
        /// <summary>
        /// the main window model
        /// </summary>
        private MainWindowModel _mainWindowModel;

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
        /// Title of the main window
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        #region Connect Button
        /// <summary>
        /// String to be shown on the Connect button
        /// </summary>
        public string Button_Connect_Text
        {
            get { return _button_Connect_Text; }
            set { SetProperty(ref _button_Connect_Text, value); }
        }

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
        public DelegateCommand Connect_Command { get; private set; }
        #endregion

        #region Comport and Baud Rate
        /// <summary>
        /// List of the available com ports
        /// </summary>
        public ObservableCollection<string> ComPorts { get; set; } = new ObservableCollection<string>();
        /// <summary>
        /// List of the available baud rates
        /// </summary>
        public ObservableCollection<int> BaudRates { get; set; }
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
                Connect_Command.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Progress Bar
        /// <summary>
        /// the max value for the progress bar
        /// </summary>
        public long ProgressBar_Maximum
        {
            get { return _progressbar_maximum; }
            set { SetProperty(ref _progressbar_maximum, value); }
        }

        /// <summary>
        /// the minimum value for the progress bar
        /// </summary>
        public long ProgressBar_Minimum
        {
            get { return _progressbar_minimum; }
            set { SetProperty(ref _progressbar_minimum, value); }
        }
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
            _mainWindowModel = new MainWindowModel();
            ComPorts = _mainWindowModel.GetComPorts();
            BaudRates = new ObservableCollection<int>(_mainWindowModel.BaudRates);

            #region Delegate Commands
            Connect_Command = new DelegateCommand(Connect_CommandExecute, Connect_CommandCanExecute);
            BrowseFile_Command = new DelegateCommand(BrowseFile_CommandExecute).ObservesCanExecute(() => BrowseFile_IsEnabled);
            Flash_Command = new DelegateCommand(Flash_CommandExecute, Flash_CommandCanExecute);
            #endregion

            #region Dispatch Timer
            // Dispatch timer for polling purposes
            System.Windows.Threading.DispatcherTimer dispatchTimer = new System.Windows.Threading.DispatcherTimer();
            dispatchTimer.Tick += (sender, e) => 
            {
                Flash_Command.RaiseCanExecuteChanged();
                ProgressBar_Current = _mainWindowModel.TotalBytesProcessed;
            };
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatchTimer.Start();
            #endregion

        }

        #endregion

        #region Private functions
        #region Connect Button
        private void Connect_CommandExecute()
        {
            if (_mainWindowModel.Target_Connect(SelectedComPort, SelectedBaudRate))
            {
                System.Diagnostics.Trace.TraceInformation("Connection Successful!");
                _isTargetConnected = true;
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation("Unable to connect to target");
                _isTargetConnected = false;
            }
        }

        private bool Connect_CommandCanExecute()
        {

            if (String.IsNullOrEmpty(SelectedComPort))
                Button_Connect_IsEnabled = false;
            else
                Button_Connect_IsEnabled = true;

            return Button_Connect_IsEnabled;
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
            string filesize= fileInfo.Length.ToString();

            filesize = string.Format("{0}", fileInfo.Length >> 10);
            ProgressBar_Maximum = (fileInfo.Length >> 10);
        }
        #endregion

        #region Flash Command

        /// <summary>
        /// Determines if the flash command is enabled
        /// </summary>
        /// <returns></returns>
        private bool Flash_CommandCanExecute()
        {
            bool result = false;

            if (string.IsNullOrEmpty(FilePath) || _isTargetConnected == false || _forceFlashButtonDisable == true)
                result = false;
            else
                result = true;

            return result;
        }

        /// <summary>
        /// Command to execute when the flash button is clicked
        /// </summary>
        private async void Flash_CommandExecute()
        {
            _forceFlashButtonDisable = true;
            await Task.Run(() =>
            {
                _mainWindowModel.Target_FlashStart();
            });
            _forceFlashButtonDisable = false;
        }
        #endregion

        #endregion

    }
}
