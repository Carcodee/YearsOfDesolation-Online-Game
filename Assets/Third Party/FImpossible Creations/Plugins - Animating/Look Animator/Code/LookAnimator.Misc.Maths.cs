using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement helper math methods
    /// </summary>
    public partial class FLookAnimator
    {

        /// <summary>
        /// Wrapping angles vector to be always  between -180 to 180 degrees
        /// </summary>
        private Vector3 WrapVector(Vector3 v)
        {
            return new Vector3(FLogicMethods.WrapAngle(v.x), FLogicMethods.WrapAngle(v.y), FLogicMethods.WrapAngle(v.z));
        }


        /// <summary>
        /// Correcting bones look at rotations for Quaternion.LookRotation() to work correctly - different modelling softwares doing sometimes crazy things with this orientations
        /// </summary>
        private Vector3 ConvertFlippedAxes(Vector3 rotations)
        {
            if (FixingPreset == EFAxisFixOrder.Parental)
            {
                // Will just return rotations
            }
            else if (FixingPreset == EFAxisFixOrder.FromBased)
            {
                rotations += OffsetAuto;
                rotations = (Quaternion.Euler(rotations) * Quaternion.FromToRotation(FromAuto, ModelForwardAxis)).eulerAngles;
            }
            else if (FixingPreset == EFAxisFixOrder.FullManual)
            {
                rotations.x *= RotCorrectionMultiplier.x;
                rotations.y *= RotCorrectionMultiplier.y;
                rotations.z *= RotCorrectionMultiplier.z;

                return (Quaternion.Euler(rotations) * Quaternion.FromToRotation(ManualFromAxis, ManualToAxis)).eulerAngles;

            }
            else if (FixingPreset == EFAxisFixOrder.ZYX)
            {
                return Quaternion.Euler(rotations.z, rotations.y - 90f, -rotations.x - 90f).eulerAngles;
            }

            return rotations;
        }


        /// <summary>
        /// Calculate angle between two directions around defined axis
        /// </summary>
        public static float AngleAroundAxis(Vector3 firstDirection, Vector3 secondDirection, Vector3 axis)
        {
            // Projecting to orthogonal target axis plane
            firstDirection = firstDirection - Vector3.Project(firstDirection, axis);
            secondDirection = secondDirection - Vector3.Project(secondDirection, axis);

            float angle = Vector3.Angle(firstDirection, secondDirection);

            return angle * (Vector3.Dot(axis, Vector3.Cross(firstDirection, secondDirection)) < 0 ? -1 : 1);
        }
    
    }
}