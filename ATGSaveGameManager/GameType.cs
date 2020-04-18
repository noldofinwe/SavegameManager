using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ATGSaveGameManager
{
    public class GameType
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("savegames")]
        public string Savegames { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }


    }
}
