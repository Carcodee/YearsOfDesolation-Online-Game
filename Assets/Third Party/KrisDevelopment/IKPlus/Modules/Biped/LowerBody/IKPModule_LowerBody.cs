using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using TransformData = SETUtil.Types.TransformData;
using SETUtil.Extend;
using UnityEngine.Serialization;
using System.Linq;

namespace IKPn
{
    [IKPModule(ModuleSignatures.LOWER_HUMANOID, displayName = "Lower Humanoid", inspectorOrder = 20, updateOrder = 10, iconPath = "IKP/ikp_lower_icon")]
    [AddComponentMenu(IKPUtils.MODULE_COMPONENT_MENU + "/Lower Body")]
    [DisallowMultipleComponent]
    public partial class IKPModule_LowerBody : ModuleBase_StandardBoneLogicSSP
    {

        internal class TargetProcessResult
        {
            public TransformData targetTrDt;
            public bool legClips;

            public Vector3 unweightedTargetPosition; // used by drop

            // If any grounder drop is applied, record it here.
            // For example, feet IK needs to correct for the drop when raycasting.
            public Vector3 dropFoot = Vector2.zero;
            public Vector3? predictionRecordFoot = null;
        }


        public override int OrderIndex()
        {
            return IKP.ORDER_INDEX + ModuleManager.GetInspectorOrder(ModuleSignatures.LOWER_HUMANOID);
        }

        public enum Property
        {
            GeneralWeight,
            LeftLegWeight,
            RightLegWeight,
            LeftKneeRotation,
            LeftFootRotation,
            RightKneeRotation,
            RightFootRotation,
            GrounderWeight,
            GrounderReach,
            LegSmoothing,
            FeetSmoothing,
            GrounderFallSpeed,
            GrounderReactSpeed,
        }

        public enum LegRaycasting
        {
            FromThighs,
            VerticallyAligned,
        }

        //property names cache
        /// <summary>
        /// Cached property index
        /// </summary>
        public static int
            p_generalWeight = (int)Property.GeneralWeight,
            p_leftLegWeight = (int)Property.LeftLegWeight,
            p_rightLegWeight = (int)Property.RightLegWeight,
            p_leftKneeRotation = (int)Property.LeftKneeRotation,
            p_leftFootRotation = (int)Property.LeftFootRotation,
            p_rightKneeRotation = (int)Property.RightKneeRotation,
            p_rightFootRotation = (int)Property.RightFootRotation,
            p_grounderWeight = (int)Property.GrounderWeight,
            p_grounderReach = (int)Property.GrounderReach,
            p_grounderFallSpeed = (int)Property.GrounderFallSpeed,
            p_grounderReactSpeed = (int)Property.GrounderReactSpeed,
            p_legSmoothing = (int)Property.LegSmoothing,
            p_feetSmoothing = (int)Property.FeetSmoothing;
        //---

        //public:
        [SerializeField][FormerlySerializedAs("_boneSetup")] private LowerBodySetup bodySetup;

        [HideInInspector]
        public bool
            forcedPositioning = true,
            feetIK = true,
            legRaycasting = true; // prevent clipping
        [SerializeField] private GrounderType grounderBehavior = GrounderType.None; //replaces Grounder Module
        [HideInInspector] public LegRaycasting legRaycastingMode;

        [HideInInspector]
        [Range(0, .9f)]
        public float 
            feetOffset = 0.05f,
            toeHeight = 0.05f;

        [HideInInspector]
        public float
            feetLength = 0.14f;

        [HideInInspector]
        [SerializeField]
        internal BilateralBase bilatBase = new BilateralBase();

        //private:
        [HideInInspector]
        [SerializeField]
        private Vector3 initialHipsV3off = Vector3.zero;

        [NonSerialized]
        private string[] propertyNames;

        private Vector3 dropVector = -Vector3.up;

        private Quaternion
            leftFootSmoothRt,
            rightFootSmoothRt;

        private LayerMask raycastMask => ikp.raycastingMask;

        private TransformData
            hipsPrePose = new TransformData(),
            leftFootPrePose = new TransformData(),
            rightFootPrePose = new TransformData(),
            leftTargetTrDt = new TransformData(),
            rightTargetTrDt = new TransformData();

        private Vector3
            leftSmoothTarget = new Vector3(),
            rightSmoothTarget = new Vector3();

