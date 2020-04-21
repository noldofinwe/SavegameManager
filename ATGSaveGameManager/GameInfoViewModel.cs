using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace ATGSaveGameManager
{
    public class GameInfoViewModel : ViewModelBase
    {
        private string _player;

        public GameInfoViewModel(GameInfoModel model, string player)
        {
            Model = model;
            _player = player;
        }

        private GameInfoModel _model;


        public GameInfoModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (value != _model)
                {
                    _model = value;
                    RaisePropertyChanged(nameof(Model));
                }
            }
        }

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

        private BitmapImage _iconImage;

        [JsonIgnore]
        public BitmapImage IconImage
        {
            get
            {
                return _iconImage;
            }
            set
            {
                if (value != _iconImage)
                {
                    _iconImage = value;
                    RaisePropertyChanged(nameof(IconImage));
                }
            }
        }

        [JsonIgnore]
        public GameType GameTypeObject
        {
            get
            {
                return _gameTypeObject;
            }
            set
            {
                if (value != _gameTypeObject)
                {
                    _gameTypeObject = value;
                    RaisePropertyChanged(nameof(GameTypeObject));
                }

            }
        }

        private GameType _gameTypeObject;

        public bool IsPlayer => Model.Players.Contains(_player);
        public bool IsYourTurn => NextPlayer.Equals(_player);

        [JsonIgnore]
        public FileInfoModel File { get; set; }

    }
}
