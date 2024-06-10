using UnityEngine;
using SETUtil.Types;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using SETUtil.Extend;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
    [IKPModule(ModuleSignatures.UPPER_HUMANOID, displayName = "Upper Humanoid", inspectorOrder = 10, updateOrder = 20, iconPath = "IKP/ikp_upper_icon")]
    [AddComponentMenu(IKPUtils.MODULE_COMPONENT_MENU + "/Upper Body")]
    [DisallowMultipleComponent]
    public partial class IKPModule_UpperBody : ModuleBase_LookAroundLogicSSP
    {
        //SETUtil.OrderedComponent:
        public override int OrderIndex()
        {
            return IKP.ORDER_INDEX + ModuleManager.GetInspectorOrder(ModuleSignatures.UPPER_HUMANOID);
        }

        public enum Property
        {
            GeneralWeight,
            LeftArmWeight,
            RightArmWeight,
            ChestWeight,
            LeftElbowRotation,
            RightElbowRotation,
            LookSpeed
        }

        //property names cache
        /// <summary>
        /// Cached property index
        /// </summary>
        public static int
            p_generalWeight = (int)Property.GeneralWeight,
            p_leftArmWeight = (int)Property.LeftArmWeight,
            p_rightArmWeight = (int)Property.RightArmWeight,
            p_chestWeight = (int)Property.ChestWeight,
            p_leftElbowRotation = (int)Property.LeftElbowRotation,
            p_rightElbowRotation = (int)Property.RightElbowRotation,
            p_lookSpeed = (int)Property.LookSpeed;
        //---

        private enum Calculation
        {
            Hands = 0,
            Chest = 1,
        }

        //property names cache
        public static int
            c_hands = (int)Calculation.Hands,
            c_chest = (int)Calculation.Chest;
        //---

        //accessors:
        public int correctingHands
        {
            get { return m_correctingHands; }
            set
            {
#if UNITY_EDITOR
                InitSerializedPropertiesIfNeeded();
                SerializedProperty _so_correctingHands = serialized.FindProperty(nameof(m_correctingHands));
                _so_correctingHands.intValue = value;
                ApplyModifiedProperties();
#else
					m_correctingHands = value;
#endif
            }
        }

        public bool isCurrentlyCorrectingHands { get { return correctingHands >= 0; } }

        public float handsForwardCorrectionL
        {
            get { return m_handsForwardCorrection.x; }
            set
            {
#if UNITY_EDITOR
                InitSerializedPropertiesIfNeeded();
                SerializedProperty _so_p = serialized.FindProperty(nameof(m_handsForwardCorrection));
                _so_p.vector2Value = new Vector2(value, m_handsForwardCorrection.y);
                ApplyModifiedProperties();
#else
					m_handsForwardCorrection.x = value;
#endif
            }
        }

        public float handsForwardCorrectionR
        {
            get { return m_handsForwardCorrection.y; }
            set
            {
#if UNITY_EDITOR
                InitSerializedPropertiesIfNeeded();
                SerializedProperty _so_p = serialized.FindProperty(nameof(m_handsForwardCorrection));
                _so_p.vector2Value = new Vector2(m_handsForwardCorrection.x, value);
                ApplyModifiedProperties();
#else
					m_handsForwardCorrection.y = value;
#endif
            }
        }

        public Quaternion leftHandRotationCorrection
        {
            get { return m_leftHandRotationCorrection; }
            set
            {
#if UNITY_EDITOR
                InitSerializedPropertiesIfNeeded();
                SerializedProperty _so_p = serialized.FindProperty(nameof(m_leftHandRotationCorrection));
                _so_p.quaternionValue = value;
                ApplyModifiedProperties();
#else
					m_leftHandRotationCorrection = value;
#endif
            }
        }

        public Quaternion rightHandRotationCorrection
        {
            get { return m_rightHandRotationCorrection; }
            set
            {
#if UNITY_EDITOR
                InitSerializedPropertiesIfNeeded();
                SerializedProperty _so_p = serialized.FindProperty(nameof(m_rightHandRotationCorrection));
                _so_p.quaternionValue = value;
                ApplyModifiedProperties();
#else
					m_rightHandRotationCorrection = value;
#endif
            }
        }

        private IKPModule_EditorSimulation editorSimModule
        {
            get
            {
                if (m_editorSimModule != null)
                {
                    return m_editorSimModule;
                }

                IKPModule_EditorSimulation _editorSim = null;

                if (ikp)
                {
                    if ((_editorSim = (IKPModule_EditorSimulation)ikp.GetModule(ModuleSignatures.EDITOR_SIM)) != null)
                    {
                        m_editorSimModule = _editorSim;
                    }
                }

                return _editorSim;
            }
            set
            {
#if UNITY_EDITOR
                InitSerializedPropertiesIfNeeded();
                SerializedProperty _so_editorSim = serialized.FindProperty(nameof(m_editorSimModule));
                _so_editorSim.objectReferenceValue = value;
                ApplyModifiedProperties();
#else
				m_editorSimModule = value;
#endif
            }
        }

        //variables:
        //public:
        [SerializeField] [FormerlySerializedAs("_boneSetup")] private UpperBodySetup bodySetup;

        [HideInInspector]
        public bool
            useTargRot = true,
            verticalLook = true,
            forcedPositioning = true;

        [HideInInspector]
        [SerializeField]
        public ChestTargetMode chestTargetMode = ChestTargetMode.Combined;

        [HideInInspector]
        [SerializeField]
        internal BilateralBase bilatBase = new BilateralBase();

        //private:

        [HideInInspector]
        [SerializeField]
        private Vector3 lookTarget3f = Vector3.zero;

        [HideInInspector]
        [SerializeField] //hide in inspector and enabled through the Editor script so it's ensured that the setup is OK
        private bool
            hasSpine = true,
            m_correctingRotation = false;

        [HideInInspector]
        [SerializeField]
        private int m_correctingHands = -1; //-1 not editing; 0 left; 1 right;

        private float chestToHeadOffset = 0.2f;

        [HideInInspector]
        [SerializeField]
        private Vector2
            m_handsForwardCorrection = Vector2.zero; //x => left, y => right

        private Vector3
            leftHandStoredPosition = Vector3.zero,
            rightHandStoredPosition = Vector3.zero;

        [HideInInspector]
        [SerializeField]
        private Quaternion
            m_leftHandRotationCorrection = Quaternion.identity,
            m_rightHandRotationCorrection = Quaternion.identity;

        private Quaternion
            chRelativeQuatSmooth = Quaternion.identity,
            spRelativeQuatSmooth = Quaternion.identity,
            chestNeutral = Quaternion.identity,
            spineNeutral = Quaternion.identity;

        private IKPModule_Head headModule = null;
        private IKPModule_EditorSimulation m_editorSimModule = null;

        private TransformData
            leftStoredTrDt = new TransformData(),
            rightStoredTrDt = new TransformData();

        private SETUtil.SceneUI.GUIButton
            modeGUIElement,
            saveGUIElement;
        private SETUtil.SceneUI.GUILabel activeLabelGUIElement;

#if UNITY_EDITOR

        private float handsForwardCorrectionClipboard;
        private Quaternion handRotationCorrectionClipboard;

        SerializedProperty
            upper_hasSpine,
            upper_useTargRot,
            upper_verticalLook,
            m_so_handsForwardCorrection,
            m_so_forcedPositioning;
        private SerializedProperty m_so_boneSetup;
#endif

        void OnDrawGizmos()
        {
            ClampVariables();
            DrawGizmos();
        }

        void DrawGizmos()
        {
            if (!Validate())
                return;

            if (bilatBase.hasLeft)
            {
                GizmoPalette g1 = active ? GizmoPalette.Blue : GizmoPalette.White;
                IKPEditorUtils.PaintBone(bodySetup.leftShoulder.position, bodySetup.leftElbow.position, g1);
                IKPEditorUtils.PaintBone(bodySetup.leftElbow.position, bodySetup.leftHand.position, g1, true);
            }
            if (bilatBase.hasRight)
            {
                GizmoPalette g2 = active ? GizmoPalette.Red : GizmoPalette.White;
                IKPEditorUtils.PaintBone(bodySetup.rightShoulder.position, bodySetup.rightElbow.position, g2);
                IKPEditorUtils.PaintBone(bodySetup.rightElbow.position, bodySetup.rightHand.position, g2, true);
            }

        }

        public void DrawHandCorrection(Side side)
        {
#if UNITY_EDITOR
            //callded by OnSceneGUI
            Vector3 _guiPivot = (side == Side.Left) ? bodySetup.leftHand.position : bodySetup.rightHand.position;

            // handle GUI elements
            if (modeGUIElement == null || modeGUIElement.onClick == null)
            {
                modeGUIElement = new SETUtil.SceneUI.GUIButton((m_correctingRotation ? "<size=12>Edit F-Offset</size>" : "<size=12>Edit Rotation</size>"), new Rect(-50, -125, 100, 20), delegate () { m_correctingRotation = !m_correctingRotation; });
            }
            else
            {
                modeGUIElement.text = m_correctingRotation ? "<size=12>Edit F-Offset</size>" : "<size=12>Edit Rotation</size>";
            }

            if (saveGUIElement == null || saveGUIElement.onClick == null)
            {
                saveGUIElement = new SETUtil.SceneUI.GUIButton("<size=12>Save</size>", new Rect(55, -95, 50, 20), EndHandCorrection);
            }
            if (activeLabelGUIElement == null)
            {
                activeLabelGUIElement = new SETUtil.SceneUI.GUILabel("F-Offset: " + ((side == Side.Left) ? m_handsForwardCorrection.x : m_handsForwardCorrection.y), new Rect(5, -25, 150, 20));
            }

            SETUtil.EditorUtil.DrawSceneElement(modeGUIElement, _guiPivot);
            SETUtil.EditorUtil.DrawSceneElement(saveGUIElement, _guiPivot);

            if (!m_correctingRotation)
                SETUtil.EditorUtil.DrawSceneElement(activeLabelGUIElement, _guiPivot);

            //HANDLES:
            // L
            if (m_correctingHands == 0 && bilatBase.hasLeft)
            {
                TransformData _tgt = ProcessTarget(Side.Left);
                if (m_correctingRotation)
                {
                    //draw rotation handle
                    EditorGUI.BeginChangeCheck();
                    Quaternion _val4f = Handles.RotationHandle(leftHandRotationCorrection, bodySetup.leftHand.position);
                    if (EditorGUI.EndChangeCheck())
                        leftHandRotationCorrection = _val4f;
                }
                else
                {
                    //draw offset handle
                    EditorGUI.BeginChangeCheck();
                    float _val = -.5f + Handles.ScaleValueHandle(handsForwardCorrectionL + .5f, bodySetup.leftHand.position, _tgt.rotation, HandleUtility.GetHandleSize(bodySetup.leftHand.position) * 6f, Handles.ArrowHandleCap, .1f);
                    if (EditorGUI.EndChangeCheck())
                        handsForwardCorrectionL = Mathf.Max(-0.49f, _val);
                }
            }
            // R
            if (m_correctingHands == 1 && bilatBase.hasRight)
            {
                TransformData _tgt = ProcessTarget(Side.Right);
                if (m_correctingRotation)
                {
                    //draw rotation handle
                    EditorGUI.BeginChangeCheck();
                    Quaternion _val4f = Handles.RotationHandle(rightHandRotationCorrection, bodySetup.rightHand.position);
                    if (EditorGUI.EndChangeCheck())
                        rightHandRotationCorrection = _val4f;
                }
                else
                {
                    //draw offset handle
                    EditorGUI.BeginChangeCheck();
                    float _val = -.5f + Handles.ScaleValueHandle(handsForwardCorrectionR + .5f, bodySetup.rightHand.position, _tgt.rotation, HandleUtility.GetHandleSize(bodySetup.rightHand.position) * 6f, Handles.ArrowHandleCap, .1f);
                    if (EditorGUI.EndChangeCheck())
                        handsForwardCorrectionR = Mathf.Max(-0.49f, _val);
                }
            }
#endif
        }

        void ClampVariables()
        {
            if (GetProperty(p_lookSpeed) < 0f)
                SetProperty(p_lookSpeed, 0f);
        }

        public override ExecutionFlag IKPPreUpdate()
        {
            Quaternion _hipsRot = bodySetup.hips.rotation;
            bodySetup.chest.rotation = chestNeutral = _hipsRot * bodySetup.hipsChestOffset;
            if (hasSpine)
                bodySetup.spine.rotation = spineNeutral = _hipsRot * bodySetup.hipsSpineOffset;

            return ExecutionFlag.Continue;
        }

        public override ExecutionFlag IKPUpdate()
        {
            if (base.IKPUpdate() == ExecutionFlag.Break)
            {
                return ExecutionFlag.Break;
            }

            ClampVariables();
            if (headModule)
                chestToHeadOffset = Vector3.Distance(headModule.GetHeadPosition(), bodySetup.chest.position);
            if (chestTargetMode != ChestTargetMode.None)
                Calculate(c_chest); //CALCULATE THE CHEST (before the hands, so that the hands can always be precise)
            Calculate(c_hands); //CALCULATE THE HANDS

            return ExecutionFlag.Continue;
        }

#if UNITY_EDITOR
        public override void DrawEditorGUI()
        {
            base.DrawEditorGUI();

            //draw target settings:
            if (bilatBase.hasLeft)
                IKPEditorUtils.DrawTargetGUI(serialized.FindProperty($"{nameof(bilatBase)}.{BilateralBase.PROPERTY_NAME_LEFT_IKP_TARGET}"), "Left Arm Target");
            if (bilatBase.hasRight)
                IKPEditorUtils.DrawTargetGUI(serialized.FindProperty($"{nameof(bilatBase)}.{BilateralBase.PROPERTY_NAME_RIGHT_IKP_TARGET}"), "Right Arm Target");
            DrawChestTargetGUI();
        }

        protected override void DrawSetup()
        {
            base.DrawSetup();

            GUILayout.BeginHorizontal();
            using (new ColorPocket())
            {
                var p_hasLeft = serialized.FindProperty($"{nameof(bilatBase)}.{nameof(bilatBase.hasLeft)}");
                p_hasLeft.boolValue = GUILayout.Toggle(bilatBase.hasLeft, "Has Left Arm");

            }
            using (new ColorPocket())
            {
                SerializedProperty p_hasRight = serialized.FindProperty($"{nameof(bilatBase)}.{nameof(bilatBase.hasRight)}");
                p_hasRight.boolValue = GUILayout.Toggle(bilatBase.hasRight, "Has Right Arm");
            }

            GUILayout.EndHorizontal();
            upper_hasSpine.boolValue = GUILayout.Toggle(hasSpine, "Has Spine");

            m_so_boneSetup.isExpanded = true;
            EditorGUILayout.PropertyField(m_so_boneSetup);
        }

        protected override void DrawSettings()
        {
            base.DrawSettings();

            upper_useTargRot.boolValue = GUILayout.Toggle(useTargRot, new GUIContent("Align hands with target rotation", "[IKPModule_UpperBody.useTargRot]\nAlign the hands with the rotation of their respective targets. The closer the target, the stronger the effect on the hands.\n(Requires IKP_Target)"));
            upper_verticalLook.boolValue = GUILayout.Toggle(verticalLook, new GUIContent("Vertical Look", "[IKPModule_UpperBody.verticalLook]\nAllow the chest to track the look target on its vertical axis."));
            m_so_forcedPositioning.boolValue = GUILayout.Toggle(forcedPositioning, new GUIContent("Forced Positioning", "[IKPModule_UpperBody.forcedPositioning]\nForced positioning will attempt to correct any limb positioning error. Forced positioning is applied after the joint rotations and in some rare cases might lead to undesired mesh deformations."));

            //hand correction

            EditorGUILayout.Space();

            if (bilatBase.hasLeft || bilatBase.hasRight)
            {
                using (new ColorPocket())
                {
                    if (correctingHands >= 0)
                    {
                        GUI.color = IKPStyle.COLOR_HIGHTLIGHT_BOX;
                    }

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        GUILayout.Label("Hand Pose Correction:", EditorStyles.boldLabel);

                        //hand correction information:
                        if (!Application.isPlaying)
                        {
                            if (bilatBase.hasLeft && correctingHands != 1)
                            {
                                DrawHandCorrectionInspector(Side.Left);
                            }

                            if (bilatBase.hasRight && correctingHands != 0)
                            {
                                SETUtil.EditorUtil.HorizontalRule();
                                DrawHandCorrectionInspector(Side.Right);
                            }
                        }
                        else
                            GUILayout.Label("[!] Can't edit hand correction in Play Mode!");
                    }
                    GUILayout.EndVertical();
                }
            }
        }

        protected override void DrawProperties()
        {
            base.DrawProperties();

            GUILayout.Label("Main Properties:", EditorStyles.boldLabel);
            for (int i = 0; i < p_lookSpeed; i++)
                DrawPropertyGUI(i, true);

            EditorGUILayout.Space(); //empty space
            GUILayout.Label("Other Properties:", EditorStyles.boldLabel);
            DrawPropertyGUI(p_lookSpeed);
        }
