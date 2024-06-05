#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement painting gizmos in scene view
    /// </summary>
    public partial class FLookAnimator
    {

        #region Getting icon textures

        public static Texture2D _tex_Backbone { get { if (__tbackbone == null) __tbackbone = Resources.Load<Texture2D>("FLookSegment"); return __tbackbone; } }
        private static Texture2D __tbackbone;

        private static Texture2D _tex_Compensbone { get { if (__tcompbone == null) __tcompbone = Resources.Load<Texture2D>("FLookCompensation.png"); return __tcompbone; } }
        private static Texture2D __tcompbone;

        private static Texture2D _tex_Finalbone { get { if (__tfinbone == null) __tfinbone = Resources.Load<Texture2D>("FLookFinalSegment.png"); return __tfinbone; } }
        private static Texture2D __tfinbone;

        private float _gizmosDist = 0.1f;

        public bool _gizmosDrawMaxDist = true;

        #endregion


        /// <summary>
        /// Auto get bones and do some calculations every change in inspector for editor's preview
        /// </summary>
        private void OnValidate()
        {
            _editor_hideEyes = false;
            RefreshLookBones();

            if (!BigAngleAutomation && AutoBackbonesWeights)
                SetAutoWeightsDefault();

            ModelForwardAxis.Normalize();
            ModelUpAxis.Normalize();

            if (Application.isPlaying)
            {
                if (BirdMode)
                {
                    if (!birdModeInitialized)
                    {
                        if (RotationSpeed < 1.7f) RotationSpeed = 2.2f;
                        if (MaxRotationSpeed < 1.7f) MaxRotationSpeed = 2.5f;
                    }

                    InitBirdMode();
                }
            }
            else
            {
                if (BirdMode)
                {
                    if (DelayMaxDistance == 0.25111f)
                        if (LeadBone) if (LeadBone.parent) DelayMaxDistance = Vector3.Distance(LeadBone.position, LeadBone.parent.position);
                }
            }
        }


        [Range(0f, 1f)]
        public float gizmosAlpha = 0.85f;
        public bool drawGizmos = true;


        private float _editor_arrowsAlpha = 2f;


        private void OnDrawGizmos()
        {
            if (BaseTransform == null || LeadBone == null)
            {
                Handles.Label(transform.position, new GUIContent(FGUI_Resources.Tex_Warning, "There is no 'Lead Bone' or 'Base Transform' defined!"));
                return;
            }

            Color gC = Gizmos.color;
            Color hC = Handles.color;
            _gizmosDist = Vector3.Distance(LeadBone.position, BaseTransform.position);

            if (_editor_arrowsAlpha < 2f) _editor_arrowsAlpha += 0.005f;

            if (DebugRays)
            {
                Vector3 lookAtPos;
                if (Application.isPlaying) lookAtPos = smoothLookPosition; else lookAtPos = GetLookAtPosition();

                if (LeadBone)
                {
                    if (BaseTransform)
                    {
                        Gizmos_DrawTargetPos(lookAtPos);
                        Gizmos_DrawClamping(_gizmosDist * 1.495f, lookAtPos);
                        Gizmos_DrawFeatureGuides(_gizmosDist * 1.495f);

                        //Debug.DrawLine(LeadBone.position + StartLookPointOffset, BaseTransform.position, new Color(0.9f, 0.25f, 0.25f, 0.7f));
                    }
                }
            }

            if (gizmosAlpha <= 0f) return;

            if (LeadBone != null)
            {
                if (LeadBone != transform) UnityEditor.Handles.Label(LeadBone.position, _tex_Finalbone);

                Vector3 previousCheckPos = LeadBone ? LeadBone.position : Vector3.zero;
                for (int i = 1; i < LookBones.Count; i++)
                {
                    if (LookBones[i].Transform != null)
                    {
                        if (Vector3.Distance(LookBones[i].Transform.position, previousCheckPos) < 0.175f)
                            continue;
                        else
                            previousCheckPos = LookBones[i].Transform.position;
                    }
                }
            }

            Gizmos.color = gC;
            Handles.color = hC;
        }


        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;

            Color gC = Gizmos.color;
            Color hC = Handles.color;


            if (_gizmosPreForw != ModelForwardAxis || _gizmosPreUp != ModelUpAxis) _editor_arrowsAlpha = 1.5f;

            Gizmos_DrawArrowGuide();

            if (BaseTransform == null || LeadBone == null) return;

            Vector3 lookAtPos;
            if (Application.isPlaying) lookAtPos = smoothLookPosition; else lookAtPos = GetLookAtPosition();

            // Reference scale distance
            _gizmosDist = Vector3.Distance(LeadBone.position, BaseTransform.position);

            if (LeadBone)
            {
                Gizmos_DrawBones();
                Gizmos_DrawCompensation();

                if (UseEyes)
                {
                    if (LeftEye) Gizmos_DrawEyeObj(LeftEye);
                    if (RightEye) Gizmos_DrawEyeObj(RightEye);
                }

                if (BaseTransform) if (!DebugRays) Gizmos_DrawTargetPos(lookAtPos);
            }

            Gizmos_DrawMaxDistance();

            Gizmos_DrawClamping(_gizmosDist * 1.5f, lookAtPos);
            Gizmos_DrawFeatureGuides(_gizmosDist * 1.495f);

            _gizmosPreUp = ModelUpAxis;
            _gizmosPreForw = ModelForwardAxis;

            Gizmos.color = gC;
            Handles.color = hC;
        }


        private void Gizmos_DrawEyeObj(Transform eye)
        {
            Vector3 p = GetHeadReference().position;
            float dist = Vector3.Distance(eye.position, p);
            Handles.color = new Color(.2f, 1f, .7f, gizmosAlpha * 0.5f);
            Handles.DrawDottedLine(eye.position, p, 1f);

            if (DebugRays) Handles.DrawLine(eye.position, Vector3.Lerp(eye.position, GetEyesTargetPosition(), .5f));

            Handles.color = new Color(1f, 1f, 1f, gizmosAlpha * 0.7f);
            Handles.SphereHandleCap(0, eye.position, Quaternion.identity, dist * 0.3f, EventType.Repaint);
            Handles.color = new Color(0f, 0f, 0f, gizmosAlpha * 0.7f);
            Handles.SphereHandleCap(0, eye.position, Quaternion.identity, dist * 0.125f, EventType.Repaint);


        }


        private void Gizmos_DrawTargetPos(Vector3 lookAtPos)
        {
            float d = Vector3.Distance(BaseTransform.position, LeadBone.position);

            //lookStartReferenceTransform = LeadBone;

            Vector3 lookStartPosition = GetLookStartMeasurePosition();

            bool wrong = false;
            if (ObjectToFollow == null && FollowMode != EFFollowMode.FollowJustPosition) wrong = true;

            Handles.color = new Color(.9f, 0.9f, 0.9f, 0.8f);
            Handles.matrix = Matrix4x4.TRS(lookStartPosition, Quaternion.identity, new Vector3(1f, .5f, 1f));
            Handles.DrawWireDisc(Vector3.zero, (lookAtPos - lookStartPosition), d * 0.045f);
            Handles.matrix = Matrix4x4.identity;
            Handles.DrawSolidDisc(lookStartPosition, (lookAtPos - lookStartPosition), d * 0.0155f);


            if (!wrong)
                Handles.color = new Color(0f, 0.7f, 0.3f, 0.9f);
            else
                Handles.color = new Color(0.7f, 0.0f, 0.3f, 0.9f);

            Handles.DrawDottedLine(lookStartPosition, lookAtPos, 3f);

            Handles.DrawLine(lookAtPos - BaseTransform.forward * d * 0.1f, lookAtPos + BaseTransform.forward * d * 0.1f);
            Handles.DrawLine(lookAtPos - BaseTransform.right * d * 0.1f, lookAtPos + BaseTransform.right * d * 0.1f);
            Handles.DrawLine(lookAtPos - BaseTransform.up * d * 0.1f, lookAtPos + BaseTransform.up * d * 0.1f);
            Handles.SphereHandleCap(0, lookAtPos, Quaternion.identity, d * 0.02f, EventType.Repaint);


            if (Application.isPlaying)
            {
                Handles.color = new Color(1f, 1f, 1f, 0.2f);
                Handles.SphereHandleCap(0, finalLookPosition, Quaternion.identity, d * 0.015f, EventType.Repaint);
            }

            if (BirdMode)
            {
                Handles.color = new Color(0.1f, 1f, 0.9f, gizmosAlpha * 0.5f);

                lookAtPos = finalLookPosition;

                Handles.DrawDottedLine(lookStartPosition, lookAtPos, 3f);

                Handles.DrawLine(lookAtPos - BaseTransform.forward * d * 0.1f, lookAtPos + BaseTransform.forward * d * 0.1f);
                Handles.DrawLine(lookAtPos - BaseTransform.right * d * 0.1f, lookAtPos + BaseTransform.right * d * 0.1f);
                Handles.DrawLine(lookAtPos - BaseTransform.up * d * 0.1f, lookAtPos + BaseTransform.up * d * 0.1f);
                Handles.SphereHandleCap(0, lookAtPos, Quaternion.identity, d * 0.02f, EventType.Repaint);
            }

        }


        private void Gizmos_DrawMaxDistance()
        {
            if (MaximumDistance <= 0f) return;
            if (!_gizmosDrawMaxDist) return;

            float a = 0.525f;

            if (Distance2D)
            {
                if (LookState == EFHeadLookState.OutOfMaxDistance)
                    Handles.color = new Color(1f, .1f, .1f, a);
                else
                    Handles.color = new Color(0.02f, .65f, 0.2f, a);

                Handles.DrawWireDisc(GetDistanceMeasurePosition(), ModelUpAxis, MaximumDistance);

                if (DistanceMeasurePoint != Vector3.zero)
                {
                    Gizmos.color = new Color(0.02f, .65f, 0.2f, a);
                    Gizmos.DrawLine(GetDistanceMeasurePosition() - Vector3.right * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor), GetDistanceMeasurePosition() + Vector3.right * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor));
                    Gizmos.DrawLine(GetDistanceMeasurePosition() - ModelForwardAxis.normalized * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor), GetDistanceMeasurePosition() + Vector3.forward * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor));
                }

                if (MaxOutDistanceFactor > 0f)
                {
                    Handles.color = new Color(.835f, .135f, .08f, a);
                    Handles.DrawWireDisc(GetDistanceMeasurePosition(), ModelUpAxis, MaximumDistance + MaximumDistance * MaxOutDistanceFactor);
                }
            }
            else
            {
                if (LookState == EFHeadLookState.OutOfMaxDistance)
                    Gizmos.color = new Color(1f, .1f, .1f, a);
                else
                    Gizmos.color = new Color(0.02f, .65f, 0.2f, a);

                Gizmos.DrawWireSphere(GetDistanceMeasurePosition(), MaximumDistance);

                if (MaxOutDistanceFactor > 0f)
                {
                    Gizmos.color = new Color(.835f, .135f, .08f, a);
                    Gizmos.DrawWireSphere(GetDistanceMeasurePosition(), MaximumDistance + MaximumDistance * MaxOutDistanceFactor);

                }

                if (DistanceMeasurePoint != Vector3.zero)
                {
                    Gizmos.color = new Color(0.02f, .65f, 0.2f, a);
                    Gizmos.DrawLine(GetDistanceMeasurePosition() - Vector3.right * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor), GetDistanceMeasurePosition() + Vector3.right * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor));
                    Gizmos.DrawLine(GetDistanceMeasurePosition() - ModelForwardAxis.normalized * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor), GetDistanceMeasurePosition() + Vector3.forward * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor));
                }
            }
        }


        private Vector3 _gizmosPreForw = Vector3.zero;
        private Vector3 _gizmosPreUp = Vector3.zero;
        private Vector2 _gizmosLastHorizClamp;
        private float _gizmosHorizAlpha = 0f;
        private float _gizmosVertAlpha = 0f;
        private Vector2 _gizmosLastVertClamp;
        [HideInInspector]
        public bool _gizmosDrawingGuides = true;
        private void Gizmos_DrawArrowGuide()
        {
            if (BaseTransform == null) return;
            if (LeadBone == null) return;

            if (_editor_arrowsAlpha > 0f)
            {
                float d = Vector3.Distance(LeadBone.position, BaseTransform.position);
                Vector3 arrowStart = Vector3.Lerp(BaseTransform.position, LeadBone.position, 0.7f);

                Handles.color = new Color(0.05f, 0.225f, 1f, 0.9f * _editor_arrowsAlpha);
                FGUI_Handles.DrawArrow(BaseTransform.TransformDirection(ModelForwardAxis) * d * .22f + arrowStart, Quaternion.LookRotation(BaseTransform.TransformDirection(ModelForwardAxis), BaseTransform.TransformDirection(ModelUpAxis)), d * 0.2f);

                Handles.color = new Color(0.05f, 0.8f, 0.05f, 0.75f * _editor_arrowsAlpha);
                arrowStart = LeadBone.position + BaseTransform.TransformDirection(ModelUpAxis) * d * .285f;
                FGUI_Handles.DrawArrow(arrowStart, Quaternion.LookRotation(BaseTransform.TransformDirection(ModelUpAxis), BaseTransform.TransformDirection(ModelForwardAxis)), d * 0.15f, 4f, 0.5f);
            }

            if (_editor_arrowsAlpha > -0.1f) _editor_arrowsAlpha -= 0.0125f;
        }


        private void Gizmos_DrawBones()
        {
            if (LeadBone == null) return;

            Color c = Handles.color;
            Color boneColor = new Color(0.075f, .85f, 0.3f, gizmosAlpha * 0.7f);
            Handles.color = boneColor;

            Vector3 f = BaseTransform.TransformDirection(ModelForwardAxis);

            if (LeadBone.childCount > 0)
                FGUI_Handles.DrawBoneHandle(LeadBone.position, LeadBone.position + BaseTransform.TransformDirection(ModelUpAxis) * Vector3.Distance(LeadBone.position, LeadBone.parent.position) / 2f, f);

            if (LookBones.Count > 0)
            {
                // Drawing back-bones
                for (int i = 1; i < LookBones.Count; i++)
                {
                    if (LookBones[i].Transform != null)
                    {
                        if (LookBones[i] == null) continue;
                        FGUI_Handles.DrawBoneHandle(LookBones[i].Transform.position, LookBones[i - 1].Transform.position, f);
                    }
                }

                if (LookBones.Count > 1)
                    FGUI_Handles.DrawBoneHandle(LookBones[1].Transform.position, LeadBone.position, f);
            }

            Handles.SphereHandleCap(0, LeadBone.position, Quaternion.identity, _gizmosDist * 0.025f, EventType.Repaint);

            for (int i = 0; i < LeadBone.childCount; i++)
            {
                Handles.color = new Color(0.8f, .8f, 0.8f, gizmosAlpha * 0.25f);
                Handles.DrawDottedLine(LeadBone.position, LeadBone.GetChild(i).position, 2.5f);
                Handles.color = new Color(0.8f, .8f, 0.8f, gizmosAlpha * 0.1f);
                Handles.SphereHandleCap(0, LeadBone.GetChild(i).position, Quaternion.identity, _gizmosDist * 0.01f, EventType.Repaint);
            }

            if (LookBones.Count > 1)
            {
                if (LookBones[LookBones.Count - 1].Transform.parent)
                {
                    Handles.color = new Color(0.8f, .8f, 0.8f, gizmosAlpha * 0.25f);
                    Handles.DrawDottedLine(LookBones[LookBones.Count - 1].Transform.position, LookBones[LookBones.Count - 1].Transform.parent.position, 3f);
                    Handles.color = new Color(0.8f, .8f, 0.8f, gizmosAlpha * 0.1f);
                    Handles.SphereHandleCap(0, LookBones[LookBones.Count - 1].Transform.parent.position, Quaternion.identity, _gizmosDist * 0.01f, EventType.Repaint);
                }
            }

            Handles.color = c;
        }


        private void Gizmos_DrawClamping(float radius, Vector3 lookPos)
        {
            if (_Editor_Category != EEditorLookCategory.Limit && DebugRays == false) return;

            Handles.matrix = BaseTransform.localToWorldMatrix;

            Vector3 startLook = GetLookStartMeasurePosition();
            Vector3 startLookLocal = BaseTransform.InverseTransformPoint(startLook);
            Vector3 dir = (lookPos - startLook).normalized;

            if (LookState == EFHeadLookState.ClampedAngle)
                Handles.color = new Color(1f, 1f, 0.1f, 0.7f);
            else
                Handles.color = new Color(0.3f, 1f, 0.1f, 0.7f);

            //Gizmos.DrawLine(startLook, startLook + dir);

            Vector3 axisDir = BaseTransform.InverseTransformDirection(dir);
            axisDir.y = 0; axisDir.Normalize();
            Handles.DrawLine(startLookLocal, startLookLocal + axisDir * radius);

            if (YRotationLimits != _gizmosLastVertClamp) _gizmosVertAlpha = 1.5f;

            // Vertical
            Handles.color = new Color(0.3f, 1f, 0.7f, 0.08f * Mathf.Min(_gizmosVertAlpha, 1f));
            Handles.DrawSolidArc(startLookLocal, Vector3.right, ModelForwardAxis, YRotationLimits.y, radius / 1.2f);
            Handles.DrawSolidArc(startLookLocal, Vector3.right, ModelForwardAxis, YRotationLimits.x, radius / 1.2f);

            Handles.color = new Color(0.22f, .8f, 0.5f, 0.75f);
            Handles.DrawWireArc(startLookLocal, Vector3.right, ModelForwardAxis, YRotationLimits.y, radius / 1.2f);
            Handles.DrawWireArc(startLookLocal, Vector3.right, ModelForwardAxis, YRotationLimits.x, radius / 1.2f);

            if (StartLookElasticRangeY > 0f)
            {
                Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.04f);
                Handles.DrawSolidArc(startLookLocal, Vector3.right, ModelForwardAxis, StartLookElasticRangeY, radius / 3.2f);
                Handles.DrawSolidArc(startLookLocal, Vector3.right, ModelForwardAxis, -StartLookElasticRangeY, radius / 3.2f);
            }

            if (XRotationLimits != _gizmosLastHorizClamp) _gizmosHorizAlpha = 1.5f;

            // Horizontal 
            Handles.color = new Color(0.3f, 1f, 0.1f, 0.08f * Mathf.Min(_gizmosHorizAlpha, 1f));
            Handles.DrawSolidArc(startLookLocal, ModelUpAxis, ModelForwardAxis, XRotationLimits.x, radius);
            Handles.DrawSolidArc(startLookLocal, ModelUpAxis, ModelForwardAxis, XRotationLimits.y, radius);

            Handles.color = new Color(0.22f, .8f, 0.08f, 0.75f);
            Handles.DrawWireArc(startLookLocal, ModelUpAxis, ModelForwardAxis, XRotationLimits.x, radius);
            Handles.DrawWireArc(startLookLocal, ModelUpAxis, ModelForwardAxis, XRotationLimits.y, radius);

            if (StartLookElasticRangeX > 0f)
            {
                Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.04f);
                Handles.DrawSolidArc(startLookLocal, ModelUpAxis, ModelForwardAxis, StartLookElasticRangeX, radius / 3.2f);
                Handles.DrawSolidArc(startLookLocal, ModelUpAxis, ModelForwardAxis, -StartLookElasticRangeX, radius / 3.2f);
            }


            if (LookState == EFHeadLookState.ClampedAngle)
                Handles.color = new Color(1f, 1f, 0.1f, 0.7f);
            else
                Handles.color = new Color(0.3f, 1f, 0.7f, 0.7f);

            axisDir = BaseTransform.InverseTransformDirection(dir);
            axisDir.x = 0; axisDir.z = Mathf.Abs(axisDir.z); axisDir.Normalize();
            Handles.DrawLine(startLookLocal, startLookLocal + axisDir * radius / 1.2f);


            _gizmosLastHorizClamp = XRotationLimits;
            _gizmosLastVertClamp = YRotationLimits;

            _gizmosVertAlpha -= 0.03f;
            _gizmosHorizAlpha -= 0.03f;

            Handles.matrix = Matrix4x4.identity;
        }


        private void Gizmos_DrawFeatureGuides(float radius)
        {
            if (!_gizmosDrawingGuides && DebugRays == false) return;

            Handles.matrix = BaseTransform.localToWorldMatrix;
            //Vector3 startLook = GetLookStartMeasurePosition();
            //Vector3 startLookLocal = BaseTransform.InverseTransformPoint(startLook);
            Vector3 startLookLocal = Vector3.zero;

            // Hold back range
            Handles.color = new Color(.4f, .4f, .4f, 0.04f);
            Handles.DrawSolidArc(startLookLocal, ModelUpAxis, -ModelForwardAxis, HoldRotateToOppositeUntil, radius / 1.5f);
            Handles.DrawSolidArc(startLookLocal, ModelUpAxis, -ModelForwardAxis, -HoldRotateToOppositeUntil, radius / 1.5f);


            // Start look range
            Handles.color = new Color(.1f, .1f, .1f, 0.04f);
            Handles.DrawSolidArc(startLookLocal, ModelUpAxis, ModelForwardAxis, LookWhenAbove, radius / 1.5f);
            Handles.DrawSolidArc(startLookLocal, ModelUpAxis, ModelForwardAxis, -LookWhenAbove, radius / 1.5f);


            // Stop looking range
            if (StopLookingAbove < 180)
            {
                if (LookState == EFHeadLookState.OutOfMaxRotation)
                    Handles.color = new Color(1f, .3f, .3f, 0.5f);
                else
                    Handles.color = new Color(.8f, .8f, .8f, 0.5f);

                Handles.DrawWireArc(Vector3.zero, ModelUpAxis, ModelForwardAxis, StopLookingAbove, radius / 1.5f);
                Handles.DrawWireArc(Vector3.zero, ModelUpAxis, ModelForwardAxis, -StopLookingAbove, radius / 1.5f);

                if (LookState == EFHeadLookState.OutOfMaxRotation)
                    Handles.color = new Color(1f, .3f, .3f, 0.05f);
                else
                    Handles.color = new Color(.8f, .8f, .8f, 0.05f);

                Handles.DrawSolidArc(Vector3.zero, ModelUpAxis, ModelForwardAxis, StopLookingAbove, radius / 1.5f);
                Handles.DrawSolidArc(Vector3.zero, ModelUpAxis, ModelForwardAxis, -StopLookingAbove, radius / 1.5f);
            }

            Handles.matrix = Matrix4x4.identity;
        }


        private void Gizmos_DrawCompensation()
        {
            if (CompensationBones.Count <= 0) return;

            // Drawing compensation bones
            Handles.color = new Color(0.1f, 1f, 0.9f, gizmosAlpha * 0.5f);
            for (int i = 0; i < CompensationBones.Count; i++)
                if (CompensationBones[i] != null)
                {
                    if (CompensationBones[i].Transform == null) continue;

                    Handles.SphereHandleCap(0, CompensationBones[i].Transform.position, Quaternion.identity, _gizmosDist * 0.0125f, EventType.Repaint);
                    if (CompensationBones[i].Transform.childCount > 0)
                    {
                        Handles.SphereHandleCap(0, CompensationBones[i].Transform.GetChild(0).position, Quaternion.identity, _gizmosDist * 0.0125f, EventType.Repaint);
                        Handles.DrawDottedLine(CompensationBones[i].Transform.position, CompensationBones[i].Transform.GetChild(0).position, 1f);
                    }
                }
        }


    }
}

#endif
