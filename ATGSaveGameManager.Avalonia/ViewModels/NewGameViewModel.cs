using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ATGSaveGameManager.ViewModel
{
    public class NewGameViewModel : PbemViewModelBase
    {
        public RelayCommand BackCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand SelectFileCommand { get; private set; }
        public RelayCommand AddPlayerCommand { get; private set; }

        private string _newGameName;
        private string _newGameFileName;
        private string _newGameAddPlayer;
        private GameType _newGameGameType;

        public NewGameViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
            BackCommand = new RelayCommand(Back, null);
            SaveCommand = new RelayCommand(Save, null);
            SelectFileCommand = new RelayCommand(SelectFile, null);
            AddPlayerCommand = new RelayCommand(AddPlayer, null);
            NewGamePlayers = new ObservableCollection<string>();
        }


        private void Save()
        {
            var gameinfo = new GameInfoModel
            {
                FileName = Path.GetFileName(NewGameFileName),
                GameType = NewGameGameType.Extension,
                Name = NewGameName,
                Players = NewGamePlayers.ToArray()
            };
            var jsonObject = JsonConvert.SerializeObject(gameinfo);

            File.WriteAllText($"{_mainViewModel.DataDirectory}\\{NewGameName}.json", jsonObject);
            _mainViewModel.AddedNewGame();

        }

        private void Back()
        {
            _mainViewModel.IsCreatingNewGame = false;
        }


        private void AddPlayer()
        {
            if (!string.IsNullOrWhiteSpace(NewGameAddPlayer))
            {
                NewGamePlayers.Add(NewGameAddPlayer);
                NewGameAddPlayer = "";
            }
        }

        private void SelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
 
            if (NewGameGameType != null)
            {
                openFileDialog.InitialDirectory = NewGameGameType.Savegames;
                openFileDialog.Filter = $"Save files (*.{NewGameGameType.Extension})|*.{NewGameGameType.Extension}|All files (*.*)|*.*";
            }
            else
            {
                openFileDialog.InitialDirectory = _mainViewModel.GameTypes.First().Savegames;
                openFileDialog.Filter = $"All files (*.*)|*.*";
            }
            if (openFileDialog.ShowDialog() == true)
            {
                NewGameFileName = openFileDialog.FileName;
            }
        }


        public GameType NewGameGameType
        {
            get
            {
                return _newGameGameType;
            }
            set
            {
                if (_newGameGameType != value)
                {
                    _newGameGameType = value;
                    RaisePropertyChanged(nameof(NewGameGameType));
                }
            }
        }



        public string NewGameName
        {
            get
            {
                return _newGameName;
            }
            set
            {
                if (_newGameName != value)
                {
                    _newGameName = value;
                    RaisePropertyChanged(nameof(NewGameName));
                }
            }
        }

        public string NewGameAddPlayer
        {
            get
            {
                return _newGameAddPlayer;
            }
            set
            {
                if (_newGameAddPlayer != value)
                {
                    _newGameAddPlayer = value;
                    RaisePropertyChanged(nameof(NewGameAddPlayer));
                }
            }
        }

        public string NewGameFileName
        {
            get
            {
                return _newGameFileName;
            }
            set
            {
                if (_newGameFileName != value)
                {
                    _newGameFileName = value;
                    RaisePropertyChanged(nameof(NewGameFileName));
                }
            }
        }

        private ObservableCollection<string> _newGamePlayers;
        public ObservableCollection<string> NewGamePlayers
        {
            get
            {
                if (_newGamePlayers == null)
                {
                    _newGamePlayers = new ObservableCollection<string>();
                }
                return _newGamePlayers;
            }
            set
            {
                _newGamePlayers = value;
                RaisePropertyChanged("NewGamePlayers");
            }
        }


    }
}
