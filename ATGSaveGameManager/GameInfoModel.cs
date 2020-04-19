using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media.Imaging;

namespace ATGSaveGameManager
{
    public class GameInfoModel : ViewModelBase
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public string[] Players { get; set; }
        public string LastPlayer { get; set; }

        public int? CurrentTurn { get; set; }
        public DateTime LastTurnTime { get; set; }

        [JsonIgnore]
        public string LastTurnTimeString => LastTurnTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");

        public string GameType { get; set; }

        [JsonIgnore]
        public string NextPlayer
        {
            get
            {
                var list = Players.ToList();

                var index = list.IndexOf(LastPlayer);

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
        public FileInfoModel File { get; set; }
    }
}
