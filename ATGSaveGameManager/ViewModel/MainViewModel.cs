using ATGSaveGameManager.Configuration;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ATGSaveGameManager.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private AppSettings _appSettings;
        public RelayCommand NewGameCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }

        private string _connection;
        private string _dataDirectory;
        private bool _isCreatingNewGame;
        private bool _isAvailable;
        private bool _isSetup;
        private string _playerName;
        private GameOverviewViewModel _gameOverviewViewModel;
        private NewGameViewModel _newGameViewModel;
        private SetupViewModel _setupViewModel;
        private ObservableCollection<GameType> _gameTypes;
        private const string _appsettingsName = "appsettings.json";
        public MainViewModel()
        {
            NewGameCommand = new RelayCommand(NewGame, null);
            OpenSettingsCommand = new RelayCommand(OpenSettings, null);
            DataDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\data";
            IsAvailable = true;
            IsCreatingNewGame = false;

            GameOverviewViewModel = new GameOverviewViewModel(this);
            NewGameViewModel = new NewGameViewModel(this);
            SetupViewModel = new SetupViewModel(this);

            LoadAppSettings();
        }

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
            using (StreamReader r = new StreamReader(_appsettingsName))
            {
                string json = r.ReadToEnd();
                _appSettings = JsonConvert.DeserializeObject<AppSettings>(json);
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

        public GameOverviewViewModel GameOverviewViewModel
        {
            get
            {
                return _gameOverviewViewModel;
            }
            set
            {
                if (_gameOverviewViewModel != value)
                {
                    _gameOverviewViewModel = value;
                    RaisePropertyChanged(nameof(GameOverviewViewModel));
                }
            }
        }

        public bool GameOverviewVisible => !IsSetup && !IsCreatingNewGame;
        public bool NewGameCreatingVisible => !IsSetup && IsCreatingNewGame;


        public NewGameViewModel NewGameViewModel
        {
            get
            {
                return _newGameViewModel;
            }
            set
            {
                if (_newGameViewModel != value)
                {
                    _newGameViewModel = value;
                    RaisePropertyChanged(nameof(NewGameViewModel));
                }
            }
        }


        public SetupViewModel SetupViewModel
        {
            get
            {
                return _setupViewModel;
            }
            set
            {
                if (_setupViewModel != value)
                {
                    _setupViewModel = value;
                    RaisePropertyChanged(nameof(SetupViewModel));
                }
            }
        }


        public bool IsSetup
        {
            get
            {
                return _isSetup;
            }
            set
            {
                if (_isSetup != value)
                {
                    _isSetup = value;
                    RaisePropertyChanged(nameof(IsSetup));
                    RaisePropertyChanged(nameof(GameOverviewVisible));
                    RaisePropertyChanged(nameof(NewGameCreatingVisible));
                }
            }
        }



        public bool IsCreatingNewGame
        {
            get
            {
                return _isCreatingNewGame;
            }
            set
            {
                if (_isCreatingNewGame != value)
                {
                    _isCreatingNewGame = value;
                    RaisePropertyChanged(nameof(IsCreatingNewGame));
                    RaisePropertyChanged(nameof(GameOverviewVisible));
                    RaisePropertyChanged(nameof(NewGameCreatingVisible));
                }
            }
        }


        public bool IsAvailable
        {
            get
            {
                return _isAvailable;
            }
            set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    RaisePropertyChanged(nameof(IsAvailable));
                }
            }
        }

        public string PlayerName
        {
            get
            {
                return _playerName;
            }
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    RaisePropertyChanged(nameof(PlayerName));
                }
            }
        }


        private void GetGameTypes()
        {
            //for_appSettings.GameTypes;
            foreach (var gametype in _appSettings.GamesTypes)
            {
                GameTypes.Add(gametype);
            }
            RaisePropertyChanged(nameof(GameTypes));
        }

        public void UpdateAppsettings(string selectedPlayerName, string selectedConnection, IEnumerable<GameType> gameTypes)
        {
            _appSettings.Player = selectedPlayerName;
            _appSettings.ConnectionStrings.BlobStorageKey = selectedConnection;
            _appSettings.GamesTypes = gameTypes.ToArray();

            using (var file = File.CreateText(_appsettingsName))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, _appSettings);
            }

            // Reload settings
            LoadAppSettings();
        }

        public ObservableCollection<GameType> GameTypes
        {
            get
            {
                if (_gameTypes == null)
                {
                    _gameTypes = new ObservableCollection<GameType>();
                }
                return _gameTypes;
            }
            set
            {
                _gameTypes = value;
                RaisePropertyChanged(nameof(GameTypes));
            }
        }

        public string Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                if (_connection != value)
                {
                    _connection = value;
                    RaisePropertyChanged(nameof(Connection));
                }
            }
        }

        public string DataDirectory
        {
            get
            {
                return _dataDirectory;
            }
            set
            {
                if (_dataDirectory != value)
                {
                    _dataDirectory = value;
                    RaisePropertyChanged(nameof(DataDirectory));
                }
            }
        }


    }
}
