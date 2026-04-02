using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ATGSaveGameManager.ViewModel
{
    public partial class GameOverviewViewModel : PbemViewModelBase
    {
        private readonly List<string> remoteGames = new List<string>();

        [ObservableProperty]
        private ObservableCollection<GameInfoViewModel> _gameList = [];

        [ObservableProperty] private string _lastSyncTime;

        public GameOverviewViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
            GameList = new ObservableCollection<GameInfoViewModel>();
        }

        [RelayCommand]
        public async Task Delete(string id)
        {
            await _mainViewModel.DeleteNode(id);
        }

        [RelayCommand]
        public async Task UploadNewFile(string id)
        {

        }

        [RelayCommand]
        public async Task SubscribeToGame(string id)
        {
            await _mainViewModel.SubscribeToNode(id);
        }

        public void LoadGames(List<GameInfoModel> nodes)
        {
            GameList.Clear();

            foreach (var node in nodes)
            {
                var gameType = _mainViewModel.GameTypes.FirstOrDefault(x => x.Extension == node.GameType);
                if (gameType != null)
                {
                    var model = new GameInfoModel();
                    model.Name = node.Name;
                    model.Id = node.Id;
                    model.Players = node.Players;
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
}