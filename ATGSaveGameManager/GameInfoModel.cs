using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATGSaveGameManager
{
    public class GameInfoModel
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public string[] Players { get; set; }
        public string LastPlayer { get; set; }

        [JsonIgnore]
        public string NextPlayer
        {
            get
            {
                string next = "";
                var list = Players.ToList();

                var index = list.IndexOf(LastPlayer);

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
        public FileInfoModel File { get; set; }

    }
}
