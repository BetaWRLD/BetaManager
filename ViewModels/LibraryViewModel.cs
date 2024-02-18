using System.Collections.Generic;
using BetaManager.Classes;
using BetaManager.Models;

namespace BetaManager.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private List<LibraryGameModel> _games;
        public List<LibraryGameModel> Games
        {
            get
            {
                _games = Saved.LibraryGames;
                _games.ForEach(x =>
                {
                    x.InstallDateString = Functions
                        .UnixTimeStampToDateTime(x.InstallDate)
                        .ToString("MM/dd/yyyy");
                    x.LastPlayDateString = Functions.TimeSince(x.LastPlayDate);
                });
                return _games;
            }
            set
            {
                _games = value;
                OnPropertyChanged(nameof(Games));
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Games));
        }

        public LibraryViewModel()
        {
            Instances.LibraryViewModel = this;
        }
    }
}
