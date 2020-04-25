using ATGSaveGameManager.Azure;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ATGSaveGameManager.ViewModel
{
    public class GameOverviewViewModel : PbemViewModelBase
    {
        public RelayCommand StartCommand { get; private set; }
        private ConcurrentDictionary<string, FileInfoModel> files = new ConcurrentDictionary<string, FileInfoModel>();
        private ConcurrentDictionary<string, FileInfoModel> remoteFiles = new ConcurrentDictionary<string, FileInfoModel>();
        private List<string> remoteGames = new List<string>();
        private ObservableCollection<GameInfoViewModel> _gameList;
        private string _lastSyncTime;

        public GameOverviewViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
            StartCommand = new RelayCommand(Start, null);
            GameList = new ObservableCollection<GameInfoViewModel>();
        }


        private void Start()
        {
            _mainViewModel.IsAvailable = false;
            if (string.IsNullOrWhiteSpace(_mainViewModel.Connection))
            {
                MessageBox.Show("Error: Connection to Azure blob storage not filled in. Please add key to the appsettings.json file");
                return;
            }

            files.Clear();
            remoteFiles.Clear();
            GameList.Clear();
            remoteGames.Clear();

            LoadGames();
            CheckRemote();
            DownloadNewGames();
            GameList.Clear();
            LoadGames();
            foreach (var gameType in _mainViewModel.GameTypes)
            {

                IndexLocally(gameType.Savegames);
                SyncDifferences(gameType);
                GameList.Clear();
                LoadGames();
            }
            _mainViewModel.IsAvailable = true;
            LastSyncTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void DownloadNewGames()
        {
            var service = new BlobStorageService(_mainViewModel.Connection);
            foreach (var game in remoteGames)
            {
                if (!GameList.Any(p => p.Model.FileName.Equals(game)))
                {
                    service.DownloadFile(game, $"{_mainViewModel.DataDirectory}\\{game}");
                }
            }
        }


        private void SyncDifferences(GameType gameType)
        {
            var service = new BlobStorageService(_mainViewModel.Connection);


            List<string> keys = remoteFiles.Keys.Union(files.Keys).ToList();
            foreach (string key in keys)
            {
                var gameViewModel = _gameList.FirstOrDefault(p => p.Model.FileName == key && p.Model.GameType == gameType.Extension);

                if (gameViewModel == null)
                {
                    continue;
                }
                var game = gameViewModel.Model;

                if (!game.Players.Contains(_mainViewModel.PlayerName))
                {
                    continue;
                }

                var gameName = $"{game.Name}.json";
                var gamePath = $"{_mainViewModel.DataDirectory}\\{gameName}";
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
                    var fileinfoModel = service.DownloadFile(key, $"{gameType.Savegames}\\{key}");
                    files.TryAdd(key, fileinfoModel);
                    files[key].FileStatus = FileStatus.New;
                    gameStatus = GameStatus.Download;
                }
                else if (local == null || (remote != null && local.LastModified < remote.LastModified) && remote.Md5 != local.Md5)
                {
                    var fileinfoModel = service.DownloadFile(key, $"{gameType.Savegames}\\{key}");
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
                    game.LastPlayer = _mainViewModel.PlayerName;
                    game.GameType = gameType.Extension;
                    game.LastTurnTime = DateTime.UtcNow;
                    if (game.CurrentTurn.HasValue)
                    {
                        if (game.Players[0].Equals(_mainViewModel.PlayerName))
                        {
                            game.CurrentTurn++;
                        }
                    }
                    else
                    {
                        game.CurrentTurn = 1;
                    }
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
            var service = new BlobStorageService(_mainViewModel.Connection);
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

        private void IndexLocally(string directory)
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


        public void LoadGames()
        {
            GameList.Clear();

            Directory.CreateDirectory(_mainViewModel.DataDirectory);
            var fileList = Directory.GetFiles(_mainViewModel.DataDirectory);
            foreach (var file in fileList)
            {
                var info = LoadJson(file);

                var gameType = _mainViewModel.GameTypes.FirstOrDefault(p => p.Extension == info.GameType);
                var gameInfoViewModel = new GameInfoViewModel(info, _mainViewModel.PlayerName);

                if (gameType != null)
                {
                    gameInfoViewModel.GameTypeObject = gameType;
                    gameInfoViewModel.IconImage = new BitmapImage(new Uri(gameType.Icon, UriKind.RelativeOrAbsolute));
                }
                if (files.ContainsKey(info.FileName))
                {
                    gameInfoViewModel.File = files[info.FileName];
                }

                GameList.Add(gameInfoViewModel);
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

        public ObservableCollection<GameInfoViewModel> GameList
        {
            get
            {
                if (_gameList == null)
                {
                    _gameList = new ObservableCollection<GameInfoViewModel>();
                }
                return _gameList;
            }
            set
            {
                _gameList = value;
                RaisePropertyChanged("GameList");
            }
        }

    }
}
