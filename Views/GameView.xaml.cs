using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BetaManager.Classes;
using BetaManager.Downloader;
using BetaManager.Models;
using BetaManager.Views.Windows;

namespace BetaManager.Views
{
    public partial class GameView : System.Windows.Controls.UserControl
    {
        FontAwesome.Sharp.IconFont tempSave;
        Brush tempColor;

        public GameView()
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.GameViewInstance = this;
            new Functions().FadeIn(this);
        }

        public async Task ChangeMarginWithAnimation(
            System.Windows.Controls.Button button,
            double left,
            double top,
            double right,
            double bottom,
            int duration
        )
        {
            ThicknessAnimation marginAnimation = new ThicknessAnimation
            {
                From = button.Margin,
                To = new Thickness(left, top, right, bottom),
                Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            };

            var tcs = new TaskCompletionSource<bool>();
            marginAnimation.Completed += (sender, e) =>
            {
                tcs.SetResult(true);
            };

            button.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);

            await tcs.Task;
        }

        public async Task UpdateText(
            System.Windows.Controls.Button button,
            string text,
            int duration
        )
        {
            var originalBrush = button.Foreground as SolidColorBrush;
            var originalColor = originalBrush.Color;

            var newBrush = new SolidColorBrush(originalColor);
            button.Foreground = newBrush;

            var colorAnimationOut = new ColorAnimation
            {
                From = originalColor,
                To = Colors.Transparent,
                Duration = TimeSpan.FromMilliseconds(duration)
            };

            var tcsOut = new TaskCompletionSource<bool>();
            colorAnimationOut.Completed += (sender, e) =>
            {
                tcsOut.SetResult(true);
            };

            newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimationOut);

            await tcsOut.Task;

            button.Content = text;

            var colorAnimationIn = new ColorAnimation
            {
                From = Colors.Transparent,
                To = originalColor,
                Duration = TimeSpan.FromMilliseconds(duration)
            };

            var tcsIn = new TaskCompletionSource<bool>();
            colorAnimationIn.Completed += (sender, e) =>
            {
                tcsIn.SetResult(true);
            };

            newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimationIn);

            await tcsIn.Task;
        }

        private void LeftArrowButton_Click(object sender, RoutedEventArgs e) { }

        private void RightArrowButton_Click(object sender, RoutedEventArgs e) { }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SizeText.Text =
                $"Game: {Saved.SelectedGame.Size}\nRepack: {Saved.SelectedGame.RepackSize}";
            if (
                DownloadManager.Instance.DownloadsList.Any(item => item.ID == Saved.SelectedGame.ID)
            )
            {
                UpdateText(DownloadButton, "Downloading...", 500);
                DownloadButton.IsEnabled = false;
            }
            if (Saved.User.Username == "guest")
            {
                FirstStar.IsEnabled = false;
                SecondStar.IsEnabled = false;
                ThirdStar.IsEnabled = false;
                FourthStar.IsEnabled = false;
                FifthStar.IsEnabled = false;
            }
            DescriptionText.Text = Functions.TruncateText(Saved.SelectedGame.Description, 300);
            CommentsSection.ItemsSource = await GetComments();
        }

        private DownloadView DownloadWindow;

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Instances.MainViewInstance.BlurFunc(0);
            Instances.MainViewInstance.AdditionalView.Content = new DownloadView();
        }

        private void IconBlock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) { }

        private void IconBlock_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) { }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Instances.MainViewModel.ShowGamesViewCommand.Execute(null);
        }

        private void MarkStar(FontAwesome.Sharp.IconBlock iconBlock)
        {
            iconBlock.IconFont = FontAwesome.Sharp.IconFont.Solid;
            iconBlock.Foreground = UIColors.ActiveColor;
        }

        private void IconBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FontAwesome.Sharp.IconBlock star = (FontAwesome.Sharp.IconBlock)sender;
            string starNum;
            switch (star.Name)
            {
                case "FirstStar":
                    Auth.AddRating(Saved.SelectedGame.ID, "1");
                    MarkStar(FirstStar);
                    break;
                case "SecondStar":
                    Auth.AddRating(Saved.SelectedGame.ID, "2");
                    MarkStar(FirstStar);
                    MarkStar(SecondStar);
                    break;
                case "ThirdStar":
                    Auth.AddRating(Saved.SelectedGame.ID, "3");
                    MarkStar(FirstStar);
                    MarkStar(SecondStar);
                    MarkStar(ThirdStar);
                    break;
                case "FourthStar":
                    Auth.AddRating(Saved.SelectedGame.ID, "4");
                    MarkStar(FirstStar);
                    MarkStar(SecondStar);
                    MarkStar(ThirdStar);
                    MarkStar(FourthStar);
                    break;
                case "FifthStar":
                    Auth.AddRating(Saved.SelectedGame.ID, "5");
                    MarkStar(FirstStar);
                    MarkStar(SecondStar);
                    MarkStar(ThirdStar);
                    MarkStar(FourthStar);
                    MarkStar(FifthStar);
                    break;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) { }

        private void CopyURL_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(Saved.SelectedGame.URL.First());
        }

        private async void ShowSS(int pos)
        {
            Instances.MainViewInstance.BlurFunc(0);
            Instances.MainViewInstance.AdditionalView.Content = new ImagesView(
                pos,
                Saved.SelectedGame.Screenshots
            );
        }

        private void FirstScreenshot_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            ShowSS(0);
        }

        private void SecondScreenshot_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            ShowSS(1);
        }

        private void ThirdScreenshot_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            ShowSS(2);
        }

        private void FourthScreenshot_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            ShowSS(3);
        }

        private void FifthScreenshot_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            ShowSS(4);
        }

        private void SixithScreenshot_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            ShowSS(5);
        }

        private void CreditText_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            Functions.StartProccess("https://fitgirl-repacks.site/");
        }

        private async Task<List<object>> GetComments()
        {
            List<CommentModel> SortedComments = (
                await CacheSystem.GetOrAdd(
                    $"gamedata.{Saved.SelectedGame.ID}",
                    () => Auth.GetGameData(Saved.SelectedGame.ID)
                )
            )
                ?.Comments
                ?.OrderByDescending(inner => Convert.ToInt64(inner.Date))
                .ToList();
            if (SortedComments == null)
                return null;
            dynamic test = new List<object>();

            for (int i = 0; i < SortedComments.Count; i++)
            {
                UserModel user = await CacheSystem.GetOrAdd(
                    $"user.{SortedComments[i].UserID}",
                    () => Auth.GetUser(SortedComments[i].UserID)
                );
                test.Add(
                    new
                    {
                        ID = SortedComments[i].ID,
                        UserID = user.ID,
                        Username = user.Username,
                        IsAuthor = user.ID == Saved.User.ID,
                        TrashVisibility = user.ID == Saved.User.ID
                            ? Visibility.Visible
                            : Visibility.Hidden,
                        Name = user.DisplayName,
                        ProfilePicture = user.ProfilePicturePath,
                        Date = Functions.TimeSince(SortedComments[i].Date),
                        Comment = SortedComments[i].Comment,
                        Likes = SortedComments[i].Likes?.Count ?? 0,
                        LikeButtonColor = SortedComments[i].Likes != null
                            ? (
                                SortedComments[i].Likes.FindIndex(s => s.UserID == Saved.User.ID)
                                > -1
                                    ? "#FFFF1493"
                                    : "#FF696969"
                            )
                            : "#FF696969",
                        Liked = SortedComments[i].Likes.Find(s => s.UserID == Saved.User.ID) != null
                            ? true
                            : false,
                    }
                );
            }
            return test;
        }

        private async void AddComment()
        {
            if (CommentText.Text.Length < 3)
            {
                Instances.MainViewInstance.Notify("Comment is too short");
                return;
            }
            dynamic dc = (List<object>)CommentsSection.ItemsSource ?? new List<object>();
            CommentText.Text = CommentText.Text.Trim();
            CommentText.IsEnabled = false;
            CommentModel commentModel = new CommentModel
            {
                Comment = CommentText.Text,
                Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Likes = new List<LikesModel>(),
            };
            if (await Auth.AddComment(commentModel, Saved.SelectedGame.ID))
            {
                GameDataModel Cached = await CacheSystem.Get<GameDataModel>(
                    $"gamedata.{Saved.SelectedGame.ID}"
                );
                if (Cached.Comments == null)
                    Cached.Comments = new List<CommentModel>();
                Cached.Comments.Insert(0, commentModel);
                Cached = await CacheSystem.UpdateOrAdd($"gamedata.{Saved.SelectedGame.ID}", Cached);
                dc.Insert(
                    0,
                    new
                    {
                        ID = commentModel.ID,
                        UserID = Saved.User.ID,
                        Username = Saved.User.Username,
                        Name = Saved.User.DisplayName,
                        ProfilePicture = Saved.User.ProfilePicturePath,
                        IsAuthor = true,
                        TrashVisibility = Visibility.Visible,
                        Date = Functions.TimeSince(commentModel.Date),
                        Comment = commentModel.Comment,
                        Likes = 0,
                        LikeButtonColor = "#FF696969",
                        Liked = false
                    }
                );
                CommentsSection.ItemsSource = null;
                CommentsSection.ItemsSource = dc;
                CommentText.Clear();
                CommentText.IsEnabled = true;
            }
            else
            {
                CommentText.IsEnabled = true;
            }
        }

        private async void CommentText_PreviewKeyDown(
            object sender,
            System.Windows.Input.KeyEventArgs e
        )
        {
            if (
                e.Key == Key.Enter
                && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift
            )
            {
                AddComment();
            }
        }

        private async void PostComment_Click(object sender, RoutedEventArgs e)
        {
            AddComment();
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            List<CommentModel> SortedComments = (
                await CacheSystem.GetOrAdd(
                    $"gamedata.{Saved.SelectedGame.ID}",
                    () => Auth.GetGameData(Saved.SelectedGame.ID)
                )
            )
                ?.Comments
                ?.OrderByDescending(inner => Convert.ToInt64(inner.Date))
                .ToList();
            List<dynamic> dc = (List<object>)CommentsSection.ItemsSource;
            System.Windows.Controls.Button block = (System.Windows.Controls.Button)sender;
            block.IsEnabled = false;
            dynamic Comment = block.DataContext;
            List<object> test = new List<object>();

            int like = await Auth.SendLike(Saved.SelectedGame.ID, Comment.ID);

            if (like == 1)
            {
                foreach (dynamic d in dc)
                {
                    if (d.ID == Comment.ID)
                        test.Add(
                            new
                            {
                                ID = Comment.ID,
                                UserID = Comment.UserID,
                                Username = Comment.Username,
                                Name = Comment.Name,
                                ProfilePicture = Comment.ProfilePicture,
                                IsAuthor = Comment.IsAuthor,
                                TrashVisibility = Comment.TrashVisibility,
                                Date = Comment.Date,
                                Comment = Comment.Comment,
                                Likes = (int)Comment.Likes + 1,
                                LikeButtonColor = "#FFFF1493",
                                Liked = true,
                            }
                        );
                    else
                        test.Add(
                            new
                            {
                                ID = d.ID,
                                UserID = d.UserID,
                                Username = d.Username,
                                Name = d.Name,
                                ProfilePicture = d.ProfilePicture,
                                IsAuthor = d.IsAuthor,
                                TrashVisibility = d.TrashVisibility,
                                Date = d.Date,
                                Comment = d.Comment,
                                Likes = d.Likes,
                                LikeButtonColor = d.LikeButtonColor,
                                Liked = d.Liked,
                            }
                        );
                }
                SortedComments[SortedComments.FindIndex(s => s.ID == Comment.ID)].Likes.Add(
                    new LikesModel { Date = 0, UserID = Saved.User.ID }
                );
                await CacheSystem.UpdateOrAdd($"gamedata.{Saved.SelectedGame.ID}", SortedComments);
            }
            else
            {
                foreach (dynamic d in dc)
                {
                    if (d.ID == Comment.ID)
                        test.Add(
                            new
                            {
                                ID = Comment.ID,
                                UserID = Comment.UserID,
                                Username = Comment.Username,
                                Name = Comment.Name,
                                ProfilePicture = Comment.ProfilePicture,
                                IsAuthor = Comment.IsAuthor,
                                TrashVisibility = Comment.TrashVisibility,
                                Date = Comment.Date,
                                Comment = Comment.Comment,
                                Likes = (int)Comment.Likes - 1,
                                LikeButtonColor = "#FF696969",
                                Liked = false,
                            }
                        );
                    else
                        test.Add(
                            new
                            {
                                ID = d.ID,
                                UserID = d.UserID,
                                Username = d.Username,
                                Name = d.Name,
                                ProfilePicture = d.ProfilePicture,
                                IsAuthor = d.IsAuthor,
                                TrashVisibility = d.TrashVisibility,
                                Date = d.Date,
                                Comment = d.Comment,
                                Likes = d.Likes,
                                LikeButtonColor = d.LikeButtonColor,
                                Liked = d.Liked,
                            }
                        );
                }
                SortedComments[SortedComments.FindIndex(s => s.ID == Comment.ID)].Likes.RemoveAt(
                    SortedComments[
                        SortedComments.FindIndex(s => s.ID == Comment.ID)
                    ].Likes.FindIndex(s => s.UserID == Saved.User.ID)
                );
                await CacheSystem.UpdateOrAdd($"gamedata.{Saved.SelectedGame.ID}", SortedComments);
            }
            block.IsEnabled = true;
            CommentsSection.ItemsSource = null;
            CommentsSection.ItemsSource = test;
        }

        private void ExpandDescription_Click(object sender, RoutedEventArgs e)
        {
            new Functions().UpdateText(DescriptionText, Saved.SelectedGame.Description);
            ExpandDescription.IsHitTestVisible = false;
            ExpandDescription.Visibility = Visibility.Hidden;
        }

        private async void DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            List<CommentModel> SortedComments = (
                await CacheSystem.GetOrAdd(
                    $"gamedata.{Saved.SelectedGame.ID}",
                    () => Auth.GetGameData(Saved.SelectedGame.ID)
                )
            )
                ?.Comments
                ?.OrderByDescending(inner => Convert.ToInt64(inner.Date))
                .ToList();
            System.Windows.Controls.Button block = (System.Windows.Controls.Button)sender;
            dynamic Comment = block.DataContext;

            int result = await Auth.DeleteComment(Saved.SelectedGame.ID, Comment.ID);
            if (result == 1)
            {
                SortedComments.RemoveAt(SortedComments.FindIndex(s => s.ID == Comment.ID));
                await CacheSystem.UpdateOrAdd($"gamedata.{Saved.SelectedGame.ID}", SortedComments);
                var comments = await GetComments();
                CommentsSection.ItemsSource = null;
                CommentsSection.ItemsSource = comments;
            }
        }
    }
}
