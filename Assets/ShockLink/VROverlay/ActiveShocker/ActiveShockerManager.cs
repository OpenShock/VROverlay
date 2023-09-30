using System;
using System.Collections.Generic;
using ShockLink.API.Models;
using UnityEngine;

namespace ShockLink.VROverlay.ActiveShocker
{
    public class ActiveShockerManager : MonoBehaviour, IUiElement, ILogReceiver
    {
        public GameObject ActiveShockerHolder;
        public GameObject ActiveShockerPrefab;
        private readonly Dictionary<string, ActiveShocker> _activeShockers = new();
        
        public void RemoveActiveShocker(string id)
        {
            _activeShockers.Remove(id, out _);
        }
        
        private void OnLogReceived(string id, string shockerName, DateTime until, ControlType type)
        {
            var isStop = type == ControlType.Stop;
            if (!_activeShockers.ContainsKey(id))
            {
                if (isStop) return;
                _activeShockers.Add(id, CreateNewActiveShocker(id, shockerName));
            };
            _activeShockers[id].UpdateInfo(type, isStop ? DateTime.UtcNow : until);
        }

        private ActiveShocker CreateNewActiveShocker(string id, string shockerName)
        {
            Debug.Log("Creating new active shocker object...");
            var go = Instantiate(ActiveShockerPrefab, ActiveShockerHolder.transform);
            var activeShocker = go.GetComponent<ActiveShocker>();
            if (activeShocker == null) throw new Exception("Active shocker item is null");
            activeShocker.Configure(this, id, shockerName);
            return activeShocker;
        }

        public bool HasVisibleObjects() => _activeShockers.Count > 0;
        public void LogReceive(GenericIni sender, ControlLog log) => OnLogReceived(log.Shocker.Id, log.Shocker.Name, DateTime.UtcNow.AddMilliseconds(log.Duration), log.Type);
        
    }
}