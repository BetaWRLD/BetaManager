using DiscordRPC;

namespace BetaManager.Classes
{
    internal class DiscordRPCServer
    {
        private string _version { get; set; }
        private string _username { get; set; }
        private string _state { get; set; }
        public DiscordRpcClient Client { get; set; }

        private static RichPresence presence = new RichPresence()
        {
            Details = Saved.User?.Username ?? "Starting...",
            Assets = new Assets()
            {
                LargeImageKey = "applogo",
                LargeImageText = $"Version: 1.5.0-Beta",
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

        public void LoggedIn()
        {
            Client.UpdateState(("@" + Saved.User?.Username) ?? "guest");
            Client.UpdateDetails("Chilling");
        }

        public void Start()
        {
            Client.Initialize();
            Client.SetPresence(presence);
        }
    }
}
