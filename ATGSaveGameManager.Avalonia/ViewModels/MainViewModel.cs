using ATGSaveGameManager.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using ATGSaveGameManager.Avalonia.ViewModels;
using ATGSaveGameManager.Avalonia.Xmpp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XmppDotNet;
using XmppDotNet.Extensions.Client.Message;
using XmppDotNet.Extensions.Client.Presence;
using XmppDotNet.Extensions.Client.Roster;
using XmppDotNet.Transport.Socket;
using XmppDotNet.Xmpp;

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
        private ObservableCollection<GameType> _gameTypes =[];
        
        private const string _appsettingsName = "appsettings.json";

        private XmppClient _client;
        private PubSubManager _pubSubManager;
        
        public MainViewModel()
        {
            DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            IsAvailable = true;
            IsCreatingNewGame = false;
            
            GameOverviewViewModel = new GameOverviewViewModel(this);
            NewGameViewModel = new NewGameViewModel(this);
            SetupViewModel = new SetupViewModel(this);

        }
        
    

        public async Task InitializeAsync()
        {
            await LoadAppSettings();
        }

    [RelayCommand]
        public void OpenSettings()
        {
            SetupViewModel.SetCurrentSettings(_appSettings);
            IsSetup = true;
        }

        internal void AddedNewGame(GameInfoModel model)
        {
            _pubSubManager.CreateGame(model);
            IsCreatingNewGame = false;
            GameOverviewViewModel.LoadGames();
        }

        private async Task LoadAppSettings()
        {
            ReadAppSettings();

            Connection = _appSettings.Password;
            PlayerName = _appSettings.Player;

            GetGameTypes();

            CheckSettings();

            SetupViewModel.SetCurrentSettings(_appSettings);
            GameOverviewViewModel.LoadGames();
            if(!string.IsNullOrWhiteSpace(_appSettings.Player) && !string.IsNullOrWhiteSpace(_appSettings.Password))
               await ConnectXmpp(_appSettings.Player, _appSettings.Password);
        }

        private async Task ConnectXmpp(string jid, string password)
        {
            // setup XmppClient with some properties
            var xmppClient = new XmppClient(
                conf =>
                {
                    conf.UseSocketTransport();
                    //conf.UseWebSocketTransport();
                    conf.AutoReconnect = true;

                    // when your server dow not support SRV records or
                    // XEP-0156 Discovering Alternative XMPP Connection Methods
                    // then you need to supply host and port for the connection as well.
                    // See docs => Host disconvery
                }
            )
            {
                Jid = jid,
                Password = password
            };

            // subscribe to the Binded session state
            xmppClient
                .StateChanged
                .Where(s => s == SessionState.Binded)
                .Subscribe(async v =>
                {
          
        
                    // send our online presence to the server
                    await xmppClient.SendPresenceAsync(Show.Chat, "free for chat");

                   });

            // connect so the server
            await xmppClient.ConnectAsync();
            _pubSubManager = new PubSubManager(xmppClient, "saves.bobbinhold.net");
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
            if (string.IsNullOrWhiteSpace(PlayerName) || string.IsNullOrWhiteSpace(Connection) || GameTypes.Count == 0)
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
        
        partial void OnIsSetupChanged(bool value)
        {
            OnPropertyChanged(nameof(GameOverviewVisible));
            OnPropertyChanged(nameof(NewGameCreatingVisible));
            
        }

        partial void OnIsCreatingNewGameChanged(bool value)
        {
            OnPropertyChanged(nameof(GameOverviewVisible));
            OnPropertyChanged(nameof(NewGameCreatingVisible));
        }
      

        private void GetGameTypes()
        {
            //for_appSettings.GameTypes;
            foreach (var gametype in _appSettings.GamesTypes)
            {
                GameTypes.Add(gametype);
            }
            
        }
        
        

        public async Task UpdateAppsettings(string selectedPlayerName, string selectedConnection, IEnumerable<GameType> gameTypes)
        {
            _appSettings.Player = selectedPlayerName;
            _appSettings.Password = selectedConnection;
            _appSettings.GamesTypes = gameTypes.ToArray();

            using (var file = File.CreateText(_appsettingsName))
            {
                file.Write(JsonSerializer.Serialize(_appSettings));
            }

            // Reload settings
            await LoadAppSettings();
            SetupViewModel.SetCurrentSettings(_appSettings);
        }
        
    }
}
