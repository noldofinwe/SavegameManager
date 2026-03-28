using ATGSaveGameManager.Configuration;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Linq;

namespace ATGSaveGameManager.ViewModel
{
    public class SetupViewModel : PbemViewModelBase
    {
        private string _selectedPlayerName;
        private string _selectedConnection;
        private string _newGameName;
        private string _newGameExtension;
        private string _newGameSaveFolder;
        private string _newGameIcon;
        private bool _adding;
        private ObservableCollection<GameTypeViewModel> _gameTypes;
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand UpdateCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand SelectDirectoryCommand { get; private set; }
        public RelayCommand SelectIconCommand { get; private set; }
        public RelayCommand SaveSettingsCommand { get; private set; }
        public RelayCommand AddNewCommand { get; private set; }
        private GameTypeViewModel _selectedGameTypeViewModel;

        public SetupViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
            SaveSettingsCommand = new RelayCommand(SaveSettings, null);
            UpdateCommand = new RelayCommand(Update, null);
            DeleteCommand = new RelayCommand(Delete, null);
            SelectDirectoryCommand = new RelayCommand(SelectDirectory, null);
            AddNewCommand = new RelayCommand(AddNew, null);
            CancelCommand = new RelayCommand(CancelAdd, null);
            AddCommand = new RelayCommand(Add, null);
            SelectIconCommand = new RelayCommand(SelectIcon, null);
        }

