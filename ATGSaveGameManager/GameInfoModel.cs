using System;
using System.Collections.Generic;
using System.Text;

namespace ATGSaveGameManager
{
    public class GameInfoModel
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public string[] Players { get; set; }
        public string PlayerName { get; set; }
    }
}
