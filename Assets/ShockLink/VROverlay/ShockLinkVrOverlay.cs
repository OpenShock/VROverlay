using UnityEngine;
using Valve.VR;

namespace ShockLink.VROverlay
{
    public class ShockLinkVrOverlay : MonoBehaviour
    {
        public static ShockLinkVrOverlay Instance;

        [Header("RenderTexture")]
        //取得元のRenderTexture
        public RenderTexture renderTexture;

        [Header("Transform")]
        //Unity準拠の位置と回転
        public Vector3 Position = new(0, -0.5f, 3);

        public Vector3 Rotation = new(0, 0, 0);

        public Vector3 Scale = new(1, 1, 1);

        //鏡像反転できるように
        public bool MirrorX = false;
        public bool MirrorY = false;

        [Header("Setting")]
        //オーバーレイの大きさ設定(幅のみ。高さはテクスチャの比から自動計算される)
        [Range(0, 100)]
        public float width = 5.0f;

        // Set overlay transparency
        [Range(0, 1)] public float alpha = 0.2f;

        private const ulong InvalidHandle = 0;
        public ulong overlayHandle = InvalidHandle;

        private CVRSystem _openvr;
        public CVROverlay Overlay;

        //native texture to pass to overlay
        private Texture_t _overlayTexture;

        // HMD viewpoint position transformation matrix
        private HmdMatrix34_t _p;
        
        // General error flag for IsError
        private bool _error = true; // Init failed

        // Check for error conditions
        public bool IsError() => _error || overlayHandle == InvalidHandle || Overlay == null || _openvr == null;
        
        private void ProcessDestroy()
        {
            if (overlayHandle != InvalidHandle && Overlay != null)Overlay.DestroyOverlay(overlayHandle);
            
            overlayHandle = InvalidHandle;
            Overlay = null;
            _openvr = null;
            _error = true;
        }

        private void OnDestroy() => ProcessDestroy();
        

        private void OnApplicationQuit() => ProcessDestroy();
        

        private void ApplicationQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
        
        private void Start()
        {
            Instance = this;
            Application.targetFrameRate = 2;

            var openVRError = EVRInitError.None;
            _error = false;

            _openvr = OpenVR.Init(ref openVRError, EVRApplicationType.VRApplication_Background);
            if (openVRError != EVRInitError.None)
            {
                Debug.LogError(openVRError);
                ProcessDestroy();
                return;
            }

            Overlay = OpenVR.Overlay;
            var overlayError = Overlay.CreateOverlay("ShockLinkVROverlay", "ShockLink VROverlay", ref overlayHandle);
            if (overlayError != EVROverlayError.None)
            {
                Debug.LogError(overlayError);
                ProcessDestroy();
                return;
            }

            var overlayTextureBounds = new VRTextureBounds_t();
            var isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
            if (isOpenGL)
            {
                _overlayTexture.eType = ETextureType.OpenGL;
                overlayTextureBounds.uMin = 0;
                overlayTextureBounds.vMin = 0;
                overlayTextureBounds.uMax = 1;
                overlayTextureBounds.vMax = 1;
                Overlay.SetOverlayTextureBounds(overlayHandle, ref overlayTextureBounds);
            }
            else
            {
                _overlayTexture.eType = ETextureType.DirectX;
                overlayTextureBounds.uMin = 0;
                overlayTextureBounds.vMin = 1;
                overlayTextureBounds.uMax = 1;
                overlayTextureBounds.vMax = 0;
                Overlay.SetOverlayTextureBounds(overlayHandle, ref overlayTextureBounds);
            }

            Overlay.SetOverlayAlpha(overlayHandle, alpha);
            Overlay.SetOverlayWidthInMeters(overlayHandle, width);

            var vecMouseScale = new HmdVector2_t
            {
                v0 = renderTexture.width,
                v1 = renderTexture.height
            };
            Overlay.SetOverlayMouseScale(overlayHandle, ref vecMouseScale);

            var quaternion = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
            var position = Position;
            position.z = -Position.z;
            var m = Matrix4x4.TRS(position, quaternion, Scale);

            var mirroring = new Vector3(MirrorX ? -1 : 1, MirrorY ? -1 : 1, 1);

            _p.m0 = mirroring.x * m.m00;
            _p.m1 = mirroring.y * m.m01;
            _p.m2 = mirroring.z * m.m02;
            _p.m3 = m.m03;
            _p.m4 = mirroring.x * m.m10;
            _p.m5 = mirroring.y * m.m11;
            _p.m6 = mirroring.z * m.m12;
            _p.m7 = m.m13;
            _p.m8 = mirroring.x * m.m20;
            _p.m9 = mirroring.y * m.m21;
            _p.m10 = mirroring.z * m.m22;
            _p.m11 = m.m23;

            Overlay.SetOverlayTransformTrackedDeviceRelative(overlayHandle, OpenVR.k_unTrackedDeviceIndex_Hmd, ref _p);


            Overlay.HideOverlay(overlayHandle);
        }

        private void Update()
        {
            if (IsError()) return;

            ProcessEvent();

            if (Overlay.IsOverlayVisible(overlayHandle))
            {
                //UpdatePosition();
                UpdateTexture();
            }
        }
        
        private void UpdatePosition()
        {
            var trans = EasyOpenVRUtil.GetTransform(OpenVR.k_unTrackedDeviceIndex_Hmd);
            if (trans == null) return;
            var localTrans = transform;
            localTrans.position = trans.Value.pos;
            localTrans.rotation = trans.Value.rot;
        }

        //表示情報を更新
        private void UpdateTexture()
        {
            if (!renderTexture.IsCreated()) return;
            _overlayTexture.handle = renderTexture.GetNativeTexturePtr();

            var overlayError = Overlay.SetOverlayTexture(overlayHandle, ref _overlayTexture);
            if (overlayError != EVROverlayError.None) Debug.LogError(overlayError);
        }

        private static readonly uint UncbVREvent =
            (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));

        private void ProcessEvent()
        {
            var vrEvent = new VREvent_t();
            while (Overlay.PollNextOverlayEvent(overlayHandle, ref vrEvent, UncbVREvent))
            {
                switch ((EVREventType)vrEvent.eventType)
                {
                    case EVREventType.VREvent_Quit:
                        ApplicationQuit();
                        return;
                }
            }
        }
    }
}