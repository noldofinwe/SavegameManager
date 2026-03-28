using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using ATGSaveGameManager.Avalonia.ViewModels;

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
                return JsonSerializer.Deserialize<GameInfoModel>(json);
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
