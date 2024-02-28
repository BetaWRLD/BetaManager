using System.Windows.Forms;
using BetaManager.Models;
using DiscordRPC;
using Button = DiscordRPC.Button;

namespace BetaManager.Classes
{
    internal class DiscordRPCServer
    {
        private static DiscordRpcClient Client { get; set; }

        public bool isIntialized
        {
            get { return Client.IsInitialized; }
        }

        private static RichPresence presence = new RichPresence()
        {
            Details = Saved.User?.Username ?? "Starting...",
            Assets = new Assets()
            {
                LargeImageKey = "applogo",
                LargeImageText = $"Version: {Application.ProductVersion}-Beta",
            },
            Buttons = new Button[]
            {
                new Button() { Url = "https://discord.gg/sbxMVMzGsF", Label = "Discord Server" },
            }
        };

        public DiscordRPCServer()
        {
            Client = new DiscordRpcClient("1199047773698396271");
        }

        public void Update(RichPresence RP = null)
        {
            Client.SetPresence(RP ?? presence);
        }

        public void UpdateState(string newState)
        {
            Client.UpdateState(newState);
        }

        public void Kill()
        {
            Client.ClearPresence();
            Client.Dispose();
        }

        public void LoggedIn()
        {
            if (!SettingsModel.DiscordRPC)
                return;
            Client.UpdateState(("@" + Saved.User?.Username) ?? "guest");
            Client.UpdateDetails("Chilling");
        }

        public void Start()
        {
            if (!SettingsModel.DiscordRPC)
                return;
            Client.Initialize();
            Client.SetPresence(presence);
        }
    }
}
