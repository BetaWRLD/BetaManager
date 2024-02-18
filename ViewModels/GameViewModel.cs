using System.Threading;
using BetaManager.Models;

namespace BetaManager.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        public UserModel Users
        {
            get { return Saved.User; }
        }
        public bool NotGuest
        {
            get { return !Saved.User.Guest; }
        }

        public async void PrepareComments()
        {
            new Thread(async () => { });
            return;
        }

        public FitGirlGameModel CurrentGame
        {
            get { return Saved.SelectedGame; }
            set { OnPropertyChanged(nameof(Saved.SelectedGame)); }
        }
    }
}
