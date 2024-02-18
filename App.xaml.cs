using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using BetaManager.Classes;
using BetaManager.Models;
using BetaManager.Views;

namespace BetaManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static void Application_ThreadException(
            object sender,
            System.Threading.ThreadExceptionEventArgs e
        )
        {
            Saved.Logger.Log("Thread Error", e.ToString());
        }

        public static void AppDomain_UnhandledException(
            object sender,
            System.UnhandledExceptionEventArgs e
        )
        {
            Saved.Logger.Log("Unhandled Exception", e.ToString());
        }

        private const string MutexName = "BetaManager";
        private Mutex mutex;

        protected async void ApplicationStart(object sender, StartupEventArgs e)
        {
            Instances.AppDispatcher = this.Dispatcher;
            bool createdNew;
            mutex = new Mutex(true, MutexName, out createdNew);

            if (createdNew)
            {
                new Thread(() => Functions.ListenForProtocol()).Start();
            }
            else
            {
                if (e.Args.Length > 0)
                {
                    string url = e.Args[0];
                    Functions.NotifyRunningInstance(url);
                }
                else
                    Functions.NotifyRunningInstance("null");
                Process.GetCurrentProcess().Kill();
            }
            if (!SettingsModel.RegisteredProtocol)
            {
                if (!Functions.IsAdministrator())
                {
                    DialogResult result = System.Windows.Forms.MessageBox.Show(
                        "BetaManager needs admin rights to register the protocol for the shortcuts. Do you want to restart as admin?",
                        "BetaManager",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        Functions.RestartAsAdmin();
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                    return;
                }
                else
                    Functions.RegisterProtocol();
            }
            Saved.CurrentVersion =
                "U2FsdGVkX18vImy16HrgFdXqzt64vAx292Nf79NzkO1VsRZiEhXq0UwNWWjU81xR";
            Directory.CreateDirectory(Saved.SaveLocation + "Logs");
            Saved.Logger = new Logger();
            AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(
                AppDomain_UnhandledException
            );
            System.Windows.Forms.Application.ThreadException +=
                new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //Auth.KillAll();
            Saved.Logger.Log("757438");
            Directory.CreateDirectory(Saved.SaveLocation + "Games\\Images\\Screenshots");
            Directory.CreateDirectory(Saved.SaveLocation + "User");
            new GamesMonitor().StartTracking();
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            using (NetworkInterfacesHandler NIH = new NetworkInterfacesHandler())
            {
                Instances.NetworkInterfacesHandler = NIH;
                NIH.NetwordChanged += NIH.NetwordChangedHandler;
                NIH.NetwordChanged += EventsHandler.NetworkInterfacesHandlerEvent;
            }
            if (Functions.GetSetting("AutoUpdateChecking") == null)
                SettingsModel.AutoUpdateChecking = true;
            if (Functions.GetSetting("GuestUser") == null)
                SettingsModel.GuestUser = false;
            if (Functions.GetSetting("TotalGamesToLoad") == null)
                SettingsModel.TotalGamesToLoad = 50;
            Saved.Logger.Log("729296");
            new ManagerView();
            Saved.Logger.Log("988954");
            new Functions().LoadDownloadsFromXml();
            Saved.Logger.Log("382625");
            Saved.Logger.Log("56718");
            Saved.Logger.Log("2026");
            Instances.DiscordClient.Start();
            new MainView(e).Show();
        }
    }
}
