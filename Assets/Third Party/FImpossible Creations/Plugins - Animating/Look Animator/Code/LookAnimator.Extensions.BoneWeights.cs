using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement feature of spreading rotation weight over backbones (just calculating values of weigt)
    /// </summary>
    public partial class FLookAnimator
    {

        /// <summary>
        /// Calculating rotation weights for "Additional Modules" bones with or without curve
        /// Called every frame or when falloff value changes
        /// </summary>
        private float preWeightFaloff = -1f;
        private float[] baseWeights;
        private float[] targetWeights;


        public void SetAutoWeightsDefault()
        {
            CalculateRotationWeights(FaloffValue);

            if (!BigAngleAutomation)
            {
                for (int i = 1; i < LookBones.Count; i++)
                {
                    LookBones[i].lookWeight = targetWeights[i];
                    LookBones[i].motionWeight = targetWeights[i];
                }
            }
            else
                for (int i = 1; i < LookBones.Count; i++)
            {
                LookBones[i].lookWeight = targetWeights[i];
            }

        }


        public void UpdateAutomationWeights()
        {
            float f = Mathf.InverseLerp(45f, 170f, Mathf.Abs(unclampedLookAngles.y));
            for (int i = 0; i < LookBones.Count; i++)
            {
                LookBones[i].motionWeight = Mathf.LerpUnclamped(LookBones[i].lookWeight, LookBones[i].lookWeightB, f);
            }
        }

        /// <summary>
        /// Refreshing boneMotionWeight variables in backbones to 'lookWeight' value visible inside inspector window by default
        /// </summary>
        public void RefreshBoneMotionWeights()
        {
            for (int i = 1; i < LookBones.Count; i++)
                LookBones[i].motionWeight = LookBones[i].lookWeight;
        }


        public float[] CalculateRotationWeights(float falloff)
        {
            if (LookBones.Count > 1)
            {
                float sum = 0f;
                if (baseWeights == null) baseWeights = new float[LookBones.Count];
                if (baseWeights.Length != LookBones.Count) baseWeights = new float[LookBones.Count];

                if (targetWeights == null) targetWeights = new float[LookBones.Count];
                if (targetWeights.Length != LookBones.Count) targetWeights = new float[LookBones.Count];

                float faloffValue = falloff;

                // When we don't have curve to calculate weight of rotations for bones we calculate them by algorithm
                if (BackBonesFalloff.length < 2 || !CurveSpread)
                {
                    CalculateWeights(baseWeights);

                    for (int i = 0; i < baseWeights.Length; i++) sum += baseWeights[i];

                    float equalVal = 1f / (float)(LookBones.Count - 1);
                    for (int i = 1; i < LookBones.Count; i++)
                    {
                        targetWeights[i] = Mathf.LerpUnclamped(baseWeights[i - 1], equalVal, faloffValue * 1.25f);
                    }
                }
                else // When we have curve we use it to calculate values
                {
                    sum = 0f;
                    float curveDiv = 1f;
                    float prog = 1f / (LookBones.Count - 1);

                    for (int i = 1; i < LookBones.Count; i++)
                    {
                        targetWeights[i] = BackBonesFalloff.Evaluate(prog * i) / curveDiv;
                        sum += targetWeights[i];
                    }

                    for (int i = 1; i < LookBones.Count; i++)
                    {
                        targetWeights[i] /= sum;
                    }
                }

            }

            return targetWeights;
        }


        /// <summary>
        /// Basic weights calcluation
        /// </summary>
        private void CalculateWeights(float[] weights)
        {
            float totalValue = 1f;
            float distributionFactor = 0.75f;

            float varLeft = totalValue;

            weights[0] = totalValue * distributionFactor * 0.65f;
            varLeft -= weights[0];

            for (int i = 1; i < weights.Length - 1; i++)
            {
                float ammountToShare = varLeft / (1f + (1f - distributionFactor)) * distributionFactor;
                weights[i] = ammountToShare;
                varLeft -= ammountToShare;
            }

            weights[weights.Length - 1] = varLeft;
            varLeft = 0;
        }


    }
}