#endif

        private void DrawChestTargetGUI()
        {
#if UNITY_EDITOR
            string targ = "chestTargetMode";
            InitSerializedPropertiesIfNeeded();
            serialized.Update();

            SerializedProperty so_targ = serialized.FindProperty(targ);

            using (new ColorPocket(IKPStyle.COLOR_TARGET))
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.Label(SETUtil.StringUtil.WordSplit(targ, true), EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(so_targ, new GUIContent("Target Behavior:", "[IKPModule_UpperBody.chestTargetMode]\nSelect chest behavior."));

                GUILayout.EndVertical();
            }

            ApplyModifiedProperties();
#endif
        }

        /// <summary>
        /// Calculate the lib by ignoring the current bilat.ikpTarget and using the provided params instead.
        /// </summary>
        internal void SetTempLimbTargetInternal(Side side, TransformData target)
        {
            IKPLocalSpace _chestReferencePoint = new IKPLocalSpace(bodySetup.chest.rotation * Quaternion.Inverse(bodySetup.chestRotationOffset));
            switch(side)
            {
                case Side.Left:
                CalculateLeftHand(_chestReferencePoint, target);
                    break;
                case Side.Right:
                CalculateRightHand(_chestReferencePoint, target);
                    break;
            }
        }

        internal void Calculate(int calculationTypeIndex)
        {
            if (calculationTypeIndex == c_hands)
            { //HANDS
                IKPLocalSpace _chestReferencePoint = new IKPLocalSpace(bodySetup.chest.rotation * Quaternion.Inverse(bodySetup.chestRotationOffset));

                if (bilatBase.hasLeft)
                {
                    CalculateLeftHand(_chestReferencePoint);
                }
                if (bilatBase.hasRight)
                {
                    CalculateRightHand(_chestReferencePoint);
                }
            }
            if (calculationTypeIndex == c_chest)
            { //CHEST
                Vector3
                    _target = Vector3.zero,
                    _srotDir = Vector3.zero,
                    _lookTarget = (headModule != null) ? headModule.lookTarget : lookTarget3f;

                Quaternion hipsQuat = bodySetup.hips.rotation;
                IKPLocalSpace ikpLsp = new IKPLocalSpace(hipsQuat * Quaternion.Inverse(bodySetup.hipsRotationOffset));

                chestNeutral = bodySetup.chest.rotation;
                if (hasSpine)
                    spineNeutral = bodySetup.spine.rotation;

                //TARGET
                if (chestTargetMode == ChestTargetMode.LookTarget)
                {
                    _target = _lookTarget - ikpLsp.up * chestToHeadOffset;
                }
                else
                { //calculate hands reach
                    if (leftHandStoredPosition == rightHandStoredPosition && leftHandStoredPosition == Vector3.zero)
                    {
                        leftHandStoredPosition = bodySetup.leftHand.position;
                        rightHandStoredPosition = bodySetup.rightHand.position;
                    }

                    Vector3 ihands = IKPUtils.NormalVector(leftHandStoredPosition, rightHandStoredPosition); //vector from the left hand to the right hand
                    _srotDir = IKPUtils.ProjectVector(new ProjectionPlane(ikpLsp.up, ikpLsp.right), ihands);
                    _srotDir = Vector3.Cross(ikpLsp.forward, _srotDir);
                    ihands = IKPUtils.ProjectVector(new ProjectionPlane(ikpLsp.forward, ikpLsp.right), ihands);
                    Vector3 t = bodySetup.hips.position + Vector3.Cross(ihands, ikpLsp.up); //neutral target

                    if (chestTargetMode == ChestTargetMode.HandsReach)
                    {
                        _target = t;
                    }
                    if (chestTargetMode == ChestTargetMode.Combined)
                    {
                        _target = Vector3.Lerp(t, _lookTarget, 0.5f);
                    }
                }

                //fallback and target vector
                Vector3 targetVec = _target - bodySetup.hips.position;

                if (!verticalLook) //flatten the target if vertical look is disabled
                    targetVec = IKPUtils.ProjectVector(new ProjectionPlane(ikpLsp.forward, ikpLsp.right), targetVec);

                float angl = Vector3.Angle(targetVec, ikpLsp.forward);

                if (trackTargetOnBack)
                {
                    if (angl >= angleLimit / 2f)
                    {
                        targetVec = IKPUtils.LimitedAngle(ikpLsp, angleLimit, targetVec);
                    }
                }
                else
                {
                    if (angl >= angleLimit / 2f)
                    {
                        targetVec = ikpLsp.forward;
                    }
                }

                Quaternion targetQuat = Quaternion.LookRotation(targetVec, ikpLsp.up + _srotDir * 0.9f); //clamp the _srotDir to 0.9f to normalize the sideways rotation behavior
                float chestWeightLimit = 1.8f;

                var _generalWeight = GetProperty(p_generalWeight);
                Quaternion chQuat = Quaternion.Lerp(chestNeutral, targetQuat * bodySetup.chestRotationOffset, GetProperty(p_chestWeight) / chestWeightLimit * _generalWeight);
                Quaternion spQuat = Quaternion.Lerp(spineNeutral, targetQuat * bodySetup.spineRotationOffset, GetProperty(p_chestWeight) / 2f / chestWeightLimit * _generalWeight);

                float lookSpeed = GetProperty(p_lookSpeed);

                chRelativeQuatSmooth = Quaternion.Lerp(chRelativeQuatSmooth, IKPUtils.GetRotationOffset(ikp.origin.rotation, chQuat), lookSpeed * Time.deltaTime);
                if (hasSpine)
                    spRelativeQuatSmooth = Quaternion.Lerp(spRelativeQuatSmooth, IKPUtils.GetRotationOffset(ikp.origin.rotation, spQuat), lookSpeed * Time.deltaTime);

                //clamp the rotation
                Vector3 chFwd = chRelativeQuatSmooth * Quaternion.Inverse(bodySetup.chestRotationOffset) * Vector3.forward;
                if (forcedAngleClamp && Vector3.Angle(ikpLsp.forward, chFwd) > angleLimit / 2f)
                {
                    if (hasSpine)
                        spRelativeQuatSmooth = spQuat;
                    chRelativeQuatSmooth = chQuat;
                }

                if (hasSpine)
                    bodySetup.spine.rotation = ikp.origin.rotation * spRelativeQuatSmooth;
                bodySetup.chest.rotation = ikp.origin.rotation * chRelativeQuatSmooth;
            }
        }

        private void CalculateLeftHand(IKPLocalSpace _chestReferencePoint, TransformData? substituteTarget = null)
        {
            CalculateArm(Side.Left,
                                    ref bodySetup.leftShoulder,
                                    ref bodySetup.leftElbow,
                                    ref bodySetup.leftHand,
                                    ref leftHandStoredPosition,
                                    Mathf.Lerp(0.7f, 1.1f, GetProperty(p_leftElbowRotation)),
                                    GetProperty(p_leftArmWeight),
                                    bodySetup.leftShoulderRotationOffset,
                                    bodySetup.leftElbowRotationOffset,
                                    bodySetup.leftHandRotationOffset,
                                    bodySetup.leftElbowLength,
                                    bodySetup.leftHandLength,
                                    -1f,
                                    _chestReferencePoint,
                                    substituteTarget);
        }

        private void CalculateRightHand(IKPLocalSpace _chestReferencePoint, TransformData? substituteTarget = null)
        {
            CalculateArm(Side.Right,
                                    ref bodySetup.rightShoulder,
                                    ref bodySetup.rightElbow,
                                    ref bodySetup.rightHand,
                                    ref rightHandStoredPosition,
                                    Mathf.Lerp(0.7f, 1.1f, GetProperty(p_rightElbowRotation)),
                                    GetProperty(p_rightArmWeight),
                                    bodySetup.rightShoulderRotationOffset,
                                    bodySetup.rightElbowRotationOffset,
                                    bodySetup.rightHandRotationOffset,
                                    bodySetup.rightElbowLength,
                                    bodySetup.rightHandLength,
                                    1f,
                                    _chestReferencePoint,
                                    substituteTarget);
        }

        private TransformData ProcessTarget(Side side, TransformData? substituteTarget = null)
        {
            float
                _armLength = 0,
                _forwardCorrection = 0;

            Vector3
                _handTargetVector = Vector3.zero,
                _shoulderPosition = Vector3.zero;

            Quaternion
                _rotationCorrection = Quaternion.identity;

            TransformData
                _handTrDt = new TransformData(),
                _handTarget = substituteTarget ?? bilatBase.GetTarget(side).Get(ikp.origin);


            //fill side-specific variables and feed them into the algorithm
            switch (side)
            {
                case Side.Left:
                    //feed left hand variables
                    _shoulderPosition = bodySetup.leftShoulder.position;
                    _armLength = bodySetup.leftElbowLength + bodySetup.leftHandLength;
                    _forwardCorrection = m_handsForwardCorrection.x;
                    _rotationCorrection = m_leftHandRotationCorrection;
                    break;
                case Side.Right:
                    //feed right hand variables
                    _shoulderPosition = bodySetup.rightShoulder.position;
                    _armLength = bodySetup.rightElbowLength + bodySetup.rightHandLength;
                    _forwardCorrection = m_handsForwardCorrection.y;
                    _rotationCorrection = m_rightHandRotationCorrection;
                    break;
            }

            //calculate
            //--position

            //apply pos correction
            _handTarget.position += _handTarget.forward * _forwardCorrection;

            _handTargetVector = IKPUtils.NormalVector(_shoulderPosition, _handTarget.position);
            float _handForward = Vector3.Distance(_shoulderPosition, _handTarget.position);
            _handForward = Mathf.Clamp(_handForward, IKPUtils.LIMB_DELTA_STRETCH_LIMIT, _armLength - IKPUtils.LIMB_DELTA_STRETCH_LIMIT); //clamp so the elbow & forward vectors are never 0
            _handTrDt.position = _shoulderPosition + _handTargetVector * _handForward;

            //--rotation

            //apply rot correction
            _handTarget.rotation *= _rotationCorrection;

            IKPLocalSpace referencePoint = new IKPLocalSpace(bodySetup.chest.rotation * Quaternion.Inverse(bodySetup.chestRotationOffset));

            float _handTargetLerp = Vector3.Distance(_handTrDt.position, _handTarget.position) / _armLength;
            IKPLocalSpace _localSpaceHand = IKPUtils.CalculateLimbLocalSpace(referencePoint, _shoulderPosition, _handTrDt.position);

            Quaternion _defaultAlgHandRot = Quaternion.LookRotation(_localSpaceHand.forward, _localSpaceHand.up); //algorithmic rotation
            _handTrDt.rotation = _defaultAlgHandRot;

            if (useTargRot)
            {
                _handTrDt.rotation = Quaternion.Lerp(_handTarget.rotation, _defaultAlgHandRot, _handTargetLerp); //if the target is too far out of reach, use the algorithmic rotation
            }

            if (side == Side.Left)
            {
                leftStoredTrDt = _handTrDt;
            }
            else
            {
                rightStoredTrDt = _handTrDt;
            }

            return _handTrDt;
        }

        private void CalculateArm(
            Side side,
            ref Transform shoulder,
            ref Transform elbow,
            ref Transform hand,
            ref Vector3 outHandStoredPosition,
            float elbowRotation,
            float armWeight,
            Quaternion shoulderRotationOffset,
            Quaternion elbowRotationOffset,
            Quaternion handRotationOffset,
            float elbowDistance,
            float handDistance,
            float elbowDirection, //does the elbow rotates inwards or outwards
            IKPLocalSpace referencePoint,
            TransformData? substituteTarget = null)
        {

            TransformData _handTransformData = ProcessTarget(side, substituteTarget);
            var _shoulderPosition = shoulder.position;

            float _handForward = Vector3.Distance(_shoulderPosition, _handTransformData.position);
            IKPLocalSpace _localSpaceHand = IKPUtils.CalculateLimbLocalSpace(referencePoint, _shoulderPosition, _handTransformData.position);

            //[hero's formula]
            var _elbowDst2 = Mathf.Pow(elbowDistance, 2);
            float _hElbow = Mathf.Sqrt(_elbowDst2 - Mathf.Pow((-Mathf.Pow(handDistance, 2) + _elbowDst2 + Mathf.Pow(_handForward, 2)) / (2f * _handForward), 2));

            if (float.IsNaN(_hElbow))
            {
                // if for any reason hero's formula returns NaN, fallback to some value in order to avoid NaN error in the quaternion calculation.
                _hElbow = IKPUtils.LIMB_DELTA_STRETCH_LIMIT;
            }

            Vector3 _elbowVector = IKPUtils.Circle(new ProjectionPlane(_localSpaceHand.up, _localSpaceHand.right * elbowDirection), elbowRotation);
            Vector3 _elbowPosition = _shoulderPosition +
                _localSpaceHand.forward * _handForward * (elbowDistance / (handDistance + elbowDistance)) +
                _hElbow * _elbowVector;
            Vector3 _shoulderToElbow = IKPUtils.NormalVector(_shoulderPosition, _elbowPosition);
            Vector3 _elbowToHand = IKPUtils.NormalVector(_elbowPosition, _handTransformData.position);

            float _p_generalWeight = GetProperty(p_generalWeight);
            float _totalLogWeight = IKPUtils.LogToOne(armWeight * _p_generalWeight);
            float _totalWeight = armWeight * _p_generalWeight;
            
            shoulder.rotation = Quaternion.Lerp(shoulder.rotation, Quaternion.LookRotation(_shoulderToElbow * elbowDistance, _localSpaceHand.up) * shoulderRotationOffset, _totalLogWeight);
            elbow.rotation = Quaternion.Lerp(elbow.rotation, Quaternion.LookRotation(_elbowToHand * handDistance, _localSpaceHand.up) * elbowRotationOffset, _totalLogWeight);
            hand.rotation = Quaternion.Lerp(hand.rotation, _handTransformData.rotation * handRotationOffset, _p_generalWeight * armWeight);

            //forced positioning:
            if (forcedPositioning)
            {
                elbow.position = Vector3.Lerp(elbow.position, _elbowPosition, _totalWeight);
                hand.position = Vector3.Lerp(hand.position, _handTransformData.position, _totalWeight);
            }

            outHandStoredPosition = hand.position;
        }

        public void SetLookTarget(Vector3 position)
        {
#if UNITY_EDITOR
            InitSerializedPropertiesIfNeeded();
            serialized.Update();
            SerializedProperty _m_so_lookTarget3f = serialized.FindProperty(nameof(lookTarget3f));
            _m_so_lookTarget3f.vector3Value = position;
            ApplyModifiedProperties();
#else
			lookTarget3f = position;
#endif
        }

        public void SetChestTargetMode(ChestTargetMode tgtMode)
        {
            chestTargetMode = tgtMode;
        }

        public float GetMaxLimbStretch(Side side)
        {
            float _limbLen = IKPUtils.LIMB_DELTA_STRETCH_LIMIT;
            if (Validate())
            {
                if (side == Side.Left)
                    if (bilatBase.hasLeft)
                        _limbLen = bodySetup.leftElbowLength + bodySetup.leftHandLength;

                if (side == Side.Right)
                    if (bilatBase.hasRight)
                        _limbLen = bodySetup.rightElbowLength + bodySetup.rightHandLength;
            }
            float _limbLimit = _limbLen - IKPUtils.LIMB_DELTA_STRETCH_LIMIT;

            return _limbLimit;
        }

        public bool Has(Side side) { return Has((int)side); }
        public bool Has(int i)
        {
            if (i == (int)Side.Left)
                return bilatBase.hasLeft;
            else if (i == (int)Side.Right)
                return bilatBase.hasRight;
            else
                return hasSpine;
            /* implementing a little hack, where
			if the input is not int for left or int for right, returns the spine boolean value.
			This prevents having additional methods fro the spine */
        }

        public Vector3 GetPivot(Side wp)
        {
            if (wp == Side.Right && bilatBase.hasRight)
            {
                return bodySetup.rightShoulder.position;
            }
            else if (bilatBase.hasLeft)
                return bodySetup.leftShoulder.position;
            return bodySetup.chest.position; //fallback in case the upper body has no arms setup
        }

        public TransformData GetLimbTransformData(Side side)
        {
            return GetLimbTransformData(side, false);
        }

        public TransformData GetLimbTransformData(Side side, bool rotOffset)
        {
            Quaternion hRot;
            if (side == Side.Right && bilatBase.hasRight)
            {
                hRot = bodySetup.rightHand.rotation;
                if (rotOffset)
                    hRot *= Quaternion.Inverse(bodySetup.rightHandRotationOffset);
                return new TransformData(bodySetup.rightHand.position, hRot);
            }
            if (side == Side.Left && bilatBase.hasLeft)
            {
                hRot = bodySetup.leftHand.rotation;
                if (rotOffset)
                    hRot *= Quaternion.Inverse(bodySetup.leftHandRotationOffset);
                return new TransformData(bodySetup.leftHand.position, hRot);
            }
            return new TransformData();
        }

        public Vector3 ForwardOffset(Side side)
        {
            if (side == Side.Left && bilatBase.hasLeft)
            {
                return leftStoredTrDt.forward * m_handsForwardCorrection.x;
            }
            if (side == Side.Right && bilatBase.hasRight)
            {
                return rightStoredTrDt.forward * m_handsForwardCorrection.y;
            }
            return Vector3.zero;
        }

        public Quaternion GetChestRotation()
        {
            return bodySetup.chest.rotation * Quaternion.Inverse(bodySetup.chestRotationOffset);
        }

        public Vector3 GetHipsPosition()
        {
            return bodySetup.hips.position;
        }

        public override void AutoSetup(BodySetupContext bodySetupContext, StringBuilder outLog)
        {
            UpperBodySetup _bodySetup = new UpperBodySetup();

            List<Transform>
                _arms = new List<Transform>(),
                _elbows = new List<Transform>(),
                _hands = new List<Transform>();

            _bodySetup.hips = PickBone(bodySetupContext, HumanBodyBones.Hips, null, BoneNamesLibrary.hips);
            _bodySetup.spine = PickBone(bodySetupContext, HumanBodyBones.Spine, null, BoneNamesLibrary.spine);
            _bodySetup.chest = PickBone(bodySetupContext, HumanBodyBones.Chest, null, BoneNamesLibrary.chest);

            _bodySetup.leftShoulder = PickBone(bodySetupContext, HumanBodyBones.LeftUpperArm, BoneNamesLibrary.elbow.Concat(BoneNamesLibrary.hand), BoneNamesLibrary.arm, BoneNamesLibrary.left);
            _bodySetup.leftElbow = PickBone(bodySetupContext, HumanBodyBones.LeftLowerArm, BoneNamesLibrary.hand.Concat(BoneNamesLibrary.arm), BoneNamesLibrary.elbow, BoneNamesLibrary.left);
            _bodySetup.leftHand = PickBone(bodySetupContext, HumanBodyBones.LeftHand, BoneNamesLibrary.elbow.Concat(BoneNamesLibrary.arm), BoneNamesLibrary.hand, BoneNamesLibrary.left);

            _bodySetup.rightShoulder = PickBone(bodySetupContext, HumanBodyBones.RightUpperArm, BoneNamesLibrary.elbow.Concat(BoneNamesLibrary.hand), BoneNamesLibrary.arm, BoneNamesLibrary.right);
            _bodySetup.rightElbow = PickBone(bodySetupContext, HumanBodyBones.RightLowerArm, BoneNamesLibrary.hand.Concat(BoneNamesLibrary.arm), BoneNamesLibrary.elbow, BoneNamesLibrary.right);
            _bodySetup.rightHand = PickBone(bodySetupContext, HumanBodyBones.RightHand, BoneNamesLibrary.elbow.Concat(BoneNamesLibrary.arm), BoneNamesLibrary.hand, BoneNamesLibrary.right);

            bodySetup = _bodySetup;


            if (GetProperty(p_lookSpeed) == 0) SetProperty(p_lookSpeed, 5f);

#if UNITY_EDITOR
            // serialize
            InitSerializedPropertiesIfNeeded();
            serialized.Update();

            var m_so_bs = serialized.FindProperty(nameof(bodySetup));
            var m_so_bs_hips = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.hips));
            var m_so_bs_spine = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.spine));
            var m_so_bs_chest = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.chest));
            var m_so_bs_leftShoulder = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.leftShoulder));
            var m_so_bs_leftElbow = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.leftElbow));
            var m_so_bs_leftHand = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.leftHand));
            var m_so_bs_rightShoulder = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.rightShoulder));
            var m_so_bs_rightElbow = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.rightElbow));
            var m_so_bs_rightHand = m_so_bs.FindPropertyRelative(nameof(UpperBodySetup.rightHand));

            m_so_bs_hips.objectReferenceValue = (Transform)_bodySetup.hips;
            m_so_bs_chest.objectReferenceValue = (Transform)_bodySetup.chest;
            m_so_bs_spine.objectReferenceValue = (Transform)_bodySetup.spine;

            m_so_bs_leftHand.objectReferenceValue = (Transform)_bodySetup.leftHand;
            m_so_bs_leftElbow.objectReferenceValue = (Transform)_bodySetup.leftElbow;
            m_so_bs_leftShoulder.objectReferenceValue = (Transform)_bodySetup.leftShoulder;

            m_so_bs_rightHand.objectReferenceValue = (Transform)_bodySetup.rightHand;
            m_so_bs_rightElbow.objectReferenceValue = (Transform)_bodySetup.rightElbow;
            m_so_bs_rightShoulder.objectReferenceValue = (Transform)_bodySetup.rightShoulder;

            ApplyModifiedProperties();