        private void Add()
        {
            if(string.IsNullOrWhiteSpace(NewGameExtension))
            {
                MessageBox.Show("Game extension is empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(NewGameSaveGame))
            {
                MessageBox.Show("Game Save game folder is empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(NewGameName))
            {
                MessageBox.Show("Game Name is empty.");
                return;
            }
    
            var gameType = new GameType
            {
                Name = NewGameName,
                Extension = NewGameExtension,
                Icon = NewGameIcon,
                Savegames = NewGameSaveGame
            };

            GameTypes.Add(new GameTypeViewModel(gameType));
            Adding = false;
        }

        private void CancelAdd()
        {
            NewGameExtension = null;
            NewGameSaveGame = null;
            NewGameIcon = null;
            NewGameName = null;
            Adding = false;
        }

        private void AddNew()
        {
            Adding = true;
        }

        private void Delete()
        {
            GameTypes.Remove(SelectedGameTypeViewModel);
            SelectedGameTypeViewModel = null;
        }

        private void Update()
        {
            SelectedGameTypeViewModel = null;
        }

        public void SetCurrentSettings(AppSettings appSettings)
        {
            GameTypes.Clear();
            CancelAdd();
            SelectedPlayerName = appSettings.Player;
            SelectedConnection = appSettings.ConnectionStrings.BlobStorageKey;
            SelectedConnection = appSettings.ConnectionStrings?.BlobStorageKey;

            if (appSettings.GamesTypes != null)
            {
                foreach (var type in appSettings.GamesTypes)
                {
                    var viewModel = new GameTypeViewModel(type);
                    GameTypes.Add(viewModel);
                }
            }
        }

        private void SelectDirectory()
        {
            var openFolder = new CommonOpenFileDialog();
            openFolder.AllowNonFileSystemItems = true;
            openFolder.Multiselect = false;
            openFolder.IsFolderPicker = true;
            openFolder.Title = "Select the game Savegame folder";

            if (openFolder.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageBox.Show("No Folder selected");
                return;
            }

            // get all the directories in selected dirctory
            if (UpdateVisible)
            {
                SelectedGameTypeViewModel.Model.Savegames = openFolder.FileName;
            }
            else
            {
                NewGameSaveGame = openFolder.FileName;
            }
        }

        private void SelectIcon()
        {
            var openFolder = new CommonOpenFileDialog();
            openFolder.AllowNonFileSystemItems = true;
            openFolder.Multiselect = false;
            openFolder.IsFolderPicker = false;
            openFolder.Title = "Select the game Icon";

            if (openFolder.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageBox.Show("No Icon selected");
                return;
            }

            if (UpdateVisible)
            {
                SelectedGameTypeViewModel.Model.Icon = openFolder.FileName;
            }
            else
            {
                NewGameIcon = openFolder.FileName;
            }
        }

        public void SaveSettings()
        {
            if (string.IsNullOrWhiteSpace(SelectedPlayerName))
            {
                MessageBox.Show("Player name is empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedConnection))
            {
                MessageBox.Show("No Azure blob storage connection has been selected, check readme on how to create Azure blob storage and how to find connection string.");
                return;
            }
            if (GameTypes.Count == 0)
            {
                MessageBox.Show("No games have been selected.");
                return;
            }

            _mainViewModel.UpdateAppsettings(SelectedPlayerName, SelectedConnection, GameTypes.Select(p => p.Model));
        }

        public string SelectedPlayerName
        {
            get
            {
                return _selectedPlayerName;
            }
            set
            {
                if (_selectedPlayerName != value)
                {
                    _selectedPlayerName = value;
                    RaisePropertyChanged(nameof(SelectedPlayerName));
                }
            }
        }

        public GameTypeViewModel SelectedGameTypeViewModel
        {
            get
            {
                return _selectedGameTypeViewModel;
            }
            set
            {
                if (_selectedGameTypeViewModel != value)
                {
                    _selectedGameTypeViewModel = value;
                    RaisePropertyChanged(nameof(SelectedGameTypeViewModel));
                    RaisePropertyChanged(nameof(UpdateVisible));
                    RaisePropertyChanged(nameof(CanAddVisible));
                    RaisePropertyChanged(nameof(AddVisible));
                }
            }
        }


        public string NewGameName
        {
            get
            {
                return _newGameName;
            }
            set
            {
                if (_newGameName != value)
                {
                    _newGameName = value;
                    RaisePropertyChanged(nameof(NewGameName));
                }
            }
        }

        public string NewGameExtension
        {
            get
            {
                return _newGameExtension;
            }
            set
            {
                if (_newGameExtension != value)
                {
                    _newGameExtension = value;
                    RaisePropertyChanged(nameof(NewGameExtension));
                }
            }
        }
        public string NewGameSaveGame
        {
            get
            {
                return _newGameSaveFolder;
            }
            set
            {
                if (_newGameSaveFolder != value)
                {
                    _newGameSaveFolder = value;
                    RaisePropertyChanged(nameof(NewGameSaveGame));
                }
            }
        }
        public string NewGameIcon
        {
            get
            {
                return _newGameIcon;
            }
            set
            {
                if (_newGameIcon != value)
                {
                    _newGameIcon = value;
                    RaisePropertyChanged(nameof(NewGameIcon));
                }
            }
        }


        public string SelectedConnection
        {
            get
            {
                return _selectedConnection;
            }
            set
            {
                if (_selectedConnection != value)
                {
                    _selectedConnection = value;
                    RaisePropertyChanged(nameof(SelectedConnection));
                }
            }
        }
        
        public bool Adding
        {
            get
            {
                return _adding;
            }
            set
            {
                if (_adding != value)
                {
                    _adding = value;
                    RaisePropertyChanged(nameof(Adding));
                    RaisePropertyChanged(nameof(CanAddVisible));
                    RaisePropertyChanged(nameof(AddVisible));
                }
            }
        }

        public bool UpdateVisible => SelectedGameTypeViewModel != null;
        public bool CanAddVisible => SelectedGameTypeViewModel == null && !Adding;
        public bool AddVisible => SelectedGameTypeViewModel == null && Adding;

        public ObservableCollection<GameTypeViewModel> GameTypes
        {
            get
            {
                if (_gameTypes == null)
                {
                    _gameTypes = new ObservableCollection<GameTypeViewModel>();
                }
                return _gameTypes;
            }
            set
            {
                _gameTypes = value;
                RaisePropertyChanged(nameof(GameTypes));
            }
        }

    }
}
