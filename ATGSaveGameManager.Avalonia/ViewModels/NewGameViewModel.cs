using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ATGSaveGameManager.Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATGSaveGameManager.ViewModel
{
    public partial class NewGameViewModel : PbemViewModelBase
    {
        [ObservableProperty] private string _newGameName;
        [ObservableProperty] private string _newGameFileName;
        [ObservableProperty] private string _newGameAddPlayer;
        [ObservableProperty] private GameType _newGameGameType;

        public NewGameViewModel(MainViewModel mainViewModel) : base(mainViewModel)
        {
            NewGamePlayers = new ObservableCollection<string>();
        }

        [RelayCommand]
        private void Save()
        {
            var gameinfo = new GameInfoModel
            {
                FileName = Path.GetFileName(NewGameFileName),
                GameType = NewGameGameType.Extension,
                Name = NewGameName,
                Players = NewGamePlayers.ToArray()
            };
            var jsonObject = JsonSerializer.Serialize(gameinfo);

            File.WriteAllText($"{_mainViewModel.DataDirectory}\\{NewGameName}.json", jsonObject);
            _mainViewModel.AddedNewGame();
        }

        [RelayCommand]
        private void Back()
        {
            _mainViewModel.IsCreatingNewGame = false;
        }

        [RelayCommand]
        private void AddPlayer()
        {
            if (!string.IsNullOrWhiteSpace(NewGameAddPlayer))
            {
                NewGamePlayers.Add(NewGameAddPlayer);
                NewGameAddPlayer = "";
            }
        }

        [RelayCommand]
        private async Task SelectFile()
        {
            var dialog = new OpenFileDialog();

            if (NewGameGameType != null)
            {
                dialog.Directory = NewGameGameType.Savegames;
                dialog.Filters.Add(new FileDialogFilter
                {
                    Name = $"{NewGameGameType.Extension} files",
                    Extensions = { NewGameGameType.Extension }
                });
            }
            else
            {
                dialog.Directory = _mainViewModel.GameTypes.First().Savegames;
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