        private Dictionary<Transform, TransformData> preIKBonePositions = new Dictionary<Transform, TransformData>();
        private Grounder grounder = null;


#if UNITY_EDITOR
        SerializedProperty
            m_so_blb_hasLeft,
            m_so_blb_hasRight,
            m_so_l_feetIK,
            m_so_l_legRaycasting,
            m_so_l_grounderBehavior,
            m_so_l_legRaycastingMode;
        SerializedProperty m_so_boneSetup;
#endif


        public override ExecutionFlag IKPPreUpdate()
        {
            // when no animation is playing, there's noone to reset the bones
            if (preIKBonePositions.Count > 0)
            {
                RestorePreIKBoneStates();
            }
            return ExecutionFlag.Continue;
        }

        public override ExecutionFlag IKPUpdate()
        {
            if (base.IKPUpdate() == ExecutionFlag.Break)
            {
                return ExecutionFlag.Break;
            }

            PrePose();

            //process feet targets (for later use inside CalculateDrop()): 
            TargetProcessResult _leftTarget;
            if (bilatBase.hasLeft)
            {
                _leftTarget = ProcessTarget(Side.Left);
                leftTargetTrDt = _leftTarget.targetTrDt;
            }
            else
            {
                _leftTarget = new TargetProcessResult();
            }

            TargetProcessResult _rightTarget;
            if (bilatBase.hasRight)
            {
                _rightTarget = ProcessTarget(Side.Right);
                rightTargetTrDt = _rightTarget.targetTrDt;
            }
            else
            {
                _rightTarget = new TargetProcessResult();
            }

            //hips orientation
            IKPLocalSpace _referenceOrientation = new IKPLocalSpace(bodySetup.hips.rotation * Quaternion.Inverse(bodySetup.hipsRotationOffset));

            if (!Grounder.IsUsingSelected(grounder, grounderBehavior))
            {
                switch (grounderBehavior)
                {
                    case GrounderType.StandardGrounder:
                        grounder = new StandardGrounder(this);
                        break;
                    case GrounderType.SimpleGrounder:
                        grounder = new SimpleGrounder(this);
                        break;
                    case GrounderType.None:
                        grounder = null;
                        break;
                }
            }

            //determine drop vector:
            dropVector = -_referenceOrientation.up;
            grounder?.DoBeforeLegs(_leftTarget, _rightTarget);

            // legs calc
            if (bilatBase.hasLeft)
                CalculateLeg(Side.Left,
                    _referenceOrientation,
                    ref leftSmoothTarget,
                    ref leftFootSmoothRt,
                    _leftTarget);

            if (bilatBase.hasRight)
                CalculateLeg(Side.Right,
                    _referenceOrientation,
                    ref rightSmoothTarget,
                    ref rightFootSmoothRt,
                    _rightTarget);


            //offset hips
            grounder?.DoAfterLegs();

            return ExecutionFlag.Continue;
        }

        public override bool Validate(List<ValidationResult> list)
        {
            // draw some errors
            if (bodySetup != null)
            {
                ValidateCriticalBodySetupBone(bodySetup.hips, nameof(bodySetup.hips), list);
                if (bilatBase.hasLeft)
                {
                    ValidateCriticalBodySetupBone(bodySetup.leftFoot, nameof(bodySetup.leftFoot), list);
                    ValidateCriticalBodySetupBone(bodySetup.leftKnee, nameof(bodySetup.leftKnee), list);
                    ValidateCriticalBodySetupBone(bodySetup.leftThigh, nameof(bodySetup.leftThigh), list);
                }
                if (bilatBase.hasRight)
                {
                    ValidateCriticalBodySetupBone(bodySetup.rightFoot, nameof(bodySetup.rightFoot), list);
                    ValidateCriticalBodySetupBone(bodySetup.rightKnee, nameof(bodySetup.rightKnee), list);
                    ValidateCriticalBodySetupBone(bodySetup.rightThigh, nameof(bodySetup.rightThigh), list);
                }
            }
            if (GetProperty(p_feetSmoothing) <= 0)
            {
                list.Add(new ValidationResult()
                {
                    message = "Foot speed is 0",
                    outcome = ValidationResult.Outcome.Warning,
                });
            }
            if (GetProperty(p_legSmoothing) <= 0)
            {
                list.Add(new ValidationResult()
                {
                    message = "Leg speed is 0",
                    outcome = ValidationResult.Outcome.Warning,
                });
            }
            if (GetProperty(p_grounderReactSpeed) <= 0 && grounderBehavior != GrounderType.None)
            {
                list.Add(new ValidationResult()
                {
                    message = "Grounder react speed is 0",
                    outcome = ValidationResult.Outcome.Warning,
                });
            }
            if (GetProperty(p_grounderFallSpeed) <= 0 && grounderBehavior != GrounderType.None && legRaycasting)
            {
                list.Add(new ValidationResult()
                {
                    message = "Grounder fall speed is 0",
                    outcome = ValidationResult.Outcome.Warning,
                });
            }
            return base.Validate(list);
        }

#if UNITY_EDITOR
        public override void DrawEditorGUI()
        {
            base.DrawEditorGUI();

            //draw target settings:
            if (bilatBase.hasLeft)
                IKPEditorUtils.DrawTargetGUI(serialized.FindProperty($"{nameof(bilatBase)}.{BilateralBase.PROPERTY_NAME_LEFT_IKP_TARGET}"), "Left Leg Target");
            if (bilatBase.hasRight)
                IKPEditorUtils.DrawTargetGUI(serialized.FindProperty($"{nameof(bilatBase)}.{BilateralBase.PROPERTY_NAME_RIGHT_IKP_TARGET}"), "Right Leg Target");
        }