#endif
            base.AutoSetup(bodySetupContext, outLog);
        }

        internal override void Init(Transform origin)
        {
            chRelativeQuatSmooth = bodySetup.chest.rotation * Quaternion.Inverse(bodySetup.chestRotationOffset);
            
            if (hasSpine)
            {
                spRelativeQuatSmooth = bodySetup.spine.rotation * Quaternion.Inverse(bodySetup.spineRotationOffset);
            }

            headModule = (IKPModule_Head)ikp.GetModule(ModuleSignatures.HEAD);
            bodySetup.hipsRotationOffset = IKPUtils.GetRotationOffset(origin, bodySetup.hips);
            bodySetup.chestRotationOffset = IKPUtils.GetRotationOffset(origin, bodySetup.chest);

            if (hasSpine)
            {
                bodySetup.spineRotationOffset= IKPUtils.GetRotationOffset(origin, bodySetup.spine);
            }
            
            bodySetup.hipsChestOffset  = IKPUtils.GetRotationOffset(bodySetup.hips, bodySetup.chest);

            if (hasSpine)
            {
                bodySetup.hipsSpineOffset = IKPUtils.GetRotationOffset(bodySetup.hips, bodySetup.spine);
            }

            IKPLocalSpace _referencePoint = new IKPLocalSpace(origin.rotation);

            bodySetup.leftElbowLength = Vector3.Distance(bodySetup.leftShoulder.position, bodySetup.leftElbow.position);
            bodySetup.leftHandLength = Vector3.Distance(bodySetup.leftElbow.position, bodySetup.leftHand.position);

            IKPLocalSpace _lspShoulderElbow = IKPUtils.CalculateLimbLocalSpace(_referencePoint, bodySetup.leftShoulder.position, bodySetup.leftElbow.position);
            IKPLocalSpace _lspElbowHand = IKPUtils.CalculateLimbLocalSpace(_referencePoint, bodySetup.leftElbow.position, bodySetup.leftHand.position);

            Quaternion _lsQuat;
            _lsQuat = Quaternion.LookRotation(_lspShoulderElbow.forward, _lspShoulderElbow.up);
            bodySetup.leftShoulderRotationOffset = IKPUtils.GetRotationOffset(_lsQuat, bodySetup.leftShoulder.rotation);
            _lsQuat = Quaternion.LookRotation(_lspElbowHand.forward, _lspElbowHand.up);
            bodySetup.leftElbowRotationOffset = IKPUtils.GetRotationOffset(_lsQuat, bodySetup.leftElbow.rotation);
            bodySetup.leftHandRotationOffset = IKPUtils.GetRotationOffset(_lsQuat, bodySetup.leftHand.rotation);

            bodySetup.rightElbowLength = Vector3.Distance(bodySetup.rightShoulder.position, bodySetup.rightElbow.position);
            bodySetup.rightHandLength = Vector3.Distance(bodySetup.rightElbow.position, bodySetup.rightHand.position);

            _lspShoulderElbow = IKPUtils.CalculateLimbLocalSpace(_referencePoint, bodySetup.rightShoulder.position, bodySetup.rightElbow.position);
            _lspElbowHand = IKPUtils.CalculateLimbLocalSpace(_referencePoint, bodySetup.rightElbow.position, bodySetup.rightHand.position);
            _lsQuat = Quaternion.LookRotation(_lspShoulderElbow.forward, _lspShoulderElbow.up);
            bodySetup.rightShoulderRotationOffset = IKPUtils.GetRotationOffset(_lsQuat, bodySetup.rightShoulder.rotation);
            _lsQuat = Quaternion.LookRotation(_lspElbowHand.forward, _lspElbowHand.up);
            bodySetup.rightElbowRotationOffset = IKPUtils.GetRotationOffset(_lsQuat, bodySetup.rightElbow.rotation);
            bodySetup.rightHandRotationOffset = IKPUtils.GetRotationOffset(_lsQuat, bodySetup.rightHand.rotation);

        }

        public override bool Validate(List<ValidationResult> outValidationResult)
        {
            // draw some errors
            if (bodySetup != null)
            {
                ValidateCriticalBodySetupBone(bodySetup.hips, nameof(bodySetup.hips), outValidationResult);
                ValidateCriticalBodySetupBone(bodySetup.chest, nameof(bodySetup.chest), outValidationResult);

                if (hasSpine)
                {
                    ValidateCriticalBodySetupBone(bodySetup.spine, nameof(bodySetup.spine), outValidationResult);
                }

                if (bilatBase.hasLeft)
                {
                    ValidateCriticalBodySetupBone(bodySetup.leftHand, nameof(bodySetup.leftHand), outValidationResult);
                    ValidateCriticalBodySetupBone(bodySetup.leftElbow, nameof(bodySetup.leftElbow), outValidationResult);
                    ValidateCriticalBodySetupBone(bodySetup.leftShoulder, nameof(bodySetup.leftShoulder), outValidationResult);
                }
                
                if (bilatBase.hasRight)
                {
                    ValidateCriticalBodySetupBone(bodySetup.rightHand, nameof(bodySetup.rightHand), outValidationResult);
                    ValidateCriticalBodySetupBone(bodySetup.rightElbow, nameof(bodySetup.rightElbow), outValidationResult);
                    ValidateCriticalBodySetupBone(bodySetup.rightShoulder, nameof(bodySetup.rightShoulder), outValidationResult);
                }
            }

            if (GetProperty(p_lookSpeed) <= 0)
            {
                outValidationResult.Add(new ValidationResult()
                {
                    message = "Speed is 0",
                    outcome = ValidationResult.Outcome.Warning,
                });
            }

            return base.Validate(outValidationResult);
        }

