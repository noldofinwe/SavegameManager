using GalaSoft.MvvmLight;
using System;
using System.Windows.Media.Imaging;

namespace ATGSaveGameManager.ViewModel
{
    public class GameTypeViewModel : ViewModelBase
    {
        private GameType _model;
        private BitmapImage _iconImage;

        public GameTypeViewModel(GameType model)
        {
            Model = model;
            IconImage = new BitmapImage(new Uri(Model.Icon, UriKind.RelativeOrAbsolute));
        }

        public GameType Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (value != _model)
                {
                    _model = value;
                    RaisePropertyChanged(nameof(Model));
                }
            }
        }

        public BitmapImage IconImage
        {
            get
            {
                return _iconImage;
            }
            set
            {
                if (value != _iconImage)
                {
                    _iconImage = value;
                    RaisePropertyChanged(nameof(IconImage));
                }
            }
        }

    }
}
