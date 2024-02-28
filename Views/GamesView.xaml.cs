using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using BetaManager.Classes;
using BetaManager.Models;

namespace BetaManager.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class GamesView : UserControl
    {
        private bool isUserScrolling = false;
        private Thread PicsThread;
        private string lastSearch;

        public GamesView()
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.GamesViewInstance = this;
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            new Functions().FadeIn(this, 250);
            if (Instances.GamesViewModel.Games == null || Instances.GamesViewModel.Games.Count < 0)
            {
                BlurFunc();
                await new Functions().FadeIn(this, 250);
                await new Functions().FadeIn(state, 300);
                var products = GetGameList(await Auth.Games());
                if (products?.Count > 0)
                {
                    Instances.GamesViewModel.Games = products;
                    ListViewProducts.ItemsSource = Instances.GamesViewModel.Games;
                    ListViewProducts.Items.Refresh();
                    PicsThread = new Thread(async () =>
                    {
                        new Functions().DownloadPicsAndScreenshots(products);
                    });
                    PicsThread.Start();
                }
                await new Functions().FadeOut(state, 300);
                BlurFunc(1);
            }
            else
            {
                if (state.Opacity != 0)
                {
                    await new Functions().FadeOut(state, 300);
                    BlurFunc(1);
                }
            }
            new Functions().FadeIn(this, 250);
        }

        private List<FitGirlGameModel> GetGameList(List<FitGirlGameModel> list, bool search = false)
        {
            if (list == null || list?.Count <= 0)
                return null;
            List<FitGirlGameModel> games = new List<FitGirlGameModel>();
            foreach (var game in list)
            {
                if (!game.Available)
                    continue;

                FitGirlGameModel tempGame = new FitGirlGameModel();
                tempGame.ID = game.ID;
                tempGame.Name = game.Name;
                tempGame.Version = game.Version;
                tempGame.Size = Functions.SizeSuffix(Convert.ToInt64(game.Size));
                tempGame.RepackSize = Functions.SizeSuffix(Convert.ToInt64(game.RepackSize));
                tempGame.RequiredSpace = Functions.SizeSuffix(
                    Convert.ToInt64(game.RepackSize) + Convert.ToInt64(game.Size)
                );
                tempGame.RequiredSpaceRaw =
                    Convert.ToInt64(game.RepackSize) + Convert.ToInt64(game.Size);
                tempGame.Description = game.Description;
                tempGame.RequirementsMinimum = game.RequirementsMinimum;
                tempGame.RequirementsRecommended = game.RequirementsRecommended;
                tempGame.Developer = game.Developer;
                tempGame.Genre = game.Genre;
                tempGame.Date = Functions
                    .UnixTimeStampToDateTime(Convert.ToInt64(game.Date))
                    .ToString("MM/dd/yyyy");
                tempGame.Available = game.Available;
                tempGame.TT = game.TT;
                tempGame.Data = game.Data;
                tempGame.Rating = game.Rating;
                tempGame.Link = game.Link;
                tempGame.AddedOn = game.AddedOn;
                tempGame.Screenshots = game.Screenshots;
                tempGame.Downloads = game.Downloads;
                tempGame.Platform = game.Platform;
                tempGame.FirstStarFont = (game.Rating / 1) >= 1 ? "Solid" : "Regular";
                tempGame.FirstStarColor = (game.Rating / 1) >= 1 ? "White" : "LightGray";
                tempGame.SecondStarFont = (game.Rating / 2) >= 1 ? "Solid" : "Regular";
                tempGame.SecondStarColor = (game.Rating / 2) >= 1 ? "White" : "LightGray";
                tempGame.ThirdStarFont = (game.Rating / 3) >= 1 ? "Solid" : "Regular";
                tempGame.ThirdStarColor = (game.Rating / 3) >= 1 ? "White" : "LightGray";
                tempGame.FourthStarFont = (game.Rating / 4) >= 1 ? "Solid" : "Regular";
                tempGame.FourthStarColor = (game.Rating / 4) >= 1 ? "White" : "LightGray";
                tempGame.FifthStarFont = (game.Rating / 5) >= 1 ? "Solid" : "Regular";
                tempGame.FifthStarColor = (game.Rating / 5) >= 1 ? "Gold" : "LightGray";
                tempGame.Credits = game.Credits;
                tempGame.URL = game.URL;
                tempGame.Picture = game.Picture;

                games.Add(tempGame);
            }

            return games;
        }

        //private GameModel CreateGame ( string ID, string Name, string Version, string Size, string Description, string Requirements, string Developer, string Genre, string Date, bool Available, string Data, BitmapSource Image )
        //{
        //    return new GameModel()
        //    {
        //        ID = ID,
        //        Name = Name,
        //        Description = Description,
        //        Requirements = Requirements,
        //        Developer = Developer,
        //        Date = Date,
        //        Picture = Image,
        //        Available = Available,
        //        Version = Version,
        //        Genre = Genre,
        //        Data = Data,
        //        Size = Size,
        //    };
        //}
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var game = (FitGirlGameModel)button.DataContext;
                var selectedGame = new FitGirlGameModel
                {
                    ID = game.ID,
                    Name = game.Name,
                    Version = game.Version,
                    Size = game.Size,
                    RepackSize = game.RepackSize,
                    RequiredSpace = game.RequiredSpace,
                    RequiredSpaceRaw = game.RequiredSpaceRaw,
                    Description = game.Description,
                    RequirementsMinimum = game.RequirementsMinimum,
                    RequirementsRecommended = game.RequirementsRecommended,
                    Developer = game.Developer,
                    Genre = game.Genre,
                    Date = game.Date,
                    Available = game.Available,
                    TT = game.TT,
                    Screenshots = game.Screenshots,
                    Data = game.Data,
                    Rating = game.Rating,
                    Downloads = game.Downloads,
                    Platform = game.Platform,
                    FirstStarFont = (game.Rating / 1) >= 1 ? "Solid" : "Regular",
                    FirstStarColor = (game.Rating / 1) >= 1 ? "White" : "LightGray",
                    SecondStarFont = (game.Rating / 2) >= 1 ? "Solid" : "Regular",
                    SecondStarColor = (game.Rating / 2) >= 1 ? "White" : "LightGray",
                    ThirdStarFont = (game.Rating / 3) >= 1 ? "Solid" : "Regular",
                    ThirdStarColor = (game.Rating / 3) >= 1 ? "White" : "LightGray",
                    FourthStarFont = (game.Rating / 4) >= 1 ? "Solid" : "Regular",
                    FourthStarColor = (game.Rating / 4) >= 1 ? "White" : "LightGray",
                    FifthStarFont = (game.Rating / 5) >= 1 ? "Solid" : "Regular",
                    FifthStarColor = (game.Rating / 5) >= 1 ? "Gold" : "LightGray",
                    Credits = game.Credits,
                    URL = game.URL,
                    Picture = game.Picture
                };
                PicsThread.Abort();
                Saved.SelectedGame = selectedGame;
                Instances.MainViewModel.ExecuteShowGameViewCommand();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var products = GetGameList( SearchTextBox.Text );
            //ListViewProducts.ItemsSource = products;
        }

        public async Task BlurFunc(int mode = 0)
        {
            BlurEffect blurEffect = new BlurEffect();
            DoubleAnimation blurAnimation;

            var tcs = new TaskCompletionSource<bool>();

            if (mode == 0)
            {
                TopBorder.Visibility = Visibility.Visible;
                blurEffect.Radius = 0.01;
                blurAnimation = new DoubleAnimation(5, TimeSpan.FromSeconds(0.25));
                blurAnimation.Completed += (s, e) =>
                {
                    tcs.SetResult(true);
                };
            }
            else
            {
                blurAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
                blurAnimation.Completed += (s, e) =>
                {
                    TopBorder.Visibility = Visibility.Hidden;
                    tcs.SetResult(true);
                };
            }

            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
            MainGrid.Effect = blurEffect;

            await tcs.Task;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (lastSearch == SearchTextBox.Text)
                return;
            if (PicsThread != null)
                PicsThread.Abort();
            await BlurFunc(0);
            if (FoundText.Opacity > 0)
            {
                await new Functions().FadeOut(FoundText, 150);
            }
            await new Functions().FadeIn(state, 300);
            var products = GetGameList(await Auth.SearchGames(SearchTextBox.Text), true);
            if (products?.Count > 0)
            {
                lastSearch = SearchTextBox.Text;
                Instances.GamesViewModel.Games = products;
                ListViewProducts.ItemsSource = Instances.GamesViewModel.Games;
                ListViewProducts.Items.Refresh();
                PicsThread = new Thread(async () =>
                {
                    new Functions().DownloadPicsAndScreenshots(products);
                });
                PicsThread.Start();
            }
            else
            {
                ListViewProducts.ItemsSource = null;
                ListViewProducts.ItemsSource = Instances.GamesViewModel.Games;
                ListViewProducts.Items.Refresh();
                new Functions().FadeIn(FoundText, 300);
            }
            await new Functions().FadeOut(state, 300);
            await BlurFunc(1);
        }

        private async void SearchTextBox_PreviewKeyDown(
            object sender,
            System.Windows.Input.KeyEventArgs e
        )
        {
            if (e.Key == Key.Enter && SearchTextBox.IsFocused)
            {
                if (lastSearch == SearchTextBox.Text)
                    return;
                if (PicsThread != null)
                    PicsThread.Abort();
                await BlurFunc(0);
                if (FoundText.Opacity > 0)
                {
                    await new Functions().FadeOut(FoundText, 150);
                }
                await new Functions().FadeIn(state, 300);
                var products = GetGameList(await Auth.SearchGames(SearchTextBox.Text), true);
                if (products?.Count > 0)
                {
                    lastSearch = SearchTextBox.Text;
                    Instances.GamesViewModel.Games = products;
                    ListViewProducts.ItemsSource = Instances.GamesViewModel.Games;
                    ListViewProducts.Items.Refresh();

                    PicsThread = new Thread(async () =>
                    {
                        new Functions().DownloadPicsAndScreenshots(products);
                    });
                    PicsThread.Start();
                }
                else
                {
                    Instances.GamesViewModel.Games = null;
                    ListViewProducts.ItemsSource = Instances.GamesViewModel.Games;
                    await new Functions().FadeIn(FoundText, 150);
                }
                await new Functions().FadeOut(state, 300);
                await BlurFunc(1);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (PicsThread != null)
                PicsThread.Abort();
            await BlurFunc(0);
            if (FoundText.Opacity > 0)
            {
                await new Functions().FadeOut(FoundText, 150);
            }
            await new Functions().FadeIn(state, 300);
            var products = GetGameList(await Auth.Games(), false);
            if (products?.Count > 0)
            {
                Instances.GamesViewModel.Games = products;
                ListViewProducts.ItemsSource = Instances.GamesViewModel.Games;
                ListViewProducts.Items.Refresh();

                PicsThread = new Thread(async () =>
                {
                    new Functions().DownloadPicsAndScreenshots(products);
                });
                PicsThread.Start();
            }
            else
            {
                new Functions().FadeIn(FoundText, 300);
            }
            await new Functions().FadeOut(state, 300);
            await BlurFunc(1);
        }

        private async void UpdateSorting(int a = 99, int b = 99)
        {
            lastSearch = null;
            if (PicsThread != null)
                PicsThread.Abort();
            await BlurFunc(0);
            if (FoundText.Opacity > 0)
            {
                await new Functions().FadeOut(FoundText, 150);
            }
            await new Functions().FadeIn(state, 300);
            if (a != 99)
                Saved.SelectedSort = a;
            if (b != 99)
                Saved.SelectedSortType = b;
            var products = GetGameList(
                await Auth.Games(Saved.SelectedSort, Saved.SelectedSortType)
            );
            if (products?.Count > 0)
            {
                Instances.GamesViewModel.Games = null;
                Instances.GamesViewModel.Games = products;
                ListViewProducts.ItemsSource = Instances.GamesViewModel.Games;
                ListViewProducts.Items.Refresh();

                PicsThread = new Thread(async () =>
                {
                    new Functions().DownloadPicsAndScreenshots(products);
                });
                PicsThread.Start();
            }
            else
            {
                new Functions().FadeIn(FoundText, 300);
            }
            await new Functions().FadeOut(state, 300);
            await BlurFunc(1);
        }

        private async void Sorting_Name_Click(object sender, RoutedEventArgs e)
        {
            UpdateSorting(0);
        }

        private async void Sorting_Size_Click(object sender, RoutedEventArgs e)
        {
            UpdateSorting(1);
        }

        private async void Sorting_Downloads_Click(object sender, RoutedEventArgs e)
        {
            UpdateSorting(2);
        }

        private async void Sorting_Date_Click(object sender, RoutedEventArgs e)
        {
            UpdateSorting(3);
        }

        private void AscButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateSorting(99, 0);
            Instances.GamesViewModel.ActiveColorDesc =
                Application.Current.Resources["plainTextColor"] as SolidColorBrush;
            Instances.GamesViewModel.ActiveColorAsc =
                Application.Current.Resources["ActiveColor"] as SolidColorBrush;
        }

        private void DescButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateSorting(99, 1);
            Instances.GamesViewModel.ActiveColorDesc =
                Application.Current.Resources["ActiveColor"] as SolidColorBrush;
            Instances.GamesViewModel.ActiveColorAsc =
                Application.Current.Resources["plainTextColor"] as SolidColorBrush;
        }

        private void QuickDownload_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Saved.SelectedGame = (FitGirlGameModel)((Button)sender).DataContext;
            Instances.MainViewInstance.BlurFunc(0);
            Instances.MainViewInstance.AdditionalView.Content = new DownloadView();
        }

        bool coded = true;

        private void SortingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!coded)
                UpdateSorting(SortingType.SelectedIndex);
            coded = false;
        }
    }
}
