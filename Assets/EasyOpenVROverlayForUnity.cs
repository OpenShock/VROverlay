using UnityEngine;
using Valve.VR;

namespace ShockLink.VrOverlay
{

    public class EasyOpenVROverlayForUnity : MonoBehaviour
    {
        public static EasyOpenVROverlayForUnity Instance;
        
        //エラーフラグ
        public bool error = true; //初期化失敗

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

        //オーバーレイの透明度を設定
        [Range(0, 1)] public float alpha = 0.2f;

        [Header("Name")]
        //ユーザーが確認するためのオーバーレイの名前
        public string OverlayFriendlyName = "SampleOverlay";

        //グローバルキー(システムのオーバーレイ同士の識別名)。
        //ユニークでなければならない。乱数やUUIDなどを勧める
        public string OverlayKeyName = "SampleOverlay";

        //オーバーレイのハンドル(整数)
        public ulong overlayHandle = INVALID_HANDLE;

        //OpenVRシステムインスタンス
        private CVRSystem openvr;

        //Overlayインスタンス
        public CVROverlay overlay;

        //オーバーレイに渡すネイティブテクスチャ
        private Texture_t overlayTexture;

        //HMD視点位置変換行列
        private HmdMatrix34_t p;

        //無効なハンドル
        private const ulong INVALID_HANDLE = 0;

        //--------------------------------------------------------------------------

        //エラー状態かをチェック
        public bool IsError()
        {
            return error || overlayHandle == INVALID_HANDLE || overlay == null || openvr == null;
        }

        //エラー処理(開放処理)
        private void ProcessError()
        {
            if (overlayHandle != INVALID_HANDLE && overlay != null)
            {
                overlay.DestroyOverlay(overlayHandle);
            }

            overlayHandle = INVALID_HANDLE;
            overlay = null;
            openvr = null;
            error = true;
        }

        private void OnDestroy()
        {
            ProcessError();
        }

        private void OnApplicationQuit()
        {
            ProcessError();
        }

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
            Application.targetFrameRate = 5;

            var openVRError = EVRInitError.None;
            EVROverlayError overlayError;
            error = false;

            openvr = OpenVR.Init(ref openVRError, EVRApplicationType.VRApplication_Background);
            if (openVRError != EVRInitError.None)
            {
                Debug.LogError(openVRError);
                ProcessError();
                return;
            }

            overlay = OpenVR.Overlay;
            overlayError = overlay.CreateOverlay(OverlayKeyName, OverlayFriendlyName, ref overlayHandle);
            if (overlayError != EVROverlayError.None)
            {
                Debug.LogError(overlayError);
                ProcessError();
                return;
            }

            var OverlayTextureBounds = new VRTextureBounds_t();
            var isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
            if (isOpenGL)
            {
                overlayTexture.eType = ETextureType.OpenGL;
                OverlayTextureBounds.uMin = 0;
                OverlayTextureBounds.vMin = 0;
                OverlayTextureBounds.uMax = 1;
                OverlayTextureBounds.vMax = 1;
                overlay.SetOverlayTextureBounds(overlayHandle, ref OverlayTextureBounds);
            }
            else
            {
                overlayTexture.eType = ETextureType.DirectX;
                OverlayTextureBounds.uMin = 0;
                OverlayTextureBounds.vMin = 1;
                OverlayTextureBounds.uMax = 1;
                OverlayTextureBounds.vMax = 0;
                overlay.SetOverlayTextureBounds(overlayHandle, ref OverlayTextureBounds);
            }

            overlay.SetOverlayAlpha(overlayHandle, alpha);
            overlay.SetOverlayWidthInMeters(overlayHandle, width);

            var vecMouseScale = new HmdVector2_t
            {
                v0 = renderTexture.width,
                v1 = renderTexture.height
            };
            overlay.SetOverlayMouseScale(overlayHandle, ref vecMouseScale);
            
            var quaternion = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
            var position = Position;
            position.z = -Position.z;
            var m = Matrix4x4.TRS(position, quaternion, Scale);
            
            var mirroring = new Vector3(MirrorX ? -1 : 1, MirrorY ? -1 : 1, 1);
            
            p.m0 = mirroring.x * m.m00;
            p.m1 = mirroring.y * m.m01;
            p.m2 = mirroring.z * m.m02;
            p.m3 = m.m03;
            p.m4 = mirroring.x * m.m10;
            p.m5 = mirroring.y * m.m11;
            p.m6 = mirroring.z * m.m12;
            p.m7 = m.m13;
            p.m8 = mirroring.x * m.m20;
            p.m9 = mirroring.y * m.m21;
            p.m10 = mirroring.z * m.m22;
            p.m11 = m.m23;
            
            overlay.SetOverlayTransformTrackedDeviceRelative(overlayHandle, OpenVR.k_unTrackedDeviceIndex_Hmd, ref p);


            overlay.HideOverlay(overlayHandle);
        }

        private void Update()
        {
            if (IsError()) return;

            ProcessEvent();

            if (overlay.IsOverlayVisible(overlayHandle))
            {
                //updatePosition();
                UpdateTexture();
            }
        }

        //位置情報を更新
        private void updatePosition()
        {
            var trans = EasyOpenVRUtil.GetTransform(OpenVR.k_unTrackedDeviceIndex_Hmd);
            if (trans != null)
            {
                transform.position = trans.Value.pos;
                transform.rotation = trans.Value.rot;
            }
        }

        //表示情報を更新
        private void UpdateTexture()
        {
            if (!renderTexture.IsCreated()) return;
            overlayTexture.handle = renderTexture.GetNativeTexturePtr();

            var overlayError = overlay.SetOverlayTexture(overlayHandle, ref overlayTexture);
            if (overlayError != EVROverlayError.None) Debug.LogError(overlayError);
        }

        private static readonly uint UncbVREvent =
            (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));

        private void ProcessEvent()
        {
            var Event = new VREvent_t();
            while (overlay.PollNextOverlayEvent(overlayHandle, ref Event, UncbVREvent))
            {
                switch ((EVREventType)Event.eventType)
                {
                    case EVREventType.VREvent_Quit:
                        Debug.Log("Quit");
                        ApplicationQuit();
                        return;
                }
            }
        }
    }
}