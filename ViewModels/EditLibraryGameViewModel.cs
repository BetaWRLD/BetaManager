using BetaManager.Classes;
using BetaManager.Models;

namespace BetaManager.ViewModels
{
    public class EditLibraryGameViewModel : ViewModelBase
    {
        public LibraryGameModel _game;
        public LibraryGameModel Game
        {
            get { return _game; }
            set
            {
                _game = value;
                OnPropertyChanged(nameof(Game));
            }
        }

        public EditLibraryGameViewModel()
        {
            Instances.EditLibraryGameViewModel = this;
        }
    }
}
