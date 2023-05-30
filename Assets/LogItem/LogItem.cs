using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;

public class LogItem : MonoBehaviour
{
    public TMP_Text MainText;

    public void Configure(GenericIni sender, ControlLog log)
    {
        EasyOpenVROverlayForUnity.Instance.overlay.ShowOverlay(EasyOpenVROverlayForUnity.Instance.overlayHandle);
        MainText.text = $"{sender.Name} -> {log.Shocker.Name} - {log.Intensity}:{(log.Duration / 1000).ToString(CultureInfo.InvariantCulture)}";

        StartCoroutine(DeleteAfter(5));
    }

    private IEnumerator DeleteAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Destroying log game object");
        UiManager.Instance.logItems.Remove(this);
        Destroy(gameObject);

        if (UiManager.Instance.logItems.Count <= 0) EasyOpenVROverlayForUnity.Instance.overlay.HideOverlay(EasyOpenVROverlayForUnity.Instance.overlayHandle);
        
    }
}
