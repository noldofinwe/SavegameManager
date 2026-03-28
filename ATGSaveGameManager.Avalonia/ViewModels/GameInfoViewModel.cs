using System.Linq;
using ATGSaveGameManager.Avalonia.ViewModels;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ATGSaveGameManager.ViewModel
{
    public partial class GameInfoViewModel : ViewModelBase
    {
        private string _player;

        public GameInfoViewModel(GameInfoModel model, string player)
        {
            Model = model;
            _player = player;
        }

        [ObservableProperty]
        private GameInfoModel _model;


      

        public string LastTurnTimeString => Model.LastTurnTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");

        public string NextPlayer
        {
            get
            {
                var list = Model.Players.ToList();

                var index = list.IndexOf(Model.LastPlayer);

                string next;
                if (index + 1 < list.Count)
                    next = list[index + 1];
                else
                {
                    next = list[0];
                }

                return next;
            }
        }

        [ObservableProperty]
        private Bitmap _iconImage;
        [ObservableProperty]
        private GameType _gameTypeObject;

        public bool IsPlayer => Model.Players.Contains(_player);
        public bool IsYourTurn => NextPlayer.Equals(_player);

        public FileInfoModel File { get; set; }

    }
}
