using System.Windows.Controls;
using BetaManager.Classes;

namespace BetaManager.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.HomeViewInstance = this;
        }

        private void HomeViewU_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            new Functions().FadeIn(this);
        }
    }
}
