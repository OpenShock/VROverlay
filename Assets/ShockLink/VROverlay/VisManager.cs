using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace ShockLink.VROverlay
{
    public class VisManager : MonoBehaviour
    {
        public static VisManager Instance;
        public List<IUiElement> UiElements;

        private bool _lastOverlayState = false;

        private void Awake()
        {
            Instance = this;
        }

        public void Check()
        {
            var vis = false;
            foreach (var element in UiElements)
                if (element.HasVisibleObjects())
                {
                    vis = true;
                    break;
                }

            UpdateOverlayIfNeeded(vis);
        }

        private void UpdateOverlayIfNeeded(bool vis)
        {
            if (vis == _lastOverlayState) return;
            _lastOverlayState = vis;
            if (vis) OpenVR.Overlay.ShowOverlay(ShockLinkVrOverlay.Instance.overlayHandle);
            else OpenVR.Overlay.HideOverlay(ShockLinkVrOverlay.Instance.overlayHandle);
        }
    }
}