using ATGSaveGameManager.Avalonia.Models;
using ATGSaveGameManager.Avalonia.ViewModels;
using ATGSaveGameManager.Configuration;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using XmppDotNet;
using XmppDotNet.Extensions.Client.Presence;
using XmppDotNet.Transport.Socket;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Client;

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
        private string _status;
        [ObservableProperty]
        private string _playerName;
        [ObservableProperty]
        private GameOverviewViewModel _gameOverviewViewModel;
        [ObservableProperty]
        private NewGameViewModel _newGameViewModel;
        [ObservableProperty]
        private SetupViewModel _setupViewModel;
        [ObservableProperty]
        private ObservableCollection<GameType> _gameTypes = [];

        private const string _appsettingsName = "appsettings.json";

        private readonly XmppClient _client;
        private PubSubManager _pubSubManager;

        public MainViewModel()
        {
            DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            IsAvailable = true;
            IsCreatingNewGame = false;

            GameOverviewViewModel = new GameOverviewViewModel(this);
            NewGameViewModel = new NewGameViewModel(this);
            SetupViewModel = new SetupViewModel(this);
            Status = "Not connected";

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

        internal async Task AddedNewGame(GameInfoModel model, GameType gameType)
        {
            await _pubSubManager.CreateGame(model, gameType);
            IsCreatingNewGame = false;
            LoadServerGames(await _pubSubManager.ListNodesAsync());
        }

        private async Task LoadAppSettings()
        {
            //Status = "Loading settings";
            ReadAppSettings();

            Connection = _appSettings.Password;
            PlayerName = _appSettings.Player;

            GetGameTypes();

            CheckSettings();

            SetupViewModel.SetCurrentSettings(_appSettings);
            // GameOverviewViewModel.LoadGames();
            if (!string.IsNullOrWhiteSpace(_appSettings.Player) && !string.IsNullOrWhiteSpace(_appSettings.Password))
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
            _pubSubManager = new PubSubManager(xmppClient, "saves.bobbinhold.net");
            // subscribe to the Binded session state
            xmppClient
                      .StateChanged
                      .Where(s => s == SessionState.Binded)
                      .Subscribe(async v =>
                      {

                          LoadServerGames(await _pubSubManager.ListNodesAsync());
                          Status = "Connected";
                          // send our online presence to the server
                          await xmppClient.SendPresenceAsync(Show.Chat, "free for chat");

                      });

            XNamespace nsPubSub = "http://jabber.org/protocol/pubsub#event";

            xmppClient
              .XmppXElementReceived
                .Where(el => el is Message msg &&
                             msg.Element(nsPubSub + "event") != null)
                  .Subscribe(el =>
                  {
                      var msg = (Message)el;

                      var ev = msg.Element(nsPubSub + "event");

                      var items = ev?.Element(nsPubSub + "items");
                      var item = items?.Element(nsPubSub + "item");

                      // Step 2: extract the payload element
                      var gameInfoElement = item?.Elements().FirstOrDefault();

                      if (gameInfoElement != null)
                      {
                          if (gameInfoElement.Name.LocalName == nameof(GameTurnModel))
                          {

                              // Step 3: deserialize

                              var serializer = new XmlSerializer(typeof(GameTurnModel));
                              using var reader = gameInfoElement.CreateReader();
                              var model = (GameTurnModel)serializer.Deserialize(reader);
                          }
                      }
                  });
            // connect so the server
            Status = "Connecting";
            await xmppClient.ConnectAsync();

        }

        private void LoadServerGames(List<GameInfoModel> listNodesAsync)
        {
            Dispatcher.UIThread.InvokeAsync(() => { GameOverviewViewModel.LoadGames(listNodesAsync); });

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

        public async Task DeleteNode(string id)
        {
            await _pubSubManager.DeleteNode(id);
            LoadServerGames(await _pubSubManager.ListNodesAsync());
        }

        internal async Task SubscribeToNode(string id)
        {
            await _pubSubManager.Subscribe(id);
            LoadServerGames(await _pubSubManager.ListNodesAsync());

        }

        internal async Task UpdateNode(string id)
        {
            var game = GameOverviewViewModel.GameList.FirstOrDefault(x => x.Model.Id == id);
            if (game != null)
            {
                var type = game.GameTypeObject;

                await _pubSubManager.UploadGameTurn(game.Model, type);
            }
        }
    }
}
