using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace ShockLink.VROverlay.ActiveShocker
{
    public class ActiveShocker : MonoBehaviour
    {
        public TMP_Text ShockerName;
        public TMP_Text Counter;

        [Header("Type Icons")] public GameObject TypeShock;
        public GameObject TypeVibrate;
        public GameObject TypeSound;
        public GameObject TypeUnkown;
        public GameObject TypeStop;

        private DateTime _until;
        private string _id;
        private ActiveShockerManager _manager;

        public void Configure(ActiveShockerManager manager, string id, string shockerName)
        {
            _manager = manager;
            _id = id;
            ShockerName.text = shockerName;
        }

        public void UpdateInfo(ControlType type, DateTime until)
        {
            _until = until;
            TypeShock.SetActive(false);
            TypeSound.SetActive(false);
            TypeVibrate.SetActive(false);
            TypeUnkown.SetActive(false);
            TypeStop.SetActive(false);
            
            switch (type)
            {
                case ControlType.Shock:
                    TypeShock.SetActive(true);
                    break;
                case ControlType.Vibrate:
                    TypeVibrate.SetActive(true);
                    break;
                case ControlType.Sound:
                    TypeSound.SetActive(true);
                    break;
                case ControlType.Stop:
                    TypeStop.SetActive(true);
                    break;
                default:
                    TypeUnkown.SetActive(true);
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // ReSharper disable once PossibleLossOfFraction
            var rawTime = (float)((int)_until.Subtract(DateTime.UtcNow).TotalMilliseconds / 100) / 10;
            var timeLeft = Math.Max(0, rawTime);
            Counter.text = $"{timeLeft}s";

            if (rawTime <= -1.5) RemoveSelf();
        }

        private void RemoveSelf()
        {
            Debug.Log("Removing active shocker object");
            _manager.RemoveActiveShocker(_id);
            Destroy(gameObject);
        }
    }
}