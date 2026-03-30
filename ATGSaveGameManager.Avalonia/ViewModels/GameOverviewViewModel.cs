using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using ATGSaveGameManager.Azure;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATGSaveGameManager.ViewModel
{
    public partial class GameOverviewViewModel : PbemViewModelBase
    {
        private ConcurrentDictionary<string, FileInfoModel> files = new ConcurrentDictionary<string, FileInfoModel>();

        private ConcurrentDictionary<string, FileInfoModel> remoteFiles =
            new ConcurrentDictionary<string, FileInfoModel>();

        private List<string> remoteGames = new List<string>();

        [ObservableProperty] 
        private ObservableCollection<GameInfoViewModel> _gameList = [];

        [ObservableProperty] private string _lastSyncTime;

        public GameOverviewViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
            GameList = new ObservableCollection<GameInfoViewModel>();
        }

        public void LoadGames(List<string> nodes)
        {
             GameList.Clear();

            foreach (var node in nodes)
            {
                var gameType = _mainViewModel.GameTypes.FirstOrDefault();
                var model = new GameInfoModel();
                model.Name = "<generated temp>";
                model.Id = node;
                model.CurrentTurn = 0;
                model.Players = new[] { "Michel" };
                var gameInfoViewModel = new GameInfoViewModel(model, _mainViewModel.PlayerName);

                if (gameType != null)
                {
                    gameInfoViewModel.GameTypeObject = gameType;
                    gameInfoViewModel.IconImage = new Bitmap(gameType.Icon);
                }

                // if (files.ContainsKey(info.FileName))
                // {
                //     gameInfoViewModel.File = files[info.FileName];
                // }

                GameList.Add(gameInfoViewModel);
            }
        }
    }
}