using ATGSaveGameManager.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using ATGSaveGameManager.Avalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATGSaveGameManager.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        private AppSettings _appSettings;
        [ObservableProperty]
        private string _connection;
        [ObservableProperty]
        private string _dataDirectory;
        [ObservableProperty]

        private bool _isCreatingNewGame;
        [ObservableProperty]

        private bool _isAvailable;
        [ObservableProperty]
        private bool _isSetup;
        [ObservableProperty]

        private string _playerName;
        [ObservableProperty]
        private GameOverviewViewModel _gameOverviewViewModel;
        [ObservableProperty]
        private NewGameViewModel _newGameViewModel;
        [ObservableProperty]
        private SetupViewModel _setupViewModel;
        [ObservableProperty]
        private ObservableCollection<GameType> _gameTypes;
        
        private const string _appsettingsName = "appsettings.json";
        public MainViewModel()
        {
            DataDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\data";
            IsAvailable = true;
            IsCreatingNewGame = false;

            GameOverviewViewModel = new GameOverviewViewModel(this);
            NewGameViewModel = new NewGameViewModel(this);
            SetupViewModel = new SetupViewModel(this);

            LoadAppSettings();
        }

        [RelayCommand]
        private void OpenSettings()
        {
            SetupViewModel.SetCurrentSettings(_appSettings);
            IsSetup = true;
        }

        internal void AddedNewGame()
        {
            IsCreatingNewGame = false;
            GameOverviewViewModel.LoadGames();
        }

        private void LoadAppSettings()
        {
            ReadAppSettings();

            Connection = _appSettings.ConnectionStrings.BlobStorageKey;
            PlayerName = _appSettings.Player;

            GetGameTypes();

            CheckSettings();

            SetupViewModel.SetCurrentSettings(_appSettings);
            GameOverviewViewModel.LoadGames();
        }

        private void ReadAppSettings()
        {
            if (File.Exists(_appsettingsName))
            {
                using (StreamReader r = new StreamReader(_appsettingsName))
                {
                    string json = r.ReadToEnd();
                    _appSettings = JsonSerializer.Deserialize<AppSettings>(json);
                }
            }
            else
            {
                _appSettings = new AppSettings();
            }
        }

        public void NewGame()
        {
            IsCreatingNewGame = true;
        }

        private void CheckSettings()
        {
            if (string.IsNullOrWhiteSpace(PlayerName) || string.IsNullOrWhiteSpace(_connection) || GameTypes.Count == 0)
            {
                IsSetup = true;
            }
            else
            {
                IsSetup = false;
            }
        }
        

        public bool GameOverviewVisible => !IsSetup && !IsCreatingNewGame;
        public bool NewGameCreatingVisible => !IsSetup && IsCreatingNewGame;
        
        //
        // public bool IsSetup
        // {
        //     get
        //     {
        //         return _isSetup;
        //     }
        //     set
        //     {
        //         if (_isSetup != value)
        //         {
        //             _isSetup = value;
        //             RaisePropertyChanged(nameof(IsSetup));
        //             RaisePropertyChanged(nameof(GameOverviewVisible));
        //             RaisePropertyChanged(nameof(NewGameCreatingVisible));
        //         }
        //     }
        // }
        //
        //
        //
        // public bool IsCreatingNewGame
        // {
        //     get
        //     {
        //         return _isCreatingNewGame;
        //     }
        //     set
        //     {
        //         if (_isCreatingNewGame != value)
        //         {
        //             _isCreatingNewGame = value;
        //             RaisePropertyChanged(nameof(IsCreatingNewGame));
        //             RaisePropertyChanged(nameof(GameOverviewVisible));
        //             RaisePropertyChanged(nameof(NewGameCreatingVisible));
        //         }
        //     }
        // }
        //
        //
        // public bool IsAvailable
        // {
        //     get
        //     {
        //         return _isAvailable;
        //     }
        //     set
        //     {
        //         if (_isAvailable != value)
        //         {
        //             _isAvailable = value;
        //             RaisePropertyChanged(nameof(IsAvailable));
        //         }
        //     }
        // }
        //
        // public string PlayerName
        // {
        //     get
        //     {
        //         return _playerName;
        //     }
        //     set
        //     {
        //         if (_playerName != value)
        //         {
        //             _playerName = value;
        //             RaisePropertyChanged(nameof(PlayerName));
        //         }
        //     }
        // }
        //

        private void GetGameTypes()
        {
            //for_appSettings.GameTypes;
            foreach (var gametype in _appSettings.GamesTypes)
            {
                GameTypes.Add(gametype);
            }
            
        }

        public void UpdateAppsettings(string selectedPlayerName, string selectedConnection, IEnumerable<GameType> gameTypes)
        {
            _appSettings.Player = selectedPlayerName;
            _appSettings.ConnectionStrings.BlobStorageKey = selectedConnection;
            _appSettings.GamesTypes = gameTypes.ToArray();

            using (var file = File.CreateText(_appsettingsName))
            {
                file.Write(JsonSerializer.Serialize(_appSettings));
            }

            // Reload settings
            LoadAppSettings();
        }
        
    }
}