#if UNITY_EDITOR

        public void EndHandCorrection()
        {
            SETUtil.EditorUtil.UnsubGUIDelegate();
            StopHandCorrectionSimulation();
            correctingHands = -1;
        }

        //SP
        protected override void InitializeSerializedProperties()
        {
            //init the serialized properties
            base.InitializeSerializedProperties();
            serialized.Update();

            upper_hasSpine = serialized.FindProperty(nameof(hasSpine));
            upper_useTargRot = serialized.FindProperty(nameof(useTargRot));
            upper_verticalLook = serialized.FindProperty(nameof(verticalLook));

            m_so_handsForwardCorrection = serialized.FindProperty(nameof(m_handsForwardCorrection));
            m_so_forcedPositioning = serialized.FindProperty(nameof(forcedPositioning));
            m_so_boneSetup = serialized.FindProperty(nameof(bodySetup));
        }
#endif

        //private methods:
        private void DrawHandCorrectionInspector(Side side)
        {
            //variables
#if UNITY_EDITOR
            InitSerializedPropertiesIfNeeded();
            
            {
                GUILayout.Label($"- {side} Hand Correction:", EditorStyles.boldLabel);
                m_so_handsForwardCorrection.vector2Value 
                    = new Vector2(
                        EditorGUILayout.FloatField("Forward Correction: ", m_handsForwardCorrection.x), m_handsForwardCorrection.y);
            
                Quaternion q = side == Side.Left ? m_leftHandRotationCorrection : m_rightHandRotationCorrection;
                GUILayout.Box("Current Rotation Correction:\n X: " + q.x + " Y: " + q.y + " Z: " + q.z + " W: " + q.w, GUILayout.MaxWidth(Screen.width));
            }

            ApplyModifiedProperties();

            var _isMissingEditorSim = !ikp.HasModule(ModuleSignatures.EDITOR_SIM);

            EditorGUI.BeginDisabledGroup(_isMissingEditorSim);
            {
                if (!isCurrentlyCorrectingHands)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button($"Correct {side} Hand Pose", GUILayout.Height(IKPStyle.MEDIUM_HEIGHT)))
                    {
                        StartHandCorrectionSimulation(side);
                        correctingHands = (int)side;
                    }

                    if (GUILayout.Button("Copy", GUILayout.ExpandWidth(false), GUILayout.Height(IKPStyle.MEDIUM_HEIGHT)))
                    {
                        if (side == Side.Left)
                        {
                            handsForwardCorrectionClipboard = handsForwardCorrectionL;
                            handRotationCorrectionClipboard = leftHandRotationCorrection;
                        }
                        else
                        {
                            handsForwardCorrectionClipboard = handsForwardCorrectionR;
                            handRotationCorrectionClipboard = rightHandRotationCorrection;
                        }
                    }

                    if (GUILayout.Button("Paste", GUILayout.ExpandWidth(false), GUILayout.Height(IKPStyle.MEDIUM_HEIGHT)))
                    {

                        if (side == Side.Left)
                        {
                            handsForwardCorrectionL = handsForwardCorrectionClipboard;
                            leftHandRotationCorrection = handRotationCorrectionClipboard;
                        }
                        else
                        {
                            handsForwardCorrectionR = handsForwardCorrectionClipboard;
                            rightHandRotationCorrection = handRotationCorrectionClipboard;
                        }
                    }

                    if (GUILayout.Button(new GUIContent(IKPStyle.refreshIcon, "Reset values"), GUILayout.Width(35), GUILayout.Height(IKPStyle.MEDIUM_HEIGHT)))
                    {
                        if (side == Side.Left)
                        {
                            handsForwardCorrectionL = 0f;
                            leftHandRotationCorrection = Quaternion.identity;
                        }
                        else
                        {
                            handsForwardCorrectionR = 0f;
                            rightHandRotationCorrection = Quaternion.identity;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("Save Pose", GUILayout.Height(IKPStyle.MEDIUM_HEIGHT)))
                    {
                        EndHandCorrection();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if (_isMissingEditorSim)
            {
                EditorGUILayout.HelpBox("[!] Editor Simulation Module required!", MessageType.Warning);
                
                if (GUILayout.Button(new GUIContent(" Add Editor Sim Module", ModuleManager.Linker(ModuleSignatures.EDITOR_SIM)?.icon), EditorStyles.miniButton))
                {
                    ikp.ToggleModule(ModuleSignatures.EDITOR_SIM, true);
                }
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Trigger editor simulation if module is present
        /// </summary>
        void StartHandCorrectionSimulation(Side side)
        {
            if (editorSimModule.isCurrentInstancePlaying)
            {
                StopHandCorrectionSimulation();
            }

            var _targetHand = side == Side.Left ? bodySetup.leftHand.position : bodySetup.rightHand.position;
            SceneView.lastActiveSceneView.Frame(new Bounds(_targetHand, Vector3.one));
            editorSimModule.Run();
        }

        void StopHandCorrectionSimulation()
        {
            editorSimModule.Stop();
        }
#endif

        float GetReach(float leftRc, float rightRc, float leftAgl, float rightAgl)
        {
            float r;
            float reachTreshold = 0.5f; //front - back

            float la = ReachCurve(leftAgl);
            float ra = ReachCurve(rightAgl);
            Vector2 rc = new Vector2(leftRc, rightRc) * reachTreshold;
            r = (rc.x + la) / (2f * (rc.y + ra));
            return r;
        }

        float ReachCurve(float angle)
        {
            Vector2 rCurveFlat = new Vector2(32f, 150f);
            float r;
            if (angle > rCurveFlat.x && angle < rCurveFlat.y) //hand is going backwards
                r = 0.5f;
            else if (angle <= rCurveFlat.x) //hand is going forward
                r = Mathf.Lerp(1f, 0.5f, angle / rCurveFlat.x);
            else
                r = Mathf.Lerp(0f, 0.5f, (180f - angle) / (180f - rCurveFlat.y));
            //else //hand is neutral
            r = Mathf.Clamp(r, 0f, 1f);
            return r;
        }
    }
}
