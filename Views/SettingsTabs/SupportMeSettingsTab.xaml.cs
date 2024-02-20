using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace BetaManager.Views.SettingsTabs
{
    /// <summary>
    /// Interaction logic for GeneralSettingsTab.xaml
    /// </summary>
    public partial class SupportMeSettingsTab : UserControl
    {
        public SupportMeSettingsTab()
        {
            this.Opacity = 0;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            new Functions().FadeIn(this);
        }

        private async void XMRAddress_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            Clipboard.SetText(
                "48EKyPa8HdrK6jTvM7Z9GEBrGyDUjrMwmA69DhgqiPegNzR8LyKXmxCgtfywEfwkvFKvLpF1WsBNPjUWhZavQnZ1UJ5kxnk"
            );
            await new Functions().UpdateText(XMRAddress, "Copied!", 200);
            await Functions.SleepAsync(250);
            await new Functions().UpdateText(
                XMRAddress,
                "48EKyPa8HdrK6jTvM7Z9GEBrGyDUjrMwmA69DhgqiPegNzR8LyKXmxCgtfywEfwkvFKvLpF1WsBNPjUWhZavQnZ1UJ5kxnk",
                200
            );
        }

        private async void ETHAddress_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            Clipboard.SetText("0xe40E8074E37fBF846D07c3cB7E865898156e172a");
            await new Functions().UpdateText(ETHAddress, "Copied!", 200);
            await Functions.SleepAsync(250);
            await new Functions().UpdateText(
                ETHAddress,
                "0xe40E8074E37fBF846D07c3cB7E865898156e172a",
                200
            );
        }

        private async void BTCAddress_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            Clipboard.SetText("bc1q8nwclx084z7sh3k9sfg4c8h0h0wd5xwva50c3w");
            await new Functions().UpdateText(BTCAddress, "Copied!", 200);
            await Functions.SleepAsync(250);
            await new Functions().UpdateText(
                BTCAddress,
                "bc1q8nwclx084z7sh3k9sfg4c8h0h0wd5xwva50c3w",
                200
            );
        }

        private void DiscordHyberlink_RequestNavigate(
            object sender,
            System.Windows.Navigation.RequestNavigateEventArgs e
        )
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
