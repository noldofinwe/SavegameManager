using ATGSaveGameManager.Configuration;

using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATGSaveGameManager.ViewModel
{
    public partial class SetupViewModel : PbemViewModelBase
    {
        [ObservableProperty]
        private string _selectedPlayerName;
        [ObservableProperty]
        private string _selectedConnection;
        [ObservableProperty]
        private string _newGameSaveGame;
        [ObservableProperty]
        private string _newGameExtension;
        [ObservableProperty]
        private string _newGameSaveFolder;
        [ObservableProperty]
        private string _newGameIcon;
        [ObservableProperty]
        private bool _adding;
        [ObservableProperty]
        private ObservableCollection<GameTypeViewModel> _gameTypes;
        [ObservableProperty]
        private GameTypeViewModel _selectedGameTypeViewModel;

        public SetupViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
          
        }

        [RelayCommand]
        private void Add()
        {
            // if(string.IsNullOrWhiteSpace(NewGameExtension))
            // {
            //     MessageBox.Show("Game extension is empty.");
            //     return;
            // }
            // if (string.IsNullOrWhiteSpace(NewGameSaveGame))
            // {
            //     MessageBox.Show("Game Save game folder is empty.");
            //     return;
            // }
            // if (string.IsNullOrWhiteSpace(NewGameName))
            // {
            //     MessageBox.Show("Game Name is empty.");
            //     return;
            // }
    
            var gameType = new GameType
            {
                Name = NewGameName,
                Extension = NewGameExtension,
                Icon = NewGameIcon,
                Savegames = NewGameSaveGame
            };

            GameTypes.Add(new GameTypeViewModel(gameType));
            Adding = false;
        }

        [RelayCommand]
        private void CancelAdd()
        {
            NewGameExtension = null;
            NewGameSaveGame = null;
            NewGameIcon = null;
            NewGameName = null;
            Adding = false;
        }

        [RelayCommand]
        private void AddNew()
        {
            Adding = true;
        }

        [RelayCommand]
        private void Delete()
        {
            GameTypes.Remove(SelectedGameTypeViewModel);
            SelectedGameTypeViewModel = null;
        }

        private void Update()
        {
            SelectedGameTypeViewModel = null;
        }

        [RelayCommand]
        public void SetCurrentSettings(AppSettings appSettings)
        {
            GameTypes.Clear();
            CancelAdd();
            SelectedPlayerName = appSettings.Player;
            SelectedConnection = appSettings.ConnectionStrings.BlobStorageKey;
            SelectedConnection = appSettings.ConnectionStrings?.BlobStorageKey;

            if (appSettings.GamesTypes != null)
            {
                foreach (var type in appSettings.GamesTypes)
                {
                    var viewModel = new GameTypeViewModel(type);
                    GameTypes.Add(viewModel);
                }
            }
        }

        [RelayCommand]
        private void SelectDirectory()
        {
            var openFolder = new CommonOpenFileDialog();
            openFolder.AllowNonFileSystemItems = true;
            openFolder.Multiselect = false;
            openFolder.IsFolderPicker = true;
            openFolder.Title = "Select the game Savegame folder";

            if (openFolder.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageBox.Show("No Folder selected");
                return;
            }

            // get all the directories in selected dirctory
            if (UpdateVisible)
            {
                SelectedGameTypeViewModel.Model.Savegames = openFolder.FileName;
            }
            else
            {
                NewGameSaveGame = openFolder.FileName;
            }
        }

        [RelayCommand]
        private void SelectIcon()
        {
            var openFolder = new CommonOpenFileDialog();
            openFolder.AllowNonFileSystemItems = true;
            openFolder.Multiselect = false;
            openFolder.IsFolderPicker = false;
            openFolder.Title = "Select the game Icon";

            if (openFolder.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageBox.Show("No Icon selected");
                return;
            }

            if (UpdateVisible)
            {
                SelectedGameTypeViewModel.Model.Icon = openFolder.FileName;
            }
            else
            {
                NewGameIcon = openFolder.FileName;
            }
        }

        [RelayCommand]
        public void SaveSettings()
        {
            if (string.IsNullOrWhiteSpace(SelectedPlayerName))
            {
                MessageBox.Show("Player name is empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedConnection))
            {
                MessageBox.Show("No Azure blob storage connection has been selected, check readme on how to create Azure blob storage and how to find connection string.");
                return;
            }
            if (GameTypes.Count == 0)
            {
                MessageBox.Show("No games have been selected.");
                return;
            }

            _mainViewModel.UpdateAppsettings(SelectedPlayerName, SelectedConnection, GameTypes.Select(p => p.Model));
        }


        // public GameTypeViewModel SelectedGameTypeViewModel
        // {
        //     get
        //     {
        //         return _selectedGameTypeViewModel;
        //     }
        //     set
        //     {
        //         if (_selectedGameTypeViewModel != value)
        //         {
        //             _selectedGameTypeViewModel = value;
        //             RaisePropertyChanged(nameof(SelectedGameTypeViewModel));
        //             RaisePropertyChanged(nameof(UpdateVisible));
        //             RaisePropertyChanged(nameof(CanAddVisible));
        //             RaisePropertyChanged(nameof(AddVisible));
        //         }
        //     }
        // }


        //
        //
        // public bool Adding
        // {
        //     get
        //     {
        //         return _adding;
        //     }
        //     set
        //     {
        //         if (_adding != value)
        //         {
        //             _adding = value;
        //             RaisePropertyChanged(nameof(Adding));
        //             RaisePropertyChanged(nameof(CanAddVisible));
        //             RaisePropertyChanged(nameof(AddVisible));
        //         }
        //     }
        // }

        public bool UpdateVisible => SelectedGameTypeViewModel != null;
        public bool CanAddVisible => SelectedGameTypeViewModel == null && !Adding;
        public bool AddVisible => SelectedGameTypeViewModel == null && Adding;

    }
}
