using System;

namespace ATGSaveGameManager.Avalonia.Models
{
    public class GameTurnModel
    {
        public DateTime LastTurnTime { get; set; }
        public string LastPlayer { get; set; }
        public string Url { get; set; }
    }
}