        protected override void DrawSetup()
        {
            base.DrawSetup();

            GUILayout.BeginHorizontal();
            m_so_blb_hasLeft.boolValue = GUILayout.Toggle(bilatBase.hasLeft, "Has Left Leg");
            m_so_blb_hasRight.boolValue = GUILayout.Toggle(bilatBase.hasRight, "Has Right Leg");
            GUILayout.EndHorizontal();

            m_so_boneSetup.isExpanded = true;
            EditorGUILayout.PropertyField(m_so_boneSetup);
        }

        protected override void DrawSettings()
        {
            base.DrawSettings();

            SerializedProperty
                _m_so_forcedPositioning = serialized.FindProperty(nameof(forcedPositioning)),
                _m_so_feetOffset = serialized.FindProperty(nameof(feetOffset)),
                _m_so_feetLength = serialized.FindProperty(nameof(feetLength)),
                _m_so_toeHeight = serialized.FindProperty(nameof(toeHeight));

            m_so_l_feetIK.boolValue = GUILayout.Toggle(feetIK, "Feet (toes) IK", GUILayout.MaxWidth(120));

            m_so_l_legRaycasting.boolValue = GUILayout.Toggle(legRaycasting, new GUIContent("Prevent clipping", $"[{nameof(IKPModule_LowerBody)}.{nameof(legRaycasting)}]\nUse 'Leg Raycasting' to prevent limbs from clipping throug geometry by following the shape of the underlying surface."));

            if (m_so_l_legRaycasting.boolValue)
            {
                EditorGUILayout.PropertyField(m_so_l_legRaycastingMode);
            }

            EditorGUILayout.PropertyField(m_so_l_grounderBehavior);
            _m_so_forcedPositioning.boolValue = GUILayout.Toggle(forcedPositioning, new GUIContent("Forced Positioning", $"[{nameof(IKPModule_LowerBody)}.{nameof(forcedPositioning)}]\nForced positioning will attempt to correct any limb positioning errors. Forced positioning is applied after the joint rotations and in some rare cases might lead to undesired mesh deformations."));

            EditorGUILayout.PropertyField(_m_so_feetOffset);

            Vector2 _legLen2f = new Vector2(bodySetup.leftKneeDistance + bodySetup.leftFootDistance, bodySetup.rightKneeDistance + bodySetup.rightFootDistance);
            float _legLen = Mathf.Min((bilatBase.hasLeft) ? _legLen2f.x : _legLen2f.y, (bilatBase.hasRight) ? _legLen2f.y : _legLen2f.x) - IKPUtils.LIMB_DELTA_STRETCH_LIMIT;
            _m_so_feetLength.floatValue = EditorGUILayout.FloatField(new GUIContent("Feet Length", "[IKPModule_LowerBody.feetLength]\nDistance to the toes."), _m_so_feetLength.floatValue);

            EditorGUILayout.PropertyField(_m_so_toeHeight);

        }

