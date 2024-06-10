using UnityEngine;
using System.Reflection;
using SETUtil.Types;
using System;
using System.Text;
using UnityEngine.XR;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
    [IKPModule(ModuleSignatures.HEAD, displayName = "Head", inspectorOrder = 0, updateOrder = 50, iconPath = "IKP/ikp_head_icon")]
	[AddComponentMenu(IKPUtils.MODULE_COMPONENT_MENU + "/Head")]
	[DisallowMultipleComponent]
	public class IKPModule_Head : ModuleBase_LookAroundLogicSSP
	{
		//SETUtil.OrderedComponent:
		public override int OrderIndex()
		{
			return IKP.ORDER_INDEX + ModuleManager.GetInspectorOrder(ModuleSignatures.HEAD);
		}

		public enum Property
		{
			Weight,
			LookSpeed
		}

		//property names cache
		/// <summary>
		/// Cached property index
		/// </summary>
		public static int
			p_weight = (int)Property.Weight,
			p_lookSpeed = (int)Property.LookSpeed;
		//---

		//public:
		[HideInInspector, SerializeField, FormerlySerializedAs("_boneSetup")] private HeadSetup bodySetup;

		//private:
		[HideInInspector, SerializeField] private bool hasNeck = true;
		[HideInInspector, SerializeField] private IKPTarget ikpTarget = new IKPTarget();

		private Quaternion smoothRelativeLookQt;

#if UNITY_EDITOR
		SerializedProperty
			m_so_hasNeck,
			m_so_bs,
			m_so_bs_head,
			m_so_bs_neck,
			m_so_bs_chest,
			m_so_bs_headRotationOffset,
			m_so_bs_neckRotationOffset,
			m_so_bs_chestRotationOffset;
#endif

		//accessors:
		public Vector3 lookTarget
		{
			get { return ikpTarget.Get(ikp.origin).position; }
		}



		public override bool Validate(List<ValidationResult> outValidationResult)
		{
			ClampVariables();

			ValidateCriticalBodySetupBone(bodySetup.head, nameof(bodySetup.head), outValidationResult);

			if (hasNeck)
			{
				ValidateCriticalBodySetupBone(bodySetup.neck, nameof(bodySetup.neck), outValidationResult);
			}

			ValidateCriticalBodySetupBone(bodySetup.chest, nameof(bodySetup.chest), outValidationResult);

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

		public override void AutoSetup(BodySetupContext bodySetupContext, StringBuilder outLog)
		{
			HeadSetup _bodySetup = new HeadSetup();

			_bodySetup.head = PickBone(bodySetupContext, HumanBodyBones.Head, null, BoneNamesLibrary.head);
			_bodySetup.neck = PickBone(bodySetupContext, HumanBodyBones.Neck, null, BoneNamesLibrary.neck);
			_bodySetup.chest = PickBone(bodySetupContext, HumanBodyBones.Chest, null, BoneNamesLibrary.chest);

			bodySetup = _bodySetup;

			if (GetProperty(p_lookSpeed) == 0) SetProperty(p_lookSpeed, 5f);

#if UNITY_EDITOR
			InitSerializedPropertiesIfNeeded();

			m_so_bs_head.objectReferenceValue = (Transform)_bodySetup.head;
			m_so_bs_chest.objectReferenceValue = (Transform)_bodySetup.chest;

			if (hasNeck)
			{
				m_so_bs_neck.objectReferenceValue = (Transform)_bodySetup.neck;
			}

			ApplyModifiedProperties();
#endif
			base.AutoSetup(bodySetupContext, outLog);
		}

		internal override void Init(Transform origin)
		{
			smoothRelativeLookQt = bodySetup.head.rotation * Quaternion.Inverse(bodySetup.headRotationOffset);

			bodySetup.headRotationOffset = IKPUtils.GetRotationOffset(origin, bodySetup.head);
			bodySetup.chestRotationOffset = IKPUtils.GetRotationOffset(origin, bodySetup.chest);

			if (hasNeck)
			{
				bodySetup.neckRotationOffset = IKPUtils.GetRotationOffset(origin, bodySetup.neck);
			}
		}


#if UNITY_EDITOR
		protected override void InitializeSerializedProperties()
		{ //init the serialized properties
			base.InitializeSerializedProperties();
			serialized.Update();
			m_so_hasNeck = serialized.FindProperty(nameof(hasNeck));

			m_so_bs = serialized.FindProperty(nameof(bodySetup));

			m_so_bs_chest = m_so_bs.FindPropertyRelative(nameof(HeadSetup.chest));
			m_so_bs_neck = m_so_bs.FindPropertyRelative(nameof(HeadSetup.neck));
			m_so_bs_head = m_so_bs.FindPropertyRelative(nameof(HeadSetup.head));
			m_so_bs_headRotationOffset = m_so_bs.FindPropertyRelative(nameof(HeadSetup.headRotationOffset));
			m_so_bs_neckRotationOffset = m_so_bs.FindPropertyRelative(nameof(HeadSetup.neckRotationOffset));
			m_so_bs_chestRotationOffset = m_so_bs.FindPropertyRelative(nameof(HeadSetup.chestRotationOffset));
		}
#endif

		public override ExecutionFlag IKPUpdate()
		{
			if (base.IKPUpdate() == ExecutionFlag.Break)
			{
				return ExecutionFlag.Break;
			}

			ClampVariables();

			IKPLocalSpace ikpLsp = new IKPLocalSpace(bodySetup.chest.rotation * Quaternion.Inverse(bodySetup.chestRotationOffset));
			Vector3 _targetPos = ikpTarget.Get(ikp.origin).position;
			Vector3 _targetVec = IKPUtils.NormalVector(bodySetup.head.position, _targetPos);
			float _angl = Vector3.Angle(_targetVec, ikpLsp.forward);

			//target vector clamp to angle limit
			if (trackTargetOnBack)
			{
				if (_angl >= angleLimit / 2f)
					_targetVec = IKPUtils.LimitedAngle(ikpLsp, angleLimit, _targetVec);
			}
			else
			{
				if (_angl > angleLimit / 2f)
					_targetVec = ikpLsp.forward;
			}

			Quaternion
				_lookQuat = Quaternion.LookRotation(_targetVec, ikpLsp.up),
				_relativeLookQuat = IKPUtils.GetRotationOffset(bodySetup.chest.rotation, _lookQuat);

			smoothRelativeLookQt = Quaternion.Lerp(smoothRelativeLookQt, _relativeLookQuat, GetProperty(p_lookSpeed) * Time.deltaTime);
			Quaternion _weightedOrientation = Quaternion.Lerp(bodySetup.head.rotation, bodySetup.chest.rotation * smoothRelativeLookQt * bodySetup.headRotationOffset, GetProperty(p_weight));

			Vector3 _headFwd = _weightedOrientation * Quaternion.Inverse(bodySetup.headRotationOffset) * Vector3.forward;
			float _headAngle = Vector3.Angle(_headFwd, ikpLsp.forward);

			if (forcedAngleClamp && _headAngle > angleLimit / 2f) //clamp rotation to angle limit
				_weightedOrientation = _lookQuat * bodySetup.headRotationOffset;

			if (hasNeck)
			{
				Quaternion _neckRot = Quaternion.Lerp(bodySetup.chest.rotation * Quaternion.Inverse(bodySetup.chestRotationOffset) * bodySetup.neckRotationOffset, _weightedOrientation, 0.5f);
				bodySetup.neck.rotation = _neckRot;
			}
			bodySetup.head.rotation = _weightedOrientation;

			return ExecutionFlag.Continue;
		}

		public void SetTarget(Vector3 position, Relative relativeMode = Relative.World)
		{
			ikpTarget.Set(position, Quaternion.identity, relativeMode);
		}

		public void SetTarget(Transform tr)
		{
			ikpTarget.Set(tr);
		}

		public void SetTarget(IKPTarget tgt)
		{
			ikpTarget = tgt;
		}

		public IKPTarget GetIKPTarget()
		{
			return ikpTarget;
		}

		internal Vector3 GetHeadPosition()
		{
			return bodySetup.head.position;
		}

		private void ClampVariables()
		{
			angleLimit = Mathf.Clamp(angleLimit, 0f, 179f);
			if (GetProperty(p_lookSpeed) < 0f)
				SetProperty(p_lookSpeed, 0f);
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (!Validate())
				return;

			GizmoPalette g1 = active ? GizmoPalette.Pink : GizmoPalette.White;
			if (hasNeck)
			{
				IKPEditorUtils.PaintBone(bodySetup.chest.position, bodySetup.neck.position, g1);
				IKPEditorUtils.PaintBone(bodySetup.neck.position, bodySetup.head.position, g1, true);
			}
			else
				IKPEditorUtils.PaintBone(bodySetup.chest.position, bodySetup.head.position, g1, true);
		}
        
		public override void DrawEditorGUI()
		{
			base.DrawEditorGUI();
			IKPEditorUtils.DrawTargetGUI(serialized.FindProperty(nameof(ikpTarget)));
		}

		protected override void DrawSetup()
		{
			base.DrawSetup();
			EditorGUILayout.PropertyField(m_so_bs_chest);
			EditorGUILayout.PropertyField(m_so_bs_head);
			EditorGUILayout.PropertyField(m_so_bs_neck);

			if (!bodySetup.head)
			{
				EditorGUILayout.HelpBox("Head bone missing!", MessageType.Error);
			}

			if (!bodySetup.neck && hasNeck)
			{
				EditorGUILayout.HelpBox("Neck bone missing!", MessageType.Error);
			}

			if (!bodySetup.chest)
			{
				EditorGUILayout.HelpBox("Chest bone missing!", MessageType.Error);
			}
		}

		protected override void DrawSettings()
		{
			base.DrawSettings();
			m_so_hasNeck.boolValue = GUILayout.Toggle(m_so_hasNeck.boolValue, "Has Neck");
		}

		protected override void DrawProperties()
		{
			DrawPropertyGUI(p_weight, true);
			DrawPropertyGUI(p_lookSpeed);
		}
#endif
	}
}
