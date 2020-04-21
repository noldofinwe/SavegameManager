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


        public string GameType { get; set; }

    }
}
