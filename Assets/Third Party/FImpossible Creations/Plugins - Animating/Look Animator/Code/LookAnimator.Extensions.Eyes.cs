using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement feature of simple eyes motion
    /// </summary>
    public partial class FLookAnimator
    {
        public bool UseEyes = false;

        [Tooltip("Target on which eyes will look, set to null if target should be the same as for head target")]
        public Transform EyesTarget;
        [Space(4f)]

        [Tooltip("Eyes transforms / bones (origin should be in center of the sphere")]
        public Transform LeftEye;
        public bool InvertLeftEye = false;
        [Tooltip("Eyes transforms / bones (origin should be in center of the sphere")]
        public Transform RightEye;
        public bool InvertRightEye = false;
        [Tooltip("Look clamping reference rotation transform, mostly parent of eye objects. If nothing is assigned then algorithm will use 'Lead Bone' as reference.")]
        public Transform HeadReference;
        public Transform GetHeadReference() { if (HeadReference != null) return HeadReference; else return LeadBone; }

        public Vector3 EyesOffsetRotation;
        public Vector3 LeftEyeOffsetRotation = Vector3.zero;
        public Vector3 RightEyeOffsetRotation = Vector3.zero;

        [Tooltip("How fast eyes should follow target")]
        [Range(0f, 1f)]
        public float EyesSpeed = 0.5f;

        [FPD_Percentage(0f, 1f)]
        public float EyesBlend = 1f;

        [Tooltip("In what angle eyes should go back to deafult position")]
        [Range(0.0f, 180f)]
        public Vector2 EyesXRange = new Vector2(-60f, 60f);
        public Vector2 EyesYRange = new Vector2(-50f, 50f);

        [Tooltip("If your eyes don't have baked keyframes in animation this value should be enabled, otherwise eyes would go crazy")]
        public bool EyesNoKeyframes = true;

        /// <summary>To make implementation of 'Eyes Animator' more responsible</summary>
        public bool CustomEyesLogics = false;

        private float EyesOutOfRangeBlend = 1f;

        // If you are using also look animator, you can simply uncomment this and one LateUpdate() line for this feature
        //public FLookAnimator UseLookAnimatorTarget = null;

        private Transform[] eyes;
        private Vector3[] eyeForwards;
        private Quaternion[] eyesInitLocalRotations;
        private Quaternion[] eyesLerpRotations;

        private float _eyesBlend;
        private Vector3 headForward;


        public Transform GetEyesTarget()
        {
            if (EyesTarget == null) return GetLookAtTransform(); else return EyesTarget;
        }

        [System.Obsolete("Now please use GetEyesTarget() or GetLookAtTransform() methods")]
        public Transform GetCurrentTarget()
        {
            return GetEyesTarget();
        }


        public Vector3 GetEyesTargetPosition()
        {
            if (EyesTarget == null) return GetLookAtPosition(); else return EyesTarget.position;
        }


        private void InitEyesModule()
        {
            eyes = new Transform[0];

            if (LeftEye != null || RightEye != null)
            {
                if (LeftEye != null && RightEye != null) eyes = new Transform[2] { LeftEye, RightEye }; else if (LeftEye != null) eyes = new Transform[1] { LeftEye }; else eyes = new Transform[1] { RightEye };
            }

            eyeForwards = new Vector3[eyes.Length];
            eyesInitLocalRotations = new Quaternion[eyes.Length];
            eyesLerpRotations = new Quaternion[eyes.Length];

            for (int i = 0; i < eyeForwards.Length; i++)
            {
                Vector3 rootPos = eyes[i].position + Vector3.Scale(BaseTransform.forward, eyes[i].transform.lossyScale);
                Vector3 targetPos = eyes[i].position;

                eyeForwards[i] = (eyes[i].InverseTransformPoint(rootPos) - eyes[i].InverseTransformPoint(targetPos)).normalized;
                eyesInitLocalRotations[i] = eyes[i].localRotation;
                eyesLerpRotations[i] = eyes[i].rotation;
            }

            headForward = Quaternion.FromToRotation(GetHeadReference().InverseTransformDirection(BaseTransform.forward), Vector3.forward) * Vector3.forward;
        }


        private void UpdateEyesLogics()
        {
            if (CustomEyesLogics) return;

            if (EyesNoKeyframes)
                for (int i = 0; i < eyeForwards.Length; i++)
                {
                    eyes[i].localRotation = eyesInitLocalRotations[i];
                }

            Transform eyeTarget = EyesTarget;
            if (eyeTarget == null)
            {
                if (MomentLookTransform != null) eyeTarget = MomentLookTransform; else eyeTarget = ObjectToFollow;
            }

            bool fade = false;
            if (eyeTarget == null) fade = true;
            else
            {
                if (EyesTarget == null)
                    if (LookState != FLookAnimator.EFHeadLookState.ClampedAngle && LookState != EFHeadLookState.Following) fade = true;
            }

            if (fade)
                EyesOutOfRangeBlend = Mathf.Max(0f, EyesOutOfRangeBlend - delta);
            else
                EyesOutOfRangeBlend = Mathf.Min(1f, EyesOutOfRangeBlend + delta);


            _eyesBlend = EyesBlend * EyesOutOfRangeBlend * LookAnimatorAmount;
            if (_eyesBlend <= 0f) return;


            if (eyeTarget != null)
            {
                Vector3 lookStartPosition = GetLookStartMeasurePosition();


                Quaternion lookRotationQuat = Quaternion.LookRotation(eyeTarget.position - lookStartPosition);
                Vector3 lookRotation = lookRotationQuat.eulerAngles;


                #region Limitating rotation

                Vector3 headRotation = (GetHeadReference().rotation * Quaternion.FromToRotation(headForward, Vector3.forward)).eulerAngles;// BaseTransform.rotation.eulerAngles;

                // Vector with degrees differences to all axes
                Vector2 deltaVector = new Vector3(Mathf.DeltaAngle(lookRotation.x, headRotation.x), Mathf.DeltaAngle(lookRotation.y, headRotation.y));

                // Limit when looking up or down
                if (deltaVector.x > EyesYRange.y)
                    lookRotation.x = headRotation.x - EyesYRange.y;
                else if (deltaVector.x < EyesYRange.x)
                    lookRotation.x = headRotation.x - EyesYRange.x;

                // Limit when looking left or right
                if (deltaVector.y > -EyesXRange.x)
                    lookRotation.y = headRotation.y - EyesXRange.y;
                else if (deltaVector.y < -EyesXRange.y)
                    lookRotation.y = headRotation.y + EyesXRange.y;

                #endregion


                for (int i = 0; i < eyes.Length; i++)
                {
                    Quaternion initRot = eyes[i].rotation;
                    Quaternion newEyeRot = Quaternion.Euler(lookRotation);

                    float mul = 1f;
                    if (eyes[i] == LeftEye) { if (InvertLeftEye) mul = -1f; } else if (eyes[i] == RightEye) if (InvertRightEye) mul = -1f;
                    newEyeRot *= Quaternion.FromToRotation(eyeForwards[i], Vector3.forward * mul);
                    newEyeRot *= eyesInitLocalRotations[i];

                    eyes[i].rotation = newEyeRot;
                    eyes[i].rotation *= Quaternion.Inverse(eyesInitLocalRotations[i]);
                    if (EyesOffsetRotation != Vector3.zero) eyes[i].rotation *= Quaternion.Euler(EyesOffsetRotation);
                    if (i == 0) { if (LeftEyeOffsetRotation != Vector3.zero) eyes[i].rotation *= Quaternion.Euler(LeftEyeOffsetRotation); }
                    else if (i ==1) if (RightEyeOffsetRotation != Vector3.zero) eyes[i].rotation *= Quaternion.Euler(RightEyeOffsetRotation);

                    newEyeRot = eyes[i].rotation;

                    eyesLerpRotations[i] = Quaternion.Slerp(eyesLerpRotations[i], newEyeRot, delta * Mathf.Lerp(2f, 40f, EyesSpeed));

                    eyes[i].rotation = Quaternion.Slerp(initRot, eyesLerpRotations[i], _eyesBlend);
                }
            }

        }

    }
}