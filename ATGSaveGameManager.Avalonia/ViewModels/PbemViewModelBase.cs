using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ATGSaveGameManager.ViewModel
{
    public class PbemViewModelBase : ViewModelBase
    {
        protected readonly MainViewModel _mainViewModel;

        public PbemViewModelBase(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        protected GameInfoModel LoadJson(string file)
        {
            using (StreamReader r = new StreamReader(file))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<GameInfoModel>(json);
            }
        }

        protected string GetFileHash(string fileName)
        {
            string hash;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
        }


        public MainViewModel MainViewModel
        {
            get
            {
                return _mainViewModel;
            }
        }

    }
}
