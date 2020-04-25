using System;
using System.Collections.Generic;
using System.Text;

namespace ATGSaveGameManager.Configuration
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public GameType[] GamesTypes { get; set; }
        public string Player { get; set; }

    }
}
