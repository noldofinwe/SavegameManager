using ATGSaveGameManager.Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ATGSaveGameManager.ViewModel
{
    public partial class NewGameViewModel : PbemViewModelBase
    {
        [ObservableProperty] private string _newGameName;
        [ObservableProperty] private string _newGameFileName;
        [ObservableProperty] private string _newGameAddPlayer;
        [ObservableProperty] private GameTypeViewModel _newGameGameType;

        public NewGameViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
            NewGamePlayers = new ObservableCollection<string>();
        }

        [RelayCommand]
        public async Task Save()
        {
            var gameinfo = new GameInfoModel
            {
                FileName = Path.GetFileName(NewGameFileName),
                GameType = NewGameGameType.Model.Extension,
                Name = NewGameName,
                Id = Guid.NewGuid().ToString("N"),
                Players = NewGamePlayers.ToArray()
            };
            await _mainViewModel.AddedNewGame(gameinfo);
        }

        [RelayCommand]
        public void Back()
        {
            _mainViewModel.IsCreatingNewGame = false;
        }

        [RelayCommand]
        public void AddPlayer()
        {
            if (!string.IsNullOrWhiteSpace(NewGameAddPlayer))
            {
                NewGamePlayers.Add(NewGameAddPlayer);
                NewGameAddPlayer = "";
            }
        }

        [RelayCommand]
        public async Task SelectFile()
        {
            var dialog = new OpenFileDialog();

            if (NewGameGameType != null)
            {
                dialog.Directory = NewGameGameType.Model.Savegames;
                dialog.Filters.Add(new FileDialogFilter
                {
                    Name = $"{NewGameGameType.Model.Extension} files",
                    Extensions = { NewGameGameType.Model.Extension }
                });
            }
            else
            {
                dialog.Directory = _mainViewModel.SetupViewModel.GameTypes.First().Model.Savegames;
                dialog.Filters.Add(new FileDialogFilter
                {
                    Name = "All files",
                    Extensions = { "*" }
                });
            }

            // You must pass a Window as the parent
            var result = await dialog.ShowAsync(App.MainWindow);

            if (result != null && result.Length > 0)
            {
                NewGameFileName = result[0];
            }
        }


        [ObservableProperty]
        private ObservableCollection<string> _newGamePlayers;
    }
}