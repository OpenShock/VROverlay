using EasyLazyLibrary;
using UnityEngine;
using Valve.VR;

public class EasyOpenVROverlayForUnity : MonoBehaviour
{
    public static EasyOpenVROverlayForUnity Instance;
    
    //エラーフラグ
    public bool error = true; //初期化失敗

    //イベントに関するログを表示するか
    public bool eventLog = false;

    [Header("RenderTexture")]
    //取得元のRenderTexture
    public RenderTexture renderTexture;

    [Header("Transform")]
    //Unity準拠の位置と回転
    public Vector3 Position = new Vector3(0, -0.5f, 3);

    public Vector3 Rotation = new Vector3(0, 0, 0);

    public Vector3 Scale = new Vector3(1, 1, 1);

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

    [Header("DeviceTracking")]
    //絶対空間か
    public bool DeviceTracking = true;

    //追従対象デバイス。HMD=0
    //public uint DeviceIndex = OpenVR.k_unTrackedDeviceIndex_Hmd;
    public TrackingDeviceSelect DeviceIndex = TrackingDeviceSelect.HMD;
    private int DeviceIndexOld = (int)TrackingDeviceSelect.None;

    [Header("Absolute space")]
    //(絶対空間の場合)ルームスケールか、着座状態か
    public bool Seated = false;

    //着座カメラのリセット(リセット後自動でfalseに戻ります)
    public bool ResetSeatedCamera = false;

    //追従対象リスト。コントロラーは変動するので特別処理
    public enum TrackingDeviceSelect
    {
        None = -99,
        RightController = -2,
        LeftController = -1,
        HMD = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
        Device1 = 1,
        Device2 = 2,
        Device3 = 3,
        Device4 = 4,
        Device5 = 5,
        Device6 = 6,
        Device7 = 7,
        Device8 = 8,
    }

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
        
        overlay.HideOverlay(overlayHandle);
    }

    private void Update()
    {
        if (IsError()) return;

        ProcessEvent();
        
        if (overlay.IsOverlayVisible(overlayHandle))
        {
            updatePosition();
            UpdateTexture();
        }
    }

    readonly EasyOpenVRUtil util = new();

    //位置情報を更新
    private void updatePosition()
    {
        var trans = util.GetHMDTransform();
        if (trans != null)
        {
            transform.position = trans.position;
            transform.rotation = trans.rotation;
        }

        //RenderTextureが生成されているかチェック
        if (!renderTexture.IsCreated())
        {
            return;
        }

        //回転を生成
        Quaternion quaternion = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
        //座標系を変更(右手系と左手系の入れ替え)
        Vector3 position = Position;
        position.z = -Position.z;
        //HMD視点位置変換行列に書き込む。
        Matrix4x4 m = Matrix4x4.TRS(position, quaternion, Scale);

        //鏡像反転
        Vector3 Mirroring = new Vector3(MirrorX ? -1 : 1, MirrorY ? -1 : 1, 1);

        //4x4行列を3x4行列に変換する。
        p.m0 = Mirroring.x * m.m00;
        p.m1 = Mirroring.y * m.m01;
        p.m2 = Mirroring.z * m.m02;
        p.m3 = m.m03;
        p.m4 = Mirroring.x * m.m10;
        p.m5 = Mirroring.y * m.m11;
        p.m6 = Mirroring.z * m.m12;
        p.m7 = m.m13;
        p.m8 = Mirroring.x * m.m20;
        p.m9 = Mirroring.y * m.m21;
        p.m10 = Mirroring.z * m.m22;
        p.m11 = m.m23;

        //回転行列を元に相対位置で表示
        if (DeviceTracking)
        {
            //deviceindexを処理(コントローラーなどはその時その時で変わるため)
            var idx = OpenVR.k_unTrackedDeviceIndex_Hmd;
            switch (DeviceIndex)
            {
                case TrackingDeviceSelect.LeftController:
                    idx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
                    break;
                case TrackingDeviceSelect.RightController:
                    idx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
                    break;
                default:
                    idx = (uint)DeviceIndex;
                    break;
            }

            //HMDからの相対的な位置にオーバーレイを表示する。
            overlay.SetOverlayTransformTrackedDeviceRelative(overlayHandle, idx, ref p);
        }
        else
        {
            //空間の絶対位置にオーバーレイを表示する
            if (!Seated)
            {
                overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding,
                    ref p);
            }
            else
            {
                overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated,
                    ref p);
            }
        }

    }

    //表示情報を更新
    private void UpdateTexture()
    {
        if(!renderTexture.IsCreated()) return;
        overlayTexture.handle = renderTexture.GetNativeTexturePtr();
        
        var overlayError = overlay.SetOverlayTexture(overlayHandle, ref overlayTexture);
        if (overlayError != EVROverlayError.None) Debug.LogError(overlayError);
    }

    private static readonly uint UncbVREvent = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));
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