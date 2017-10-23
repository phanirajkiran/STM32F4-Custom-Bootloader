using Prism.Mvvm;
using CustomBootloaderFlash.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Prism.Commands;
using System;
using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace CustomBootloaderFlash.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private Members
        /// <summary>
        /// Title of the main window
        /// </summary>
        private string _title = "Custom Bootloader Flash Utility by William Huang";

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

            Connect_Command = new DelegateCommand(Connect_CommandExecute, Connect_CommandCanExecute);
            BrowseFile_Command = new DelegateCommand(BrowseFile_CommandExecute).ObservesCanExecute(() => BrowseFile_IsEnabled);

        }

        
        #endregion

        #region Private functions
        #region Connect Button
        private void Connect_CommandExecute()
        {
            if (_mainWindowModel.Target_Connect(SelectedComPort, SelectedBaudRate))
            {
                MessageBox.Show("Connection Successful!");
            }
            else
            {
                MessageBox.Show("Unable to connect to target");
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
        #endregion

    }
}
