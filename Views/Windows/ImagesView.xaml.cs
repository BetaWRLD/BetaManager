using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BetaManager.Classes;

namespace BetaManager.Views.Windows
{
    /// <summary>
    /// Interaction logic for ImagesView.xaml
    /// </summary>
    public partial class ImagesView : UserControl
    {
        private List<string> images;
        private int index;

        public ImagesView(int indexToShow, List<string> imageList)
        {
            this.Opacity = 0;
            images = imageList;
            index = indexToShow;
            InitializeComponent();
        }

        private async void MainViewInstance_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseOutside(e, this) && this.Opacity == 1)
            {
                await new Functions().FadeOut(this);
                this.IsHitTestVisible = false;
                Instances.MainViewInstance.BlurFunc(1);
            }
        }

        private bool IsMouseOutside(MouseButtonEventArgs e, UIElement i)
        {
            Point relativePosition = e.GetPosition(i);

            return relativePosition.X < 0
                || relativePosition.Y < 0
                || relativePosition.X >= this.ActualWidth
                || relativePosition.Y >= this.ActualHeight;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Instances.MainViewInstance.PreviewMouseDown += MainViewInstance_PreviewMouseDown;
            CurrentImage.ImageSource = new BitmapImage(new System.Uri(images[index]));
            new Functions().FadeIn(this);
        }

        private void MoveLeft_Click(object sender, RoutedEventArgs e) { }

        private void MoveRight_Click(object sender, RoutedEventArgs e) { }
    }
}
