using BetaManager.Downloader;
using System;
using System.Windows;
using System.Windows.Threading;

namespace BetaManager
{
    public class ManagerTimers
    {
        public void BlurStuff ( DownloadClient download )
        {
            double currentBlurRadius = 0;
            double targetBlurRadius = 10;
            double blurIncrement = 1;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds( 1 );
            timer.Tick += ( sender, e ) =>
            {
                currentBlurRadius += blurIncrement;
                download.BlurAmount = currentBlurRadius.ToString();

                if ( currentBlurRadius >= targetBlurRadius )
                {
                    timer.Stop();
                    download.CancelConfirmation = Visibility.Visible;
                    download.UpdateDownloadDisplay();
                }
            };

            timer.Start();
        }
        public void UnBlurStuff ( DownloadClient download )
        {
            double currentBlurRadius = 10;
            double targetBlurRadius = 0;
            double blurIncrement = 1;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds( 1 );
            timer.Tick += ( x, y ) =>
            {
                currentBlurRadius -= blurIncrement;
                download.BlurAmount = currentBlurRadius.ToString();

                if ( currentBlurRadius <= targetBlurRadius )
                {
                    timer.Stop();
                    download.CancelConfirmation = Visibility.Hidden;
                    download.UpdateDownloadDisplay();
                }
            };

            timer.Start();
        }
    }
}