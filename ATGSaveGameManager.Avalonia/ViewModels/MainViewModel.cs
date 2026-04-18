using ATGSaveGameManager.Avalonia.Models;
using ATGSaveGameManager.Avalonia.ViewModels;
using ATGSaveGameManager.Configuration;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmppDotNet;
using XmppDotNet.Extensions.Client.Message;
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

        private XmppClient _client;
        private PubSubManager _pubSubManager;


        private ISecureStorage _secureStorage;
        private readonly string SecretName = "ATGSaveGameManager.XmppPassword";
        public MainViewModel()
        {
            DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            IsAvailable = true;
            IsCreatingNewGame = false;

            GameOverviewViewModel = new GameOverviewViewModel(this);
            NewGameViewModel = new NewGameViewModel(this);
            SetupViewModel = new SetupViewModel(this);
            Status = "Not connected";
            _secureStorage = SecureStorageFactory.Create();
        }


        public async Task InitializeAsync()
        {
            await LoadAppSettings();
        }

        [RelayCommand]
        public void OpenSettings()
        {
            // SetupViewModel.SetCurrentSettings(_appSettings);
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
            try
            {
                //Status = "Loading settings";
                ReadAppSettings();

                Connection = await _secureStorage.RetrieveAsync(SecretName);
                PlayerName = _appSettings.Player;

                GetGameTypes();

                CheckSettings();

                SetupViewModel.SetCurrentSettings(_appSettings, Connection);
                // GameOverviewViewModel.LoadGames();

                if (!string.IsNullOrWhiteSpace(_appSettings.Player) && !string.IsNullOrWhiteSpace(Connection))
                    await ConnectXmpp(_appSettings.Player, Connection);
            }
            catch (Exception ex)
            {
                await MessageBoxManager
                              .GetMessageBoxStandard("Error", $"{ex.Message}\r\n{ex.StackTrace}")
                              .ShowAsync();
            }

        }

        private async Task ConnectXmpp(string jid, string password)
        {
            // setup XmppClient with some properties
            _client = new XmppClient(
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
            _pubSubManager = new PubSubManager(_client, "saves.bobbinhold.net");
            // subscribe to the Binded session state
            _client
                      .StateChanged
                      .Where(s => s == SessionState.Binded)
                      .Subscribe(async v =>
                      {

                          LoadServerGames(await _pubSubManager.ListNodesAsync());
                          Status = "Connected";
                          // send our online presence to the server
                          await _client.SendPresenceAsync(Show.Chat, "free for chat");

                      });

            XNamespace nsPubSub = "http://jabber.org/protocol/pubsub#event";

            _client
              .XmppXElementReceived
                .Where(el => el is Message msg &&
                             msg.Element(nsPubSub + "event") != null)
                  .Subscribe(async el =>
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
                              LoadServerGames(await _pubSubManager.ListNodesAsync());
                          }
                      }
                  });
            // connect so the server
            Status = "Connecting";
            await _client.ConnectAsync();

        }

        private void LoadServerGames(List<GameInfoModel> listNodesAsync)
        {
            Dispatcher.UIThread.InvokeAsync(async () => { await GameOverviewViewModel.LoadGames(listNodesAsync); });

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
            await _secureStorage.StoreAsync(SecretName, selectedConnection);
            _appSettings.GamesTypes = gameTypes.ToArray();

            using (var file = File.CreateText(_appsettingsName))
            {
                file.Write(JsonSerializer.Serialize(_appSettings));
            }

            // Reload settings
            await LoadAppSettings();
            SetupViewModel.SetCurrentSettings(_appSettings, selectedConnection);
        }

        public async Task DeleteNode(string id)
        {
            await _pubSubManager.DeleteNode(id);
            LoadServerGames(await _pubSubManager.ListNodesAsync());
        }

        public async Task Download(string id)
        {
            var game = GameOverviewViewModel.GameList.FirstOrDefault(x => x.Model.Id == id);
            if (game != null)
            {
                var type = game.GameTypeObject;

                await _pubSubManager.DownloadFile(game.Model, type);
                game.Status = "Downloaded";
            }
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

                await _pubSubManager.UploadGameTurn(game.Model, type, game.NextPlayer);
                game.Status = "Uploaded";
                if(!string.IsNullOrWhiteSpace(game.Model.MucName))
                {
                    await _client.SendGroupChatMessageAsync(game.Model.MucName, $"Turn Done, next player is {game.GetNextPlayer(game.NextPlayer)}");
                }
            }
        }
    }
}
