using UnityEngine;
using Valve.VR;

namespace EasyLazyLibrary
{
    public class EasyOpenVRUtil
    {
        //定数定義
        public const uint InvalidDeviceIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

        //VRハンドル
        CVRSystem openvr;

        //内部保持用全デバイス姿勢
        TrackedDevicePose_t[] allDevicePose;

        //デバイス姿勢を常にアップデートするか
        bool autoupdate = true;

        //光子遅延補正予測時間(0=補正なし or 予測時間取得失敗)
        float PredictedTime;

        //最終更新フレームカウント
        int LastFrameCount;

        //姿勢クラス
        public class Transform
        {
            public uint deviceid = InvalidDeviceIndex;
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 velocity = Vector3.zero;
            public Vector3 angularVelocity = Vector3.zero;

            //デバッグ用
            public override string ToString()
            {
                return "deviceid: " + deviceid + " position:" + position.ToString() + " rotation:" + rotation.ToString() + " velocity:"+ velocity.ToString() + " angularVelocity:" + angularVelocity.ToString();
            }
        }

        public EasyOpenVRUtil()
        {
            //とりあえず初期化する

            if (System.Diagnostics.Process.GetProcessesByName("vrmonitor").Length > 0)
            {
                Init();
            }
        }

        public uint GetHMDIndex()
        {
            if (!IsReady()) { return InvalidDeviceIndex; }
            return OpenVR.k_unTrackedDeviceIndex_Hmd;
        }

        //初期化。失敗したらfalse
        public void Init()
        {
            openvr = OpenVR.System;
        }

        //本ライブラリが利用可能か確認する
        public bool IsReady()
        {
            return openvr != null;
        }

        //全デバイス情報を更新
        public void Update()
        {
            allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            if (!IsReady()) { return; }
            //すべてのデバイスの情報を取得
            openvr.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, PredictedTime, allDevicePose);
            //最終更新フレームを更新
            LastFrameCount = Time.frameCount;
        }

        //デバイスが有効か
        public bool IsDeviceValid(uint index)
        {
            //自動更新処理
            if (autoupdate)
            {
                //前回と違うフレームの場合のみ更新
                if (LastFrameCount != Time.frameCount)
                {
                    UpdatePredictedTime(); //光子遅延時間のアップデート追加
                    Update();
                }
            }
            //情報が有効でないなら更新
            if (allDevicePose == null)
            {
                Update();
            }
            //それでも情報が有効でないなら失敗
            if (allDevicePose == null)
            {
                return false;
            }

            //device indexが有効
            if (index != OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                //接続されていて姿勢情報が有効
                if (allDevicePose[index].bDeviceIsConnected && allDevicePose[index].bPoseIsValid)
                {
                    return true;
                }
            }
            return false;
        }

        public SteamVR_Utils.RigidTransform? GetHMDTransform()
        {
            return GetTransform(GetHMDIndex());
        }

        //指定デバイスの姿勢情報を取得
        public SteamVR_Utils.RigidTransform? GetTransform(uint index)
        {
            //有効なデバイスか
            if (!IsDeviceValid(index))
            {
                return null;
            }

            var Pose = allDevicePose[index];
            return new SteamVR_Utils.RigidTransform(Pose.mDeviceToAbsoluteTracking);
        }

        //device情報を取得する
        public bool GetPropertyFloat(uint idx, ETrackedDeviceProperty prop, out float result)
        {
            ETrackedPropertyError error = new ETrackedPropertyError();
            result = openvr.GetFloatTrackedDeviceProperty(idx, prop, ref error);
            return (error == ETrackedPropertyError.TrackedProp_Success);
        }
        

        //----------------光子遅延時間----------------------------

        //予測遅延時間(動作-光子遅延時間)を設定
        public void UpdatePredictedTime()
        {
            PredictedTime = GetPredictedTime();
        }
        
        //現在の予測遅延時間(動作-光子遅延時間)を取得
        public float GetPredictedTime()
        {
            //最後のVsyncからの経過時間(フレーム経過時間)を取得
            float FrameTime = 0;
            ulong FrameCount = 0;

            if (!IsReady()) { return 0; }

            if (!openvr.GetTimeSinceLastVsync(ref FrameTime, ref FrameCount))
            {
                return 0; //有効な値を取得できなかった
            }

            //たまにすごい勢いで増えることがある
            if (FrameTime > 1.0f)
            {
                return 0; //有効な値を取得できなかった
            }

            //1フレームあたりの時間取得
            float DisplayFrequency = 0;
            if (!GetPropertyFloat(GetHMDIndex(), ETrackedDeviceProperty.Prop_DisplayFrequency_Float, out DisplayFrequency))
            {
                return 0; //有効な値を取得できなかった
            }
            float DisplayCycle = 1f / DisplayFrequency;

            //光子遅延時間(出力からHMD投影までにかかる時間)取得
            float PhotonDelay = 0;
            if (!GetPropertyFloat(GetHMDIndex(), ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float, out PhotonDelay))
            {
                return 0; //有効な値を取得できなかった
            }

            //予測遅延時間(1フレームあたりの時間 - 現在フレーム経過時間 + 光子遅延時間)
            var PredictedTimeNow = DisplayCycle - FrameTime + PhotonDelay;

            //負の値は過去になる。
            if (PredictedTimeNow < 0)
            {
                return 0;
            }

            return PredictedTimeNow;
        }


    }
}