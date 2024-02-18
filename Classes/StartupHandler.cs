using System;

namespace BetaManager.Classes
{
    internal class StartupHandler : IDisposable
    {
        public string Function { get; private set; }
        public string SecondArgument { get; private set; }

        public StartupHandler(string URL)
        {
            if (URL == null)
                return;
            if (URL.StartsWith("betamanager://"))
            {
                string[] parts = URL.Split('/');
                if (parts.Length == 4)
                {
                    Function = parts[2];
                    SecondArgument = parts[3];
                }
            }
        }

        public void Dispose() { }
    }
}
