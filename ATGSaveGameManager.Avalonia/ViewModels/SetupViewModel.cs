using ATGSaveGameManager.Configuration;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using ATGSaveGameManager.Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;

namespace ATGSaveGameManager.ViewModel
{
    public partial class SetupViewModel : PbemViewModelBase
    {
        [ObservableProperty] private string _selectedPlayerName;
        [ObservableProperty] private string _selectedConnection;
        [ObservableProperty] private string _newGameSaveGame;
        [ObservableProperty] private string _newGameName;
        [ObservableProperty] private string _newGameExtension;
        [ObservableProperty] private string _newGameSaveFolder;
        [ObservableProperty] private string _newGameIcon;
        [ObservableProperty] private bool _adding;
        [ObservableProperty] private ObservableCollection<GameTypeViewModel> _gameTypes = [];
        [ObservableProperty] private GameTypeViewModel _selectedGameTypeViewModel;

        
        
        public SetupViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
        }

        [RelayCommand]
        public async Task Add()
        {
            if (string.IsNullOrWhiteSpace(NewGameExtension))
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Error", "Game extension is empty.")
                    .ShowAsync();
                return;
            }

            if (string.IsNullOrWhiteSpace(NewGameSaveGame))
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Error", "Game Save game folder is empty.")
                    .ShowAsync();
                return;
            }

            if (string.IsNullOrWhiteSpace(NewGameName))
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Error", "Game Name is empty.")
                    .ShowAsync();
                return;
            }

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
        public void Cancel()
        {
            NewGameExtension = null;
            NewGameSaveGame = null;
            NewGameIcon = null;
            NewGameName = null;
            Adding = false;
        }

        [RelayCommand]
        public void AddNew()
        {
            Adding = true;
        }

        [RelayCommand]
        public void Delete()
        {
            GameTypes.Remove(SelectedGameTypeViewModel);
            SelectedGameTypeViewModel = null;
        }

        [RelayCommand]
        public void Update()
        {
            SelectedGameTypeViewModel = null;
        }
        
        public void SetCurrentSettings(AppSettings appSettings, string password)
        {
            GameTypes.Clear();
            Cancel();
            SelectedPlayerName = appSettings.Player;
            SelectedConnection = password;

            if (appSettings?.GamesTypes != null)
            {
                foreach (var type in appSettings.GamesTypes)
                {
                    var viewModel = new GameTypeViewModel(type);
                    GameTypes.Add(viewModel);
                }
                OnPropertyChanged(nameof(GameTypes));
            }
        }

        [RelayCommand]
        public async Task SelectDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select the game Savegame folder"
            };

            // Show dialog — requires a parent window
            var result = await dialog.ShowAsync(App.MainWindow);

            if (string.IsNullOrWhiteSpace(result))
            {
                // You can use MsBox.Avalonia or your own dialog service
                var box = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Error", "No folder selected");
                await box.ShowAsync();
                return;
            }

            // Apply result
            if (UpdateVisible)
            {
                SelectedGameTypeViewModel.Model.Savegames = result;
            }
            else
            {
                NewGameSaveGame = result;
            }
        }

        [RelayCommand]
        public async Task SelectIcon()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select the game Icon",
                AllowMultiple = false
            };

            // Optional: restrict to image files
            dialog.Filters.Add(new FileDialogFilter
            {
                Name = "Image files",
                Extensions = { "png", "jpg", "jpeg", "bmp", "ico" }
            });

            dialog.Filters.Add(new FileDialogFilter
            {
                Name = "All files",
                Extensions = { "*" }
            });

            // Show dialog — requires a parent window
            var result = await dialog.ShowAsync(App.MainWindow);

            if (result == null || result.Length == 0)
            {
                var box = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Error", "No icon selected");
                await box.ShowAsync();
                return;
            }

            var file = result[0];

            if (UpdateVisible)
            {
                SelectedGameTypeViewModel.Model.Icon = file;
            }
            else
            {
                NewGameIcon = file;
            }
        }


        [RelayCommand]
        public async Task SaveSettings()
        {
            if (string.IsNullOrWhiteSpace(SelectedPlayerName))
            {
                await MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Error", "Player name is empty.")
                    .ShowAsync();
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedConnection))
            {
                await MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard(
                        "Error",
                        "No Azure blob storage connection has been selected. Check the readme for instructions.")
                    .ShowAsync();
                return;
            }

            if (GameTypes.Count == 0)
            {
                await MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Error", "No games have been selected.")
                    .ShowAsync();
                return;
            }

            await _mainViewModel.UpdateAppsettings(
                SelectedPlayerName,
                SelectedConnection,
                GameTypes.Select(p => p.Model));
        }


        partial void OnSelectedGameTypeViewModelChanged(GameTypeViewModel value)
        {
            OnPropertyChanged(nameof(UpdateVisible));
            OnPropertyChanged(nameof(CanAddVisible));
            OnPropertyChanged(nameof(AddVisible));
        }
        partial void OnAddingChanged(bool value)
        {
            OnPropertyChanged(nameof(CanAddVisible));


            OnPropertyChanged(nameof(AddVisible));
        }


        public bool UpdateVisible => SelectedGameTypeViewModel != null;
        public bool CanAddVisible => SelectedGameTypeViewModel == null && !Adding;
        public bool AddVisible => SelectedGameTypeViewModel == null && Adding;
    }
}