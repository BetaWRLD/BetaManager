using System.Runtime.InteropServices;

namespace BetaManager
{
    public class PowerManagement
    {
        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetSuspendState(
            bool hibernate,
            bool forceCritical,
            bool disableWakeEvent
        );

        [DllImport("user32.dll")]
        private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        private const uint EWX_SHUTDOWN = 0x00000001;
        private const uint EWX_REBOOT = 0x00000002;
        private const uint EWX_LOGOFF = 0x00000000;
        private const uint EWX_FORCE = 0x00000004;

        public static void Sleep()
        {
            SetSuspendState(false, false, false);
        }

        public static void Hibernate()
        {
            SetSuspendState(true, false, false);
        }

        public static void ShutDown()
        {
            ExitWindowsEx(EWX_SHUTDOWN | EWX_FORCE, 0);
        }

        public static void Restart()
        {
            ExitWindowsEx(EWX_REBOOT | EWX_FORCE, 0);
        }

        public static void LogOff()
        {
            ExitWindowsEx(EWX_LOGOFF | EWX_FORCE, 0);
        }
    }
}
