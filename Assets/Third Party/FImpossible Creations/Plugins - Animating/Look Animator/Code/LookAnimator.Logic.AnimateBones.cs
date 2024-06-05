using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement rotation animation behaviour for bones
    /// </summary>
    public partial class FLookAnimator
    {
        public int ParentalOffsetsV = 2;

        /// <summary>
        /// Not using offsets, just hard bones rotations setup
        /// </summary>
        private void AnimateBonesUnsynced(Quaternion diffOnMain, Quaternion backTarget, float d)
        {
            Quaternion refRot;
            if (nodValue > 0f) if (BackBonesNod != 0f) backTarget *= Quaternion.Euler(NodAxis * (nodValue * nodPower * -NoddingTransitions * 40f) * BackBonesNod);

            for (int i = 1; i < LookBones.Count; i++)
            {
                LookBones[i].Transform.localRotation = LookBones[i].localStaticRotation;

                refRot = (backTarget * LookBones[i].correctionOffsetQ) * Quaternion.Inverse(diffOnMain * LookBones[i].animatedStaticRotation);
                LookBones[i].CalculateMotion(refRot, WeightsMultiplier, d, finalMotionWeight);
            }

            LookBones[0].Transform.localRotation = LookBones[0].localStaticRotation;

            if (nodValue <= 0f)
                refRot = (targetLookRotation * LookBones[0].correctionOffsetQ) * Quaternion.Inverse(diffOnMain * LookBones[0].animatedStaticRotation);
            else
                refRot = (targetLookRotation * LookBones[0].correctionOffsetQ * Quaternion.Euler(NodAxis * (nodValue * nodPower * -NoddingTransitions * 40f))) * Quaternion.Inverse(diffOnMain * LookBones[0].animatedStaticRotation);

            //refRot = (targetLookRotation * LookBones[0].correctionOffsetQ) * Quaternion.Inverse(diffOnMain * LookBones[0].animatedStaticRotation);
            LookBones[0].CalculateMotion(refRot, WeightsMultiplier, d, finalMotionWeight);
        }


        /// <summary>
        /// Using offsets to rotate bones to sync it with Unity's Animator animation
        /// </summary>
        private void AnimateBonesSynced(Quaternion diffOnMain, Quaternion backTarget, float d)
        {
            Quaternion refRot;
            if (nodValue > 0f) if (BackBonesNod != 0f) backTarget *= Quaternion.Euler(NodAxis * (nodValue * nodPower * -NoddingTransitions * 40f) * BackBonesNod);

            for (int i = LookBones.Count - 1; i >= 1; i--)
            {
                refRot = (backTarget * LookBones[i].correctionOffsetQ) * Quaternion.Inverse(diffOnMain * LookBones[i].animatedStaticRotation);
                LookBones[i].CalculateMotion(refRot, WeightsMultiplier, d, finalMotionWeight);
            }

            if (nodValue <= 0f)
                refRot = (targetLookRotation * LookBones[0].correctionOffsetQ) * Quaternion.Inverse(diffOnMain * LookBones[0].animatedStaticRotation);
            else
                refRot = (targetLookRotation * (LookBones[0].correctionOffsetQ * Quaternion.Euler(NodAxis * (nodValue * nodPower * -NoddingTransitions * 40f)))) * Quaternion.Inverse(diffOnMain * LookBones[0].animatedStaticRotation);

            LookBones[0].CalculateMotion(refRot, WeightsMultiplier, d, finalMotionWeight);
        }


        /// <summary>
        /// Offsets bones to target look rotations
        /// </summary>
        private void AnimateBonesParental(float d)
        {
            float nodFactor = (nodValue * nodPower * -NoddingTransitions * 40f);
            float backNodFactor = nodFactor * BackBonesNod;

            bool offs = false;
            if (BackBonesAddOffset != Vector3.zero || NoddingTransitions != 0f) offs = true;

            for (int i = LookBones.Count - 1; i >= 1; i--)
            {
                Quaternion offset = Quaternion.identity;

                if (offs || LookBones[i].correctionOffset != Vector3.zero)
                {
                    if (ParentalOffsetsV == 2)
                    {
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.x + LookBones[i].correctionOffset.x + NodAxis.x * backNodFactor, LookBones[i].right);
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.y + LookBones[i].correctionOffset.y + NodAxis.y * backNodFactor, LookBones[i].up);
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.z + LookBones[i].correctionOffset.z + NodAxis.z * backNodFactor, LookBones[i].forward);
                    }
                    else
                    if (ParentalOffsetsV == 1)
                    {
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.x + LookBones[i].correctionOffset.x + NodAxis.x * backNodFactor, LookBones[i].Transform.right);
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.y + LookBones[i].correctionOffset.y + NodAxis.y * backNodFactor, LookBones[i].Transform.up);
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.z + LookBones[i].correctionOffset.z + NodAxis.z * backNodFactor, LookBones[i].Transform.forward);
                    }
                    else
                    {
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.x + LookBones[i].correctionOffset.x + NodAxis.x * backNodFactor, BaseTransform.right);
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.y + LookBones[i].correctionOffset.y + NodAxis.y * backNodFactor, BaseTransform.up);
                        offset *= Quaternion.AngleAxis(BackBonesAddOffset.z + LookBones[i].correctionOffset.z + NodAxis.z * backNodFactor, BaseTransform.forward);
                    }

                }

                LookBones[i].CalculateMotion(targetLookRotation * offset, WeightsMultiplier, d, finalMotionWeight);
            }

            Quaternion headOff = Quaternion.identity;
            if (LookBones[0].correctionOffset != Vector3.zero || NoddingTransitions != 0f)
            {
                if (ParentalOffsetsV == 2)
                {
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.x + NodAxis.x * nodFactor, LookBones[0].Transform.right);
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.y + NodAxis.y * nodFactor, LookBones[0].Transform.up);
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.z + NodAxis.z * nodFactor, LookBones[0].Transform.forward);
                }
                else
                if (ParentalOffsetsV == 1)
                {
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.x + NodAxis.x * nodFactor, LookBones[0].right);
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.y + NodAxis.y * nodFactor, LookBones[0].up);
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.z + NodAxis.z * nodFactor, LookBones[0].forward);
                }
                else
                {
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.x + NodAxis.x * nodFactor, BaseTransform.right);
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.y + NodAxis.y * nodFactor, BaseTransform.up);
                    headOff *= Quaternion.AngleAxis(LookBones[0].correctionOffset.z + NodAxis.z * nodFactor, BaseTransform.forward);
                }
            }

                LookBones[0].CalculateMotion(targetLookRotation * headOff, WeightsMultiplier, d, finalMotionWeight);
          
        }


    }
}