        protected override void DrawProperties()
        {
            base.DrawProperties();

            GUILayout.Label("Main Properties:", EditorStyles.boldLabel);
            for (int i = 0; i < p_grounderWeight; i++)
            {
                DrawPropertyGUI(i, true);
            }

            if (grounderBehavior != GrounderType.None)
            {
                EditorGUILayout.Space();

                GUILayout.Label("Grounder Properties:", EditorStyles.boldLabel);
                DrawPropertyGUI(p_grounderWeight, true);
                DrawPropertyGUI(p_grounderReach);
                DrawPropertyGUI(p_grounderReactSpeed);

                EditorGUI.BeginDisabledGroup(!legRaycasting);
                {
                    DrawPropertyGUI(p_grounderFallSpeed);
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space(); //empty space
            GUILayout.Label("Other Properties:", EditorStyles.boldLabel);

            DrawPropertyGUI(p_legSmoothing);
            DrawPropertyGUI(p_feetSmoothing);
        }
#endif


        public override void AutoSetup(BodySetupContext bodySetupContext, StringBuilder outLog)
        {
            LowerBodySetup _bodySetup = new LowerBodySetup();

            //hips
            _bodySetup.hips = PickBone(bodySetupContext, HumanBodyBones.Hips, null, BoneNamesLibrary.hips);

            _bodySetup.leftThigh = PickBone(bodySetupContext, HumanBodyBones.LeftUpperLeg, BoneNamesLibrary.knee.Concat(BoneNamesLibrary.right), BoneNamesLibrary.thigh, BoneNamesLibrary.left);
            _bodySetup.leftKnee = PickBone(bodySetupContext, HumanBodyBones.LeftLowerLeg, BoneNamesLibrary.foot.Concat(BoneNamesLibrary.right), BoneNamesLibrary.knee, BoneNamesLibrary.left);
            _bodySetup.leftFoot = PickBone(bodySetupContext, HumanBodyBones.LeftFoot, BoneNamesLibrary.knee.Concat(BoneNamesLibrary.right), BoneNamesLibrary.foot, BoneNamesLibrary.left);

            _bodySetup.rightThigh = PickBone(bodySetupContext, HumanBodyBones.RightUpperLeg, BoneNamesLibrary.knee.Concat(BoneNamesLibrary.left), BoneNamesLibrary.thigh, BoneNamesLibrary.right);
            _bodySetup.rightKnee = PickBone(bodySetupContext, HumanBodyBones.RightLowerLeg, BoneNamesLibrary.foot.Concat(BoneNamesLibrary.left), BoneNamesLibrary.knee, BoneNamesLibrary.right);
            _bodySetup.rightFoot = PickBone(bodySetupContext, HumanBodyBones.RightFoot, BoneNamesLibrary.knee.Concat(BoneNamesLibrary.left), BoneNamesLibrary.foot, BoneNamesLibrary.right);

            bodySetup = _bodySetup;

            if (GetProperty(p_feetSmoothing) == 0) SetProperty(p_feetSmoothing, 5f);
            if (GetProperty(p_legSmoothing) == 0) SetProperty(p_legSmoothing, 5f);

#if UNITY_EDITOR

            InitSerializedPropertiesIfNeeded();
            serialized.Update();

            var _bs = serialized.FindProperty(nameof(bodySetup));
            var _bs_hips = _bs.FindPropertyRelative(nameof(LowerBodySetup.hips));
            var _bs_leftFoot = _bs.FindPropertyRelative(nameof(LowerBodySetup.leftFoot));
            var _bs_leftKnee = _bs.FindPropertyRelative(nameof(LowerBodySetup.leftKnee));
            var _bs_leftThigh = _bs.FindPropertyRelative(nameof(LowerBodySetup.leftThigh));
            var _bs_rightFoot = _bs.FindPropertyRelative(nameof(LowerBodySetup.rightFoot));
            var _bs_rightKnee = _bs.FindPropertyRelative(nameof(LowerBodySetup.rightKnee));
            var _bs_rightThigh = _bs.FindPropertyRelative(nameof(LowerBodySetup.rightThigh));

            _bs_hips.objectReferenceValue = (Transform)_bodySetup.hips;
            _bs_leftFoot.objectReferenceValue = (Transform)_bodySetup.leftFoot;
            _bs_leftKnee.objectReferenceValue = (Transform)_bodySetup.leftKnee;
            _bs_leftThigh.objectReferenceValue = (Transform)_bodySetup.leftThigh;
            _bs_rightFoot.objectReferenceValue = (Transform)_bodySetup.rightFoot;
            _bs_rightKnee.objectReferenceValue = (Transform)_bodySetup.rightKnee;
            _bs_rightThigh.objectReferenceValue = (Transform)_bodySetup.rightThigh;

            ApplyModifiedProperties();

#endif
            base.AutoSetup(bodySetupContext, outLog);
        }

        internal override void Init(Transform origin)
        {
            bodySetup.hipsRotationOffset = IKPUtils.GetRotationOffset(origin, bodySetup.hips);
            initialHipsV3off = IKPUtils.GetVectorOffset(origin, bodySetup.hips.position);
            RecordPreIKBoneStates();

            IKPLocalSpace _referenceOrientation = new IKPLocalSpace(bodySetup.hips.rotation * Quaternion.Inverse(bodySetup.hipsRotationOffset));

            if (bilatBase.hasLeft)
            {
                InitLegSetup(
                    ref bodySetup.leftThighRotationOffset,
                    ref bodySetup.leftKneeRotationOffset,
                    ref bodySetup.leftFootRotationOffset,
                    ref bodySetup.leftKneeDistance,
                    ref bodySetup.leftFootDistance,
                    _referenceOrientation,
                    ref bodySetup.leftThigh,
                    ref bodySetup.leftKnee,
                    ref bodySetup.leftFoot);
            }
            if (bilatBase.hasRight)
            {
                InitLegSetup(
                    ref bodySetup.rightThighRotationOffset,
                    ref bodySetup.rightKneeRotationOffset,
                    ref bodySetup.rightFootRotationOffset,
                    ref bodySetup.rightKneeDistance,
                    ref bodySetup.rightFootDistance,
                    _referenceOrientation,
                    ref bodySetup.rightThigh,
                    ref bodySetup.rightKnee,
                    ref bodySetup.rightFoot);
            }
        }

        private void RecordPreIKBoneStates()
        {
            Action<Transform> _apply = (t) =>
            {
                preIKBonePositions[t] = new TransformData(t.localPosition, t.localRotation);
            };
            _apply(bodySetup.hips);
            _apply(bodySetup.leftFoot);
            _apply(bodySetup.leftThigh);
            _apply(bodySetup.leftKnee);
            _apply(bodySetup.rightFoot);
            _apply(bodySetup.rightThigh);
            _apply(bodySetup.rightKnee);
        }

        private void RestorePreIKBoneStates()
        {
#if UNITY_EDITOR || DEBUG
            Debug.Assert(preIKBonePositions.Count > 0, "Restore called before any pre-IK bone states are available.");
#endif
            Action<Transform> _apply = (t) =>
            {
                t.localPosition = preIKBonePositions[t].position;
                t.localRotation = preIKBonePositions[t].rotation;
            };
            _apply(bodySetup.hips);
            _apply(bodySetup.leftFoot);
            _apply(bodySetup.leftThigh);
            _apply(bodySetup.leftKnee);
            _apply(bodySetup.rightFoot);
            _apply(bodySetup.rightThigh);
            _apply(bodySetup.rightKnee);
        }

        private void InitLegSetup(
            ref Quaternion thighRotationOffset,
            ref Quaternion kneeRotationOffset,
            ref Quaternion footRotationOffset,
            ref float kneeDistance,
            ref float footDistance,
            IKPLocalSpace referencePoint,
            ref Transform thigh,
            ref Transform knee,
            ref Transform foot)
        {

            Vector3 _thighToKnee = IKPUtils.NormalVector(thigh.position, knee.position);
            IKPLocalSpace _lsp = IKPUtils.CalculateLimbLocalSpace(referencePoint, _thighToKnee);
            /* SP-> thigh offset*/
            thighRotationOffset = IKPUtils.GetRotationOffset(_lsp.ToQuaternion(), thigh.rotation);
            Vector3 _kneeToFoot = IKPUtils.NormalVector(knee.position, foot.position);
            _lsp = IKPUtils.CalculateLimbLocalSpace(_lsp, _kneeToFoot);
            /* SP-> knee offset*/
            kneeRotationOffset = IKPUtils.GetRotationOffset(_lsp.ToQuaternion(), knee.rotation);
            /* SP-> foot offset*/
            footRotationOffset = IKPUtils.GetRotationOffset(referencePoint.ToQuaternion(), foot.rotation);
            /* SP-> k_dst*/
            kneeDistance = Vector3.Distance(thigh.position, knee.position);
            /* SP-> f_dst*/
            footDistance = Vector3.Distance(knee.position, foot.position);
        }

        public bool Has(Side side)
        {
            return (side == Side.Left) ? (bilatBase.hasLeft) : (bilatBase.hasRight);
        }


        //SP
#if UNITY_EDITOR
        protected override void InitializeSerializedProperties()
        { //init the serialized properties
            base.InitializeSerializedProperties();
            serialized.Update();
            m_so_l_feetIK = serialized.FindProperty(nameof(feetIK));
            m_so_blb_hasLeft = serialized.FindDeepProperty($"{nameof(bilatBase)}.{nameof(bilatBase.hasLeft)}");
            m_so_blb_hasRight = serialized.FindDeepProperty($"{nameof(bilatBase)}.{nameof(bilatBase.hasRight)}");
            m_so_l_legRaycasting = serialized.FindProperty(nameof(legRaycasting));
            m_so_l_grounderBehavior = serialized.FindProperty(nameof(grounderBehavior));
            m_so_l_legRaycastingMode = serialized.FindProperty(nameof(legRaycastingMode));
            m_so_boneSetup = serialized.FindProperty(nameof(bodySetup));
        }

        void OnDrawGizmos()
        {
            if (!Validate())
                return;

            if (bilatBase.hasLeft)
            {
                GizmoPalette _colorPalette = active ? GizmoPalette.Blue : GizmoPalette.White;
                IKPn.IKPEditorUtils.PaintBone(bodySetup.leftThigh.position, bodySetup.leftKnee.position, _colorPalette);
                IKPn.IKPEditorUtils.PaintBone(bodySetup.leftKnee.position, bodySetup.leftFoot.position + (bodySetup.leftFoot.position - bodySetup.leftKnee.position).normalized * feetOffset, _colorPalette, true);
            }

            if (bilatBase.hasRight)
            {
                GizmoPalette _colorPalette = active ? GizmoPalette.Red : GizmoPalette.White;
                IKPn.IKPEditorUtils.PaintBone(bodySetup.rightThigh.position, bodySetup.rightKnee.position, _colorPalette);
                IKPn.IKPEditorUtils.PaintBone(bodySetup.rightKnee.position, bodySetup.rightFoot.position + (bodySetup.rightFoot.position - bodySetup.rightKnee.position).normalized * feetOffset, _colorPalette, true);
            }
        }
#endif


        void PrePose()
        {
            hipsPrePose.Set(bodySetup.hips);
            leftFootPrePose.Set(bodySetup.leftFoot);
            rightFootPrePose.Set(bodySetup.rightFoot);
        }

        bool SphereCast(out RaycastHit hit, Vector3 pos, Vector3 dir, float dist)
        {
            float _radius = feetLength / 2f;
            Vector3 _origin = pos - dir * _radius;
            var _hit = Physics.SphereCast(_origin, _radius, dir, out hit, dist, raycastMask);
            return _hit;
        }

        private TargetProcessResult ProcessTarget(Side side)
        {
            TransformData _target = new TransformData();
            float
                _kneeDistance = 0f,
                _footDistance = 0f,
                _legWeight;
            Vector3
                _thighPosition = Vector3.zero;

            Transform _origin = ikp.origin;
            TransformData _unweightedTarget = default;

            switch (side)
            {
                case Side.Left:
                    _legWeight = GetProperty(p_leftLegWeight);
                    _target = TransformData.Lerp(leftFootPrePose, _unweightedTarget = bilatBase.GetTarget(Side.Left).Get(_origin), _legWeight);
                    _kneeDistance = bodySetup.leftKneeDistance;
                    _footDistance = bodySetup.leftFootDistance;
                    _thighPosition = bodySetup.leftThigh.position;
                    break;

                case Side.Right:
                    _legWeight = GetProperty(p_rightLegWeight);
                    _target = TransformData.Lerp(rightFootPrePose, _unweightedTarget = bilatBase.GetTarget(Side.Right).Get(_origin), _legWeight);
                    _kneeDistance = bodySetup.rightKneeDistance;
                    _footDistance = bodySetup.rightFootDistance;
                    _thighPosition = bodySetup.rightThigh.position;
                    break;
            }

            var _thighToTarget = _target.position - _thighPosition;
            var _thighToTargetDistance = _thighToTarget.magnitude;

            float _legLen = _kneeDistance + _footDistance - IKPUtils.LIMB_DELTA_STRETCH_LIMIT; //LIMB_STRETCH_LIMIT prevents errors where knee vector becomes zero
            bool _legClips = false;


            if (_thighToTarget != Vector3.zero)
            {
                //check for surfaces

                if (legRaycasting)
                {
                    RaycastHit _hit;
                    // raycast for target
                    Vector3 _fromPoint;
                    Vector3 _toDireciton;
                    float _castLength;
                    switch (legRaycastingMode)
                    {
                        case LegRaycasting.FromThighs:
                            _fromPoint = _thighPosition;
                            _toDireciton = _target.position - _thighPosition;
                            _castLength = _legLen + GetProperty(p_grounderReach);
                            break;
                        case LegRaycasting.VerticallyAligned:
                            _fromPoint = _target.position + _origin.up * _legLen;
                            _toDireciton = -_origin.up;
                            _castLength = _legLen * 2 + GetProperty(p_grounderReach);
                            break;
                        default: throw new Exception($"Leg Raycasting {legRaycastingMode} not defined");
                    }

                    if (SphereCast(out _hit, _fromPoint, _toDireciton, _castLength))
                    {
                        if (Vector3.SqrMagnitude(_hit.point - _thighPosition) < (_thighToTargetDistance * _thighToTargetDistance))
                        {
                            _target.position = _hit.point;
                            _unweightedTarget.position = _target.position;
                            _legClips = true;
                        }
                    }
                }
            }
#if UNITY_EDITOR
            //Debug.DrawLine(_target.position + Vector3.up * 0.04f, _target.position);
#endif
            return new TargetProcessResult
            {
                targetTrDt = _target,
                legClips = _legClips,
                unweightedTargetPosition = _unweightedTarget.position,
            };
        }

        void CalculateLeg(Side side, IKPLocalSpace referenceOrientation, ref Vector3 smoothTarg, ref Quaternion footSmoothRt, TargetProcessResult targetResult)
        {
            float
                _kneeRotationFactor = 0f,
                _footRotationFactor = 0f,
                _kneeLength_a = 0f,
                _footLength_b = 0f,
                _directionFactor = 1f;

            Vector3 _feetDropOffset = targetResult.dropFoot;

            Quaternion
                _thighRotationOffset = Quaternion.identity,
                _kneeRotationOffset = Quaternion.identity,
                _footRotationOffset = Quaternion.identity;

            Transform
                _thigh = null,
                _knee = null,
                _foot = null;

            TransformData
                _targetTrDt = targetResult.targetTrDt;

            switch (side)
            {
                case Side.Left:
                    _kneeRotationFactor = GetProperty(p_leftKneeRotation);
                    _footRotationFactor = GetProperty(p_leftFootRotation);
                    _kneeLength_a = bodySetup.leftKneeDistance;
                    _footLength_b = bodySetup.leftFootDistance;
                    _directionFactor = 1f;
                    _thighRotationOffset = bodySetup.leftThighRotationOffset;
                    _kneeRotationOffset = bodySetup.leftKneeRotationOffset;
                    _footRotationOffset = bodySetup.leftFootRotationOffset;
                    _thigh = bodySetup.leftThigh;
                    _knee = bodySetup.leftKnee;
                    _foot = bodySetup.leftFoot;
                    break;

                case Side.Right:
                    _kneeRotationFactor = GetProperty(p_rightKneeRotation);
                    _footRotationFactor = GetProperty(p_rightFootRotation);
                    _kneeLength_a = bodySetup.rightKneeDistance;
                    _footLength_b = bodySetup.rightFootDistance;
                    _directionFactor = -1f;
                    _thighRotationOffset = bodySetup.rightThighRotationOffset;
                    _kneeRotationOffset = bodySetup.rightKneeRotationOffset;
                    _footRotationOffset = bodySetup.rightFootRotationOffset;
                    _thigh = bodySetup.rightThigh;
                    _knee = bodySetup.rightKnee;
                    _foot = bodySetup.rightFoot;
                    break;
            }

            //cache:
            float _generalWeightValue = GetProperty(p_generalWeight);

            //apply the drop offset from the grounder calculations
            /*	drop works as follows: the primary leg remains unchanged,
				the secondary leg is calculated with an offset such that it would match the grounded pose after the drop has been applied to the hips.
			*/
            var _targetVector = _targetTrDt.position - _thigh.position;
            var _targetVectorNormal = _targetVector.normalized;

            _footLength_b += feetOffset;

            //get target vector based on the lerp between target position and animation position
            float _maxLimbReach = _kneeLength_a + _footLength_b - IKPUtils.LIMB_DELTA_STRETCH_LIMIT;
            float _distanceToTarget = Vector3.Distance(_thigh.position, _targetTrDt.position);

            //get target vec, apply smoothing
            Vector3 _targetVectorScaled = smoothTarg = Vector3.Lerp(smoothTarg
                , _targetVectorNormal * Mathf.Clamp(_distanceToTarget, IKPUtils.LIMB_DELTA_STRETCH_LIMIT, _maxLimbReach)
                , Time.deltaTime * GetProperty(p_legSmoothing));

            if (_targetVectorScaled != Vector3.zero)
            {
                IKPLocalSpace _targetLsp = IKPUtils.CalculateLimbLocalSpace(referenceOrientation, _targetVectorScaled);

                //knee positioning -> /*using Hero's formula*/
                var _c = _targetVectorScaled.magnitude;
                var _d = (IKPUtils.Pw2(_footLength_b) - IKPUtils.Pw2(_kneeLength_a) + IKPUtils.Pw2(_c)) / (2f * _c);
                float _hKnee = Mathf.Sqrt(Mathf.Abs((_footLength_b * _footLength_b) - (_d * _d)));

                if (float.IsNaN(_hKnee))
                {
                    // if for any reason hero's formula returns NaN, fallback to some value in order to avoid NaN error in the quaternion calculation.
                    _hKnee = 0f;
                }

                float _footMap = Mathf.Lerp(.05f, .4f, _footRotationFactor);
                Vector3 _kneeDirection = IKPUtils.Circle(
                        new ProjectionPlane(_targetLsp.up, _targetLsp.right * _directionFactor),
                        _footMap + _kneeRotationFactor + .56f
                    );
                /*knee pos ->*/
                var _kneePosition =
                    _thigh.position +
                    _targetVectorNormal * (_c - _d) +
                    _kneeDirection * _hKnee;

                //rotations:
                Vector3 _thighToKneeNrm = IKPUtils.NormalVector(_thigh.position, _kneePosition);
                Vector3 _footPosition = _thigh.position + _targetVectorScaled;
                Vector3 _kneeToFootNrm = IKPUtils.NormalVector(_kneePosition, _footPosition);
                var _feetHeightOffsetV3 = _kneeToFootNrm * feetOffset;

                IKPLocalSpace _thighLsp = IKPUtils.CalculateLimbLocalSpace(referenceOrientation, _thighToKneeNrm);
                IKPLocalSpace _kneeLsp = IKPUtils.CalculateLimbLocalSpace(_thighLsp, _kneeToFootNrm);

                if (_thigh.position != _footPosition)
                {
                    Quaternion _thighRotation = _thighLsp.ToQuaternion() * _thighRotationOffset;
                    Quaternion _kneeRotation = _kneeLsp.ToQuaternion() * _kneeRotationOffset;

                    /*thigh rot ->*/
                    _thigh.rotation = Quaternion.Lerp(_thigh.rotation, _thighRotation, _generalWeightValue);
                    /*knee rot ->*/
                    _knee.rotation = Quaternion.Lerp(_knee.rotation, _kneeRotation, _generalWeightValue);

                    //forced positioning:
                    if (forcedPositioning)
                    {
                        _knee.position = Vector3.Lerp(_knee.position, _kneePosition, _generalWeightValue);
                        _foot.position = Vector3.Lerp(_foot.position, _footPosition - _feetHeightOffsetV3, _generalWeightValue);
                    }
                }

                Vector3 _footPrediction = targetResult.predictionRecordFoot ?? _foot.position; // foot pos without drop offsetting the target

                if (feetIK)
                {
                    //foot algorithm
                    var _fOff = _feetDropOffset;
                    Vector3
                        _footPointA = _footPrediction + _fOff,
                        _footPointB; 
                    Vector3 _forward = IKPUtils.Circle(new ProjectionPlane(Vector3.Cross(_kneeToFootNrm, _targetLsp.right), _targetLsp.right * _directionFactor), _footMap) * feetLength;
                    Vector3 _rayPoint = _knee.position + _fOff + _forward;
                    
                    Vector3 _rayVector = Vector3.down;
                    RaycastHit _hit;
                    var _rayLen = _kneeLength_a + _footLength_b; // some arbitrary large enough value

                    if (Physics.Raycast(_rayPoint, _rayVector, out _hit, _rayLen, raycastMask))
                        _footPointB = _hit.point;
                    else
                        _footPointB = _rayPoint + _rayVector.normalized * _rayLen;

                    _footPointB -= _kneeToFootNrm * toeHeight;

                    //Debug.DrawRay(_footPointA, Vector3.right, Color.magenta, .1f, false);
                    //Debug.DrawRay(_rayPoint, _rayVector, Color.black, .1f, false);

                    Quaternion _footLookRotation = Quaternion.LookRotation(IKPUtils.NormalVector(_footPointA, _footPointB), -_kneeToFootNrm);
                    footSmoothRt = Quaternion.Lerp(footSmoothRt, _footLookRotation, GetProperty(p_feetSmoothing) * Time.deltaTime);
                    _foot.rotation = Quaternion.Lerp(_foot.rotation, footSmoothRt * _footRotationOffset, _generalWeightValue);
                }
            }
        }
    }
}
