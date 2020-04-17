using ATGSaveGameManager.Azure;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ATGSaveGameManager.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IConfiguration _configuration;
        public RelayCommand StartCommand { get; private set; }
        public RelayCommand NewGameCommand { get; private set; }
        public RelayCommand BackCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand SelectFileCommand { get; private set; }
        public RelayCommand AddPlayerCommand { get; private set; }

        private FileSystemWatcher watcher;
        private int i = 0;
        private ConcurrentDictionary<string, FileInfoModel> files = new ConcurrentDictionary<string, FileInfoModel>();
        private ConcurrentDictionary<string, FileInfoModel> remoteFiles = new ConcurrentDictionary<string, FileInfoModel>();
        private List<string> remoteGames = new List<string>();
        private DispatcherTimer timer;
        private string connection;
        private string directory;
        private string dataDirectory;
        private bool isCreatingNewGame;
        private bool _isAvailable;
        private string _newGameName;
        private string _newGameFileName;
        private string _newGameAddPlayer;
        private string _lastSyncTime;


        private string _selectedName;
        private string _selectedFile;
        private string _playerName;

        public bool IsCreatingNewGame
        {
            get
            {
                return isCreatingNewGame;
            }
            set
            {
                if (isCreatingNewGame != value)
                {
                    isCreatingNewGame = value;
                    RaisePropertyChanged(nameof(IsCreatingNewGame));
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
        public string LastSyncTime
        {
            get
            {
                return _lastSyncTime;
            }
            set
            {
                if (_lastSyncTime != value)
                {
                    _lastSyncTime = value;
                    RaisePropertyChanged(nameof(LastSyncTime));
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

        public string NewGameAddPlayer
        {
            get
            {
                return _newGameAddPlayer;
            }
            set
            {
                if (_newGameAddPlayer != value)
                {
                    _newGameAddPlayer = value;
                    RaisePropertyChanged(nameof(NewGameAddPlayer));
                }
            }
        }

        public string NewGameFileName
        {
            get
            {
                return _newGameFileName;
            }
            set
            {
                if (_newGameFileName != value)
                {
                    _newGameFileName = value;
                    RaisePropertyChanged(nameof(NewGameFileName));
                }
            }
        }
        public MainViewModel()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            StartCommand = new RelayCommand(Start, null);
            NewGameCommand = new RelayCommand(NewGame, null);
            BackCommand = new RelayCommand(Back, null);
            SaveCommand = new RelayCommand(Save, null);
            SelectFileCommand = new RelayCommand(SelectFile, null);
            AddPlayerCommand = new RelayCommand(AddPlayer, null);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            connection = _configuration.GetConnectionString("BlobStorageKey");
            directory = _configuration.GetValue<string>("Directory");
            PlayerName = _configuration.GetValue<string>("Player");
            GameList = new ObservableCollection<GameInfoModel>();
            NewGamePlayers = new ObservableCollection<string>();
            dataDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\data";

            LoadGames();
            IsAvailable = true;
        }

        private void AddPlayer()
        {
            NewGamePlayers.Add(NewGameAddPlayer);
            NewGameAddPlayer = "";
        }

        private void SelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Save files (*.at2)|*.at2|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = directory;
            if (openFileDialog.ShowDialog() == true)
            {
                NewGameFileName = openFileDialog.FileName;
            }
        }

        private void Save()
        {
            var gameinfo = new GameInfoModel
            {
                FileName = Path.GetFileName(NewGameFileName),
                Name = NewGameName,
                Players = NewGamePlayers.ToArray()
            };
            var jsonObject = JsonConvert.SerializeObject(gameinfo);

            File.WriteAllText($"{dataDirectory}\\{NewGameName}.json", jsonObject);
            IsCreatingNewGame = false;

        }

        private void Back()
        {
            IsCreatingNewGame = false;
        }

        private void LoadGames()
        {
            Directory.CreateDirectory(dataDirectory);
            var fileList = Directory.GetFiles(dataDirectory);
            foreach (var file in fileList)
            {
                var info = LoadJson(file);
                if (files.ContainsKey(info.FileName))
                {
                    info.File = files[info.FileName];
                }
                GameList.Add(info);
            }
        }

        private ObservableCollection<GameInfoModel> _gameList;
        public ObservableCollection<GameInfoModel> GameList
        {
            get
            {
                if (_gameList == null)
                {
                    _gameList = new ObservableCollection<GameInfoModel>();
                }
                return _gameList;
            }
            set
            {
                _gameList = value;
                RaisePropertyChanged("GameList");
            }
        }

        private ObservableCollection<string> _newGamePlayers;
        public ObservableCollection<string> NewGamePlayers
        {
            get
            {
                if (_newGamePlayers == null)
                {
                    _newGamePlayers = new ObservableCollection<string>();
                }
                return _newGamePlayers;
            }
            set
            {
                _newGamePlayers = value;
                RaisePropertyChanged("NewGamePlayers");
            }
        }


        public string SelectedName
        {
            get
            {
                return _selectedName;
            }
            set
            {
                if (_selectedName != value)
                {
                    _selectedName = value;
                    RaisePropertyChanged("SelectedName");
                }
            }
        }

        public string SelectedFile
        {
            get
            {
                return _selectedFile;
            }
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    RaisePropertyChanged("SelectedFile");
                }
            }
        }
        private void NewGame()
        {
            IsCreatingNewGame = true;
        }

        public GameInfoModel LoadJson(string file)
        {
            using (StreamReader r = new StreamReader(file))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<GameInfoModel>(json);
            }
        }

        private void Start()
        {
            IsAvailable = false;
            if (string.IsNullOrWhiteSpace(connection))
            {
                MessageBox.Show("Error: Connection to Azure blob storage not filled in. Please add key to the appsettings.json file");
                return;
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                MessageBox.Show("Error: Directory to scan for save files not filled in. Please add key to the appsettings.json file");
                return;
            }
            files.Clear();
            remoteFiles.Clear();
            GameList.Clear();
            remoteGames.Clear();
            LoadGames();
            IndexLocally();
            CheckRemote();
            DownloadNewGames();
            GameList.Clear();
            LoadGames();
            SyncDifferences();
            GameList.Clear();
            LoadGames();
            //CreateWatcher();
            IsAvailable = true;
            LastSyncTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        }

        private void DownloadNewGames()
        {
            var service = new BlobStorageService(connection);
            foreach (var game in remoteGames)
            {
                if (!GameList.Any(p => p.FileName.Equals(game)))
                {
                    service.DownloadFile(game, $"{dataDirectory}\\{game}");
                }
            }
        }

        private void CreateWatcher()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }
            watcher = new FileSystemWatcher(directory);
            watcher.Filter = "*.at2";

            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
            //Enable events
            watcher.EnableRaisingEvents = true;

            //Add event watcher
            watcher.Changed += OnChanged;
        }

        private void SyncDifferences()
        {
            var service = new BlobStorageService(connection);


            List<string> keys = remoteFiles.Keys.Union(files.Keys).ToList();
            foreach (string key in keys)
            {
                var game = _gameList.FirstOrDefault(p => p.FileName == key);

                if (game == null)
                {
                    continue;
                }
                var gameName = $"{game.Name}.json";
                var gamePath = $"{dataDirectory}\\{gameName}";
                var gameMd5 = GetFileHash(gamePath);

                var gameStatus = GameStatus.DoNothing;

                FileInfoModel remote = null;
                FileInfoModel local = null;

                if (remoteFiles.ContainsKey(key))
                {
                    remote = remoteFiles[key];
                }

                if (files.ContainsKey(key))
                {
                    local = files[key];
                }

                if (remote == null)
                {
                    var bytes = File.ReadAllBytes(local.FullPath);
                    var bytesGame = File.ReadAllBytes(gamePath);
                    service.UploadFileToBlob(key, local.Md5, bytes, "application/zip");
                    files[key].FileStatus = FileStatus.Uploaded;
                    remoteFiles.TryAdd(key, local);
                    gameStatus = GameStatus.Upload;

                }
                else if (local != null && remote.LastModified < local.LastModified && remote.Md5 != local.Md5)
                {
                    var bytes = File.ReadAllBytes(local.FullPath);
                    service.UploadFileToBlob(key, local.Md5, bytes, "application/zip");
                    files[key].FileStatus = FileStatus.Uploaded;
                    remoteFiles[key] = local;
                    gameStatus = GameStatus.Upload;
                }

                if (local == null)
                {
                    var fileinfoModel = service.DownloadFile(key, $"{directory}\\{key}");
                    files.TryAdd(key, fileinfoModel);
                    files[key].FileStatus = FileStatus.New;
                    gameStatus = GameStatus.Download;
                }
                else if (local == null || (remote != null && local.LastModified < remote.LastModified) && remote.Md5 != local.Md5)
                {
                    var fileinfoModel = service.DownloadFile(key, $"{directory}\\{key}");
                    files[key] = fileinfoModel;
                    files[key].FileStatus = FileStatus.Downloaded;
                    gameStatus = GameStatus.Download;
                }

                if (local?.Md5 == remote?.Md5)
                {
                    files[key].FileStatus = FileStatus.NotChanged;
                }

                if (gameStatus == GameStatus.Download)
                {
                    service.DownloadFile(gameName, gamePath);
                }
                else if (gameStatus == GameStatus.Upload)
                {
                    game.LastPlayer = _playerName;
                    var jsonObject = JsonConvert.SerializeObject(game);
                    File.WriteAllText(gamePath, jsonObject);
                    var hash = GetFileHash(gamePath);
                    var bytes = File.ReadAllBytes(gamePath);
                    service.UploadFileToBlob(gameName, hash, bytes, "application/json");
                }
            }
        }

        private void CheckRemote()
        {
            var service = new BlobStorageService(connection);
            var temp = service.GetFiles();
            foreach (var file in temp)
            {
                if (file.Key.EndsWith(".json"))
                {
                    remoteGames.Add(file.Key);
                }
                else
                {
                    remoteFiles.TryAdd(file.Key, file.Value);
                }
            }
        }

        private void IndexLocally()
        {
            var fileList = Directory.GetFiles(directory);
            foreach (var file in fileList)
            {
                var info = new FileInfo(file);
                var fileInfoModel = new FileInfoModel
                {
                    FullPath = file,
                    Name = info.Name,
                    LastModified = info.LastWriteTimeUtc,
                    Md5 = GetFileHash(file)
                };
                files.TryAdd(info.Name, fileInfoModel);
            }
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!e.Name.Equals("autosave.at2"))
            {

                timer.Tick += new EventHandler(async (object s, EventArgs a) =>
                {
                    timer.Stop();
                    OnUpdate(source, e);
                });
                timer.Start();
            }
            // Specify what is done when a file is changed, created, or deleted.
        }

        private void OnUpdate(object source, FileSystemEventArgs e)
        {
            var info = new FileInfo(e.Name);
            var hash = GetFileHash(e.FullPath);
            var service = new BlobStorageService(connection);

            if (!files.ContainsKey(e.Name))
            {
                var newFile = new FileInfoModel
                {
                    FullPath = e.FullPath,
                    Md5 = hash,
                    Name = e.Name,
                    LastModified = info.LastWriteTimeUtc
                };

                files[e.Name] = newFile;

                var file = File.ReadAllBytes(e.FullPath);

                service.UploadFileToBlob(e.Name, hash, file, "application/zip");
                remoteFiles.TryAdd(e.Name, newFile);
            }
            else
            {
                var currentFile = files[e.Name];
                if (currentFile.Md5 != hash)
                {
                    var file = File.ReadAllBytes(e.FullPath);

                    service.UploadFileToBlob(e.Name, hash, file, "application/zip");
                    currentFile.Md5 = hash;
                    currentFile.LastModified = info.LastWriteTimeUtc;
                    files[e.FullPath] = currentFile;
                }
            }
        }

        private string GetFileHash(string fileName)
        {
            string hash;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
        }
    }
}
