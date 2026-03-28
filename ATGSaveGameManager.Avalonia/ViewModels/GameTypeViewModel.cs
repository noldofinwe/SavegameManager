using System;
using ATGSaveGameManager.Avalonia.ViewModels;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ATGSaveGameManager.ViewModel
{
    public partial class GameTypeViewModel : ViewModelBase
    {
        [ObservableProperty]
        private GameType _model;
        
        [ObservableProperty]
        private Bitmap _iconImage;

        public GameTypeViewModel(GameType model)
        {
            Model = model;
            IconImage = new Bitmap(Model.Icon);
        }
    }
}
