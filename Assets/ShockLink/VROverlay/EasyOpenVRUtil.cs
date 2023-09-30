using System;
using Valve.VR;

namespace ShockLink.VROverlay
{
    public static class EasyOpenVRUtil
    {
        public static SteamVR_Utils.RigidTransform? GetTransform(uint index)
        {
            if (OpenVR.System == null) return null;
            var allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding,
                GetPredictedTime(), allDevicePose);

            return new SteamVR_Utils.RigidTransform(allDevicePose[index].mDeviceToAbsoluteTracking);
        }

        private static bool GetPropertyFloat(uint idx, ETrackedDeviceProperty prop, out float result)
        {
            var error = new ETrackedPropertyError();
            result = OpenVR.System.GetFloatTrackedDeviceProperty(idx, prop, ref error);
            return error == ETrackedPropertyError.TrackedProp_Success;
        }

        //Get the current estimated delay time (action-photon delay time)
        private static float GetPredictedTime()
        {
            float frameTime = 0;
            ulong frameCount = 0;

            if (OpenVR.System == null) return 0;
            if (!OpenVR.System.GetTimeSinceLastVsync(ref frameTime, ref frameCount)) return 0;
            if (frameTime > 1.0f) return 0;


            //Get time per frame
            if (!GetPropertyFloat(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_DisplayFrequency_Float,
                    out var displayFrequency)) return 0;

            //Acquisition of photon delay time (time required from output to HMD projection)
            if (!GetPropertyFloat(OpenVR.k_unTrackedDeviceIndex_Hmd,
                    ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float, out var photonDelay)) return 0;

            //Predicted delay time (time per frame - current frame elapsed time + photon delay time)
            var predictedTimeNow = 1f / displayFrequency - frameTime + photonDelay;

            return Math.Min(0, predictedTimeNow);
        }
    }
}