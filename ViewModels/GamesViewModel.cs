using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using BetaManager.Classes;
using BetaManager.Models;

namespace BetaManager.ViewModels
{
    public class GamesViewModel : ViewModelBase
    {
        Brush activeColorAsc =
            Application.Current.Resources[
                Saved.SelectedSortType == 0 ? "ActiveColor" : "plainTextColor"
            ] as SolidColorBrush;
        Brush activeColorDesc =
            Application.Current.Resources[
                Saved.SelectedSortType == 1 ? "ActiveColor" : "plainTextColor"
            ] as SolidColorBrush;
        public Brush ActiveColorAsc
        {
            get { return activeColorAsc; }
            set
            {
                activeColorAsc = value;
                OnPropertyChanged(nameof(ActiveColorAsc));
            }
        }
        public Brush ActiveColorDesc
        {
            get { return activeColorDesc; }
            set
            {
                activeColorDesc = value;
                OnPropertyChanged(nameof(ActiveColorDesc));
            }
        }
        private static List<FitGirlGameModel> _games;
        public List<FitGirlGameModel> Games
        {
            get { return _games; }
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

        public GamesViewModel()
        {
            Instances.GamesViewModel = this;
        }
    }
}
