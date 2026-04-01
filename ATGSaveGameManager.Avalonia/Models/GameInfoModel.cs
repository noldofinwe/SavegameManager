using ATGSaveGameManager.Avalonia.Models;

namespace ATGSaveGameManager
{
    public class GameInfoModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string FileName { get; set; }

        public string[] Players { get; set; }
        public string GameType { get; set; }

        public GameTurnModel GameTurnModel { get; set; }
    }
}
