using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShockLink.VrOverlay
{
    public class LogItem : MonoBehaviour
    {
        public TMP_Text Text;

        [Header("Type Icons")] public GameObject TypeShock;
        public GameObject TypeVibrate;
        public GameObject TypeSound;
        public GameObject TypeUnkown;

        public void Configure(GenericIni sender, ControlLog log)
        {
            if (EasyOpenVROverlayForUnity.Instance != null)
                EasyOpenVROverlayForUnity.Instance.overlay.ShowOverlay(EasyOpenVROverlayForUnity.Instance
                    .overlayHandle);
            Text.text =
                $"{log.Shocker.Name} <color=#e3e3e3>{log.Intensity}<color=#a1a1a1>:</color>{(log.Duration / 1000f).ToString(CultureInfo.InvariantCulture)}</color> <color=#828282>{sender.Name}</color>";

            TypeUnkown.SetActive(false);
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
            UiManager.Instance.logItems.Remove(this);
            Destroy(gameObject);

            if (UiManager.Instance.logItems.Count <= 0)
                if (EasyOpenVROverlayForUnity.Instance != null)
                    EasyOpenVROverlayForUnity.Instance.overlay.HideOverlay(EasyOpenVROverlayForUnity.Instance
                        .overlayHandle);
        }
    }
}