

using System;

namespace ATGSaveGameManager
{
    public class GameInfoModel
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
