using System.Collections;
using System.Globalization;
using ShockLink.API.Models;
using TMPro;
using UnityEngine;

namespace ShockLink.VROverlay.LogItem
{
    public class LogItem : MonoBehaviour
    {
        public TMP_Text Text;

        [Header("Type Icons")] public GameObject TypeShock;
        public GameObject TypeVibrate;
        public GameObject TypeSound;
        public GameObject TypeStop;
        public GameObject TypeUnkown;

        private LogItemManager _manager;

        public void Configure(LogItemManager manager, GenericIni sender, ControlLog log)
        {
            _manager = manager;
            
            if (ShockLinkVrOverlay.Instance != null)
                ShockLinkVrOverlay.Instance.Overlay.ShowOverlay(ShockLinkVrOverlay.Instance
                    .overlayHandle);
            Text.text =
                $"{log.Shocker.Name} <color=#e3e3e3>{log.Intensity}<color=#a1a1a1>:</color>{(log.Duration / 1000f).ToString(CultureInfo.InvariantCulture)}</color> <color=#ababab>{sender.Name}</color>";
            
            switch (log.Type)
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
                    Text.text = $"{log.Shocker.Name} STOP <color=#828282>{sender.Name}</color>";
                    TypeStop.SetActive(true);
                    break;
                default:
                    TypeUnkown.SetActive(true);
                    break;
            }

            StartCoroutine(DeleteAfter(5));
        }

        private IEnumerator DeleteAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Debug.Log("Destroying log game object");
            _manager.RemoveItem(this);
            Destroy(gameObject);
            
            VisManager.Instance.Check();
        }
    }
}