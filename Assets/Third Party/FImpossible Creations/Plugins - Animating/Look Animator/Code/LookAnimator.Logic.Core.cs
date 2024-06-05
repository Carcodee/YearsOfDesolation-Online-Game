using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement core look direction calculations for Look Animator to work universally with different rigs
    /// </summary>
    public partial class FLookAnimator
    {
        /// <summary> We can leave head looking in last position when loosing target </summary>
#pragma warning disable 0414
        private Vector3 lookFreezeFocusPoint;
        /// <summary> Smoothing focus position for more mildness in motion </summary>
        private Vector3 smoothLookPosition = Vector3.zero;
        private Vector3 _velo_smoothLookPosition = Vector3.zero;
        /// <summary> Final look position coordinates used by look animator, needed for bird mode </summary>
        private Vector3 finalLookPosition = Vector3.zero;

        private bool usingAxisCorrection;
        private Matrix4x4 axisCorrectionMatrix;

        /// <summary> Delta time value for all calculations which needs it </summary>
        private float delta;


        /// <summary>
        /// Main head look rotation calculations logics execution
        /// </summary>
        private void CalculateLookAnimation()
        {
            // Begin check for stop lokoing
            _stopLooking = false;

            // Stopping looking when object to follow is null
            if (FollowMode != EFFollowMode.FollowJustPosition) if (ObjectToFollow == null) if (MomentLookTransform == null) _stopLooking = true;


            Vector3 lookRotation;

            LookPositionUpdate();
            LookWhenAboveGoBackCalculations();

            // When stopping look, we don't want head to goo too fast onto forward look pose
            if (_stopLooking) finalLookPosition = transform.TransformPoint(lookFreezeFocusPoint);
            else
            {
                if (!BirdMode)
                    finalLookPosition = smoothLookPosition;
                else
                    finalLookPosition = BirdTargetPosition;
            }


            if (FixingPreset != EFAxisFixOrder.Parental)
            {
                #region Calculating target rotation and angles

                // If our target is out of max distance let's look at default direction
                if (LookState == EFHeadLookState.OutOfMaxDistance)
                    targetLookAngles = Vector3.MoveTowards(targetLookAngles, Vector3.zero, 1f + RotationSpeed);
                else
                {
                    // Direction towards target in different spaces
                    Vector3 worldDirectionAndTargetAngles = (finalLookPosition - GetLookStartMeasurePosition()).normalized;

                    // Supporting different axis orientation using matrix
                    if (usingAxisCorrection)
                    {
                        worldDirectionAndTargetAngles = axisCorrectionMatrix.inverse.MultiplyVector(worldDirectionAndTargetAngles).normalized;
                        worldDirectionAndTargetAngles = WrapVector(Quaternion.LookRotation(worldDirectionAndTargetAngles, axisCorrectionMatrix.MultiplyVector(ModelUpAxis).normalized).eulerAngles);
                    }
                    else // Easier calculations when using standard z - forward - y up
                    {
                        worldDirectionAndTargetAngles = BaseTransform.InverseTransformDirection(worldDirectionAndTargetAngles);
                        worldDirectionAndTargetAngles = WrapVector(Quaternion.LookRotation(worldDirectionAndTargetAngles, BaseTransform.TransformDirection(ModelUpAxis)).eulerAngles);
                    }

                    targetLookAngles = worldDirectionAndTargetAngles;
                }

                Vector2 angles = targetLookAngles;
                angles = LimitAnglesCalculations(angles);
                AnimateAnglesTowards(angles);

                #endregion

                // Character rotation offset
                if (usingAxisCorrection)
                {
                    Quaternion fromto = Quaternion.FromToRotation(Vector3.right, Vector3.Cross(Vector3.up, ModelForwardAxis));
                    fromto = Quaternion.Euler(finalLookAngles) * fromto * BaseTransform.rotation;
                    lookRotation = fromto.eulerAngles;
                }
                else
                    lookRotation = finalLookAngles + BaseTransform.eulerAngles;

                // Additional operations
                lookRotation += RotationOffset;
                lookRotation = ConvertFlippedAxes(lookRotation);
            }
            else // Universal correction method
            {
                lookRotation = LookRotationParental((finalLookPosition - GetLookStartMeasurePosition()).normalized).eulerAngles;
            }

            if (!_stopLooking) lookFreezeFocusPoint = BaseTransform.InverseTransformPoint(finalLookPosition);
            targetLookRotation = Quaternion.Euler(lookRotation);

            SetTargetBonesRotations();
        }


        /// <summary>
        /// Animating LookBones rotation values to desired ones
        /// </summary>
        private void SetTargetBonesRotations()
        {
            if (FixingPreset == EFAxisFixOrder.Parental)
            {
                if (UltraSmoother <= 0f)
                    AnimateBonesParental(1f);
                else
                    AnimateBonesParental(delta * Mathf.Lerp(21f, 3f, UltraSmoother));
            }
            else
            {
                Quaternion backTarget = targetLookRotation * Quaternion.Euler(BackBonesAddOffset);
                Quaternion diffOnMain = BaseTransform.rotation * Quaternion.Inverse(rootStaticRotation);

                if (UseBoneOffsetRotation) // Animator synced
                {
                    if (UltraSmoother <= 0f)
                        AnimateBonesSynced(diffOnMain, backTarget, 1f);
                    else
                        AnimateBonesSynced(diffOnMain, backTarget, delta * Mathf.Lerp(21f, 3f, UltraSmoother));
                }
                else // Hard target rotations
                {
                    if (UltraSmoother <= 0f)
                        AnimateBonesUnsynced(diffOnMain, backTarget, 1f);
                    else
                        AnimateBonesUnsynced(diffOnMain, backTarget, delta * Mathf.Lerp(21f, 3f, UltraSmoother));
                }
            }

        }


        /// <summary>
        /// Making y position of look target position a little offseted, when we look at something we don't look at it totally directly, we also using eyes direction to see things fully - this thing makes animation little more realistic
        /// </summary>
        private Quaternion LookRotationParental(Vector3 direction)
        {
            // Refreshing reference pose on bones if model is not animated (it's done anyway by Unity Animator if we use it)
            if (!SyncWithAnimator)
                for (int i = 0; i < LookBones.Count; i++) LookBones[i].Transform.localRotation = LookBones[i].localStaticRotation;

            if (ParentalReferenceBone == null)
                _parentalBackParentRot = LeadBone.parent.rotation;
            else
                _parentalBackParentRot = ParentalReferenceBone.rotation;

            // Target look rotation equivalent for LeadBone's parent
            Vector3 lookDirectionParent = Quaternion.Inverse(_parentalBackParentRot) * (direction).normalized;

            // Getting angle offset in y axis - horizontal rotation
            _parentalAngles.y = AngleAroundAxis(parentalReferenceLookForward, lookDirectionParent, parentalReferenceUp);

            Vector3 targetRight = Vector3.Cross(parentalReferenceUp, lookDirectionParent);
            Vector3 horizontalPlaneTarget = lookDirectionParent - Vector3.Project(lookDirectionParent, parentalReferenceUp);

            _parentalAngles.x = AngleAroundAxis(horizontalPlaneTarget, lookDirectionParent, targetRight);


            _parentalAngles = LimitAnglesCalculations(_parentalAngles);
            _parentalAngles = AnimateAnglesTowards(_parentalAngles);

            Vector3 referenceRightDir = Vector3.Cross(parentalReferenceUp, parentalReferenceLookForward);

            if (NoddingTransitions != 0f)
            {
                float nodAmount = nodValue * nodPower * 40f;
                _parentalAngles.x += nodAmount * BackBonesNod;
            }

            if (RotationOffset != Vector3.zero)
            {
                _parentalAngles += new Vector2(RotationOffset.x, RotationOffset.y);
            }

            return ParentalRotationMaths(referenceRightDir, _parentalAngles.x, _parentalAngles.y);
        }

        [Tooltip("If your neck bones are rotated in a wrong way, you can try putting here parent game object of last back bone in chain")]
        public Transform ParentalReferenceBone;
        private Quaternion _parentalBackParentRot;
        private Vector2 _parentalAngles = Vector2.zero;

        private Quaternion ParentalRotationMaths(Vector3 referenceRightDir, float xAngle, float yAngle)
        {
            // With calculated angles we can get rotation by rotating around desired axes
            Vector3 lookDirectionParent = Quaternion.AngleAxis(yAngle, parentalReferenceUp) * Quaternion.AngleAxis(xAngle, referenceRightDir) * parentalReferenceLookForward;

            // Making look and up direction perpendicular
            Vector3 upDirGoal = parentalReferenceUp;
            Vector3.OrthoNormalize(ref lookDirectionParent, ref upDirGoal);

            // Look and up directions in lead's parent space
            Vector3 lookDir = lookDirectionParent;
            DynamicReferenceUp = upDirGoal;
            Vector3.OrthoNormalize(ref lookDir, ref DynamicReferenceUp);

            // Finally getting look rotation
            Quaternion lookRot = _parentalBackParentRot * Quaternion.LookRotation(lookDir, DynamicReferenceUp);
            lookRot *= Quaternion.Inverse(_parentalBackParentRot * Quaternion.LookRotation(parentalReferenceLookForward, parentalReferenceUp));

            return lookRot;
        }


        /// <summary>
        /// Correction matrix for axis correction calculations
        /// </summary>
        private void UpdateCorrectionMatrix()
        {
            // Supporting different axis orientation using matrix
            if (ModelUpAxis != Vector3.up || ModelForwardAxis != Vector3.forward)
            {
                usingAxisCorrection = true;
                axisCorrectionMatrix = Matrix4x4.TRS(BaseTransform.position, Quaternion.LookRotation(BaseTransform.TransformDirection(ModelForwardAxis), BaseTransform.TransformDirection(ModelUpAxis)), BaseTransform.lossyScale);
            }
            else // Easier calculations when using standard z - forward - y up
                usingAxisCorrection = false;
        }

    }
}