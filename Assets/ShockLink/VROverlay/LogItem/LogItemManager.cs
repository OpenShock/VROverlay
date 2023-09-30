using System;
using System.Collections.Generic;
using ShockLink.API.Models;
using UnityEngine;

namespace ShockLink.VROverlay.LogItem
{
    public class LogItemManager : MonoBehaviour, IUiElement, ILogReceiver
    {
        public GameObject LogGo;
        public GameObject LogPrefab;
        public List<LogItem> logItems = new();

        public bool HasVisibleObjects() => logItems.Count > 0;

        public void LogReceive(GenericIni sender, ControlLog log)
        {
            var go = Instantiate(LogPrefab, LogGo.transform);
            var logItem = go.GetComponent<LogItem>();
            if (logItem == null) throw new Exception("Log item is null");

            logItem.Configure(this, sender, log);
            logItems.Add(logItem);
        }

        public void RemoveItem(LogItem item)
        {
            logItems.Remove(item);
        }
    }
}