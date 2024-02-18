using System;
using System.Collections.Generic;
using System.Threading;
using BetaManager.Models;

namespace BetaManager.Classes
{
    internal class NetworkInterfacesHandler : IDisposable
    {
        public List<VPNInterfaceModel> InterfacesList = new List<VPNInterfaceModel>();
        public event EventHandler NetwordChanged;

        public NetworkInterfacesHandler()
        {
            if (InterfacesList.Count == 0)
            {
                InterfacesList = Functions.GetNetworkInterfaces();
            }
            new Thread(() =>
            {
                while (true)
                {
                    EventsHandler.NetworkInterfacesHandlerEvent(null, null);
                    var list = Functions.GetNetworkInterfaces();
                    bool isWqual = ListsAreEqual(InterfacesList, list);
                    if (!isWqual)
                    {
                        InterfacesList = list;
                        NetwordChanged.Invoke(this, new EventArgs());
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        static bool ListsAreEqual(List<VPNInterfaceModel> list1, List<VPNInterfaceModel> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1.Find(a => a.Name == list2[i].Name) == null)
                {
                    return false;
                }
            }

            return true;
        }

        public void NetwordChangedHandler(object sender, EventArgs e) { }

        public void Dispose() { }
    }
}
