using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ShockLink.VROverlay
{
    public class InitShockLink : MonoBehaviour
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SWHide = 0;

        private void Start()
        {
/*#if !UNITY_EDITOR
        if(!Environment.GetCommandLineArgs().Contains("--show-window")) {
            var hwnd = GetActiveWindow();
            ShowWindow(hwnd, SWHide);
        }
#endif*/
            ShockLinkUserHub.Start();
        }
    }
}