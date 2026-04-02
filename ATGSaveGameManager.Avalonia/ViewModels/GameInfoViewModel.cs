using ATGSaveGameManager.Avalonia.ViewModels;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

namespace ATGSaveGameManager.ViewModel
{
    public partial class GameInfoViewModel : ViewModelBase
    {

        private readonly string _player;

        public GameInfoViewModel(GameInfoModel model, string player)
        {
            Model = model;
            _player = player;
        }

        [ObservableProperty]
        private GameInfoModel _model;




        public string LastTurnTimeString => Model.GameTurnModel == null ? "Unknown" : Model.GameTurnModel.LastTurnTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");

        public string NextPlayer
        {
            get
            {
                if (Model.GameTurnModel == null)
                {
                    return "Unknown";
                }
                var list = Model.Players.ToList();

                var index = list.IndexOf(Model.GameTurnModel.LastPlayer);

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
