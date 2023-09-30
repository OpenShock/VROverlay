using System.Collections.Generic;
using ShockLink.API.Models;
using UnityEngine;

namespace ShockLink.VROverlay
{
    public class UiManager : MonoBehaviour
    {
        public static UiManager Instance;
        public List<ILogReceiver> LogReceivers;
        public List<string> aaa;

        private void Awake()
        {
            Instance = this;
        }

        public void AddLog(GenericIni sender, ControlLog log)
        {
            foreach (var logReceiver in LogReceivers)logReceiver.LogReceive(sender, log);
        }
    }
}