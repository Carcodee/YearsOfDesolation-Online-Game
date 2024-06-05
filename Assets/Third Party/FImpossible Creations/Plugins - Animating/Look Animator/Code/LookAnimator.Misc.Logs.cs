using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement debugging features
    /// </summary>
    public partial class FLookAnimator
    {
        /// <summary>
        /// Checking if base references are assigned, if not then printing warning log
        /// </summary>
        private void _LOG_NoRefs()
        {
#if UNITY_EDITOR
            if (LeadBone != null && BaseTransform != null) return;
            if (!BaseTransform) { Debug.Log("[Look Animator] No Base Transform Defined in " + name + "!"); return; }
            if (!LeadBone) { Debug.Log("[Look Animator] No Lead Bone Defined in " + name + "!"); return; }
#endif
        }

        private void _Debug_Rays()
        {
            if (!DebugRays) return;

            Debug.DrawRay(GetLookStartMeasurePosition() + Vector3.up * 0.01f, Quaternion.Euler(finalLookAngles) * BaseTransform.TransformDirection(ModelForwardAxis), Color.cyan);
            //Vector3 startLook = GetLookStartMeasurePosition();

            //Debug.DrawRay(startLook + Vector3.up * 0.25f, axisCorrectionMatrix.MultiplyVector(Vector3.forward), Color.blue);
            //Debug.DrawRay(startLook + Vector3.up * 0.25f, axisCorrectionMatrix.MultiplyVector(Vector3.up), Color.green);

            //Debug.DrawRay(GetLookStartMeasurePosition() + Vector3.up * 0.9f, Quaternion.Euler(finalLookAngles) * Vector3.forward, Color.magenta);
            //Debug.DrawRay(GetLookStartMeasurePosition() + Vector3.up, Quaternion.Euler(finalLookAngles) * ModelForwardAxis, Color.cyan);

            //Quaternion fromto = Quaternion.FromToRotation(Vector3.forward, ModelForwardAxis) * Quaternion.FromToRotation(Vector3.up, ModelUpAxis);
            //Quaternion rot = Quaternion.Euler(finalLookAngles) * fromto * BaseTransform.rotation;

            //Debug.DrawRay(GetLookStartMeasurePosition() + Vector3.up * 1.1f, rot * Vector3.forward, Color.yellow);

            //fromto = Quaternion.FromToRotation(Vector3.right, Vector3.Cross(Vector3.up, ModelForwardAxis));
            //rot = fromto * Quaternion.Euler(finalLookAngles) * BaseTransform.rotation;

            //Debug.DrawRay(GetLookStartMeasurePosition() + Vector3.up * 1.2f, rot * Vector3.forward, Color.red);
        }

    }
}