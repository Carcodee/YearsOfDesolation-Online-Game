using UnityEngine;
using IKPn;
using SETUtil.Extend;
using SETUtil.Types;
using UnityEngine.Serialization;
using System.Text;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
    [IKPModule(ModuleSignatures.WEAPON, inspectorOrder = 30, updateOrder = 30, iconPath = "IKP/ikp_weapon_icon")]
	[AddComponentMenu(IKPUtils.MODULE_COMPONENT_MENU + "/Weapon")]
	[DisallowMultipleComponent]
	public class IKPModule_Weapon : ModuleBase_LookAroundLogicSSP
	{
		//SETUtil.OrderedComponent:
		public override int OrderIndex()
		{
			return IKP.ORDER_INDEX + ModuleManager.GetInspectorOrder(ModuleSignatures.WEAPON);
		}

		public enum Property
		{
			Weight,
			WeightRatio,
			LookSpeed,
			ForwardOffset,
			Tilt
		}

		//property names cache
		/// <summary>
		/// Cached property index
		/// </summary>
		public static int
			p_weight         = (int)Property.Weight,
			p_weightRatio    = (int)Property.WeightRatio,
			p_lookSpeed      = (int)Property.LookSpeed,
			p_forwardOffset  = (int)Property.ForwardOffset,
			p_tilt           = (int)Property.Tilt;
		//---

		protected override bool requiresBoneRotationOffsetValues => false;

		//public:
		[SerializeField, FormerlySerializedAs("Weapon")] private Transform _weapon;
		public Transform weapon
		{
			get {
				return _weapon;
			}

			set{
				_weapon = value;
				weaponCollisionRecord = new WeaponCollisionRecord(_weapon);

				if(ikpWeaponComponent){
					Quaternion _hRot = ikpWeaponComponent.GetHandRotation(handSide);
					handleRotationOffset = IKPUtils.GetRotationOffset(_hRot, weapon.rotation);
				}else{
					handleRotationOffset = Quaternion.identity;
				}
			}
		}

		[HideInInspector]
		public float weaponLiftHeight = 0.35f;

		//private:
		[HideInInspector, SerializeField]
		private IKPAimTarget aimTarget = new IKPAimTarget();

		private Quaternion handleRotationOffset = Quaternion.identity;

		private TransformData smoothOffset = new TransformData(Vector3.zero, Quaternion.identity);

		[HideInInspector, SerializeField]
		private Side handSide = Side.Right;

		private WeaponCollisionRecord _weaponCollisionRecord;
		internal WeaponCollisionRecord weaponCollisionRecord {
			get {
				if(_weaponCollisionRecord == null){
					_weaponCollisionRecord = new WeaponCollisionRecord(_weapon);
				}

				return _weaponCollisionRecord;
			}
			private set { _weaponCollisionRecord = value; }
		}
		
		private IKP_Weapon ikpWeaponComponent { 
			get{ 
				return weaponCollisionRecord != null ? weaponCollisionRecord.ikpWeapon : null; 
			} 
		}

		private IKPModule_Head headModule;
		private IKPModule_UpperBody upperBodyModule;
		private IKPModule_WeaponCollision weaponCollisionModule;


		internal override void Init(Transform origin)
		{
			if (ikp.HasModule(ModuleSignatures.HEAD))
				headModule = ikp.GetModule(ModuleSignatures.HEAD) as IKPModule_Head;
			if (ikp.HasModule(ModuleSignatures.UPPER_HUMANOID))
				upperBodyModule = ikp.GetModule(ModuleSignatures.UPPER_HUMANOID) as IKPModule_UpperBody;
			if (ikp.HasModule(ModuleSignatures.WEAPON_COLLISION))
				weaponCollisionModule = ikp.GetModule(ModuleSignatures.WEAPON_COLLISION) as IKPModule_WeaponCollision;
		}

		internal void SetWeapon (Transform weapon)
		{
			this.weapon = weapon;

#if UNITY_EDITOR
			using (var _so = new SerializedObject(this)) {
				var _p = _so.FindProperty(nameof(weapon));
				_p.objectReferenceValue = weapon;
				_so.ApplyModifiedProperties();
			}
#endif
		}

		public override ExecutionFlag IKPUpdate()
		{
			if (base.IKPUpdate() == ExecutionFlag.Break || weapon == null || upperBodyModule == null)
				return ExecutionFlag.Break;

			Transform _origin = ikp.origin;

			//Moved from IKP (26.08.2018)
			float _reach = upperBodyModule.GetMaxLimbStretch(handSide);
			Vector3
				_pivot = upperBodyModule.GetPivot(handSide),
				_weaponTarget;
			Quaternion _chestBetaRot = upperBodyModule.GetChestRotation();
			TransformData _primaryHandTrDt = upperBodyModule.GetLimbTransformData(handSide, true);

			if (aimTarget.mode == IKPAimTargetMode.TargetInheritance
				&& headModule != null)
				_weaponTarget = headModule.GetIKPTarget().Get(_origin).position;
			else
				_weaponTarget = aimTarget.Get(_origin).position;

			//LERP_1f (WEAPON):
			Vector3 targetVector = IKPUtils.NormalVector(_pivot, _weaponTarget);
			IKPLocalSpace chestLsp = new IKPLocalSpace(_chestBetaRot);

			if (!trackTargetOnBack)
				if (Vector3.Angle(targetVector, chestLsp.forward) >= angleLimit / 2f)
					targetVector = IKPUtils.LimitedAngle(chestLsp, angleLimit, targetVector);

			//calculate look direction
			IKPLocalSpace weaponLsp = IKPUtils.CalculateLimbLocalSpace(chestLsp, targetVector);

			float _tilt = GetProperty(p_tilt);
			Vector3 _weaponUp = IKPUtils.Circle(new ProjectionPlane(weaponLsp.right, weaponLsp.up), _tilt);

			//calculate short base offset vector
			float _liftFactor = AngleLerp(_weaponUp, chestLsp.up);
			Vector3 _shortBase = _pivot + chestLsp.up * weaponLiftHeight * _liftFactor;
			float
				_stockOffset = 0f, // weaponScript -> weaponDimension
				_weaponOffset = 1f;

			if (ikpWeaponComponent != null)
			{
				_weaponOffset = ikpWeaponComponent.GetForwardOffset();
				_stockOffset = ikpWeaponComponent.GetStockOffset();
			}
			float _forwardOffset = GetProperty(p_forwardOffset) * _weaponOffset;

			TransformData _orientation = new TransformData(_shortBase + targetVector * (_forwardOffset * _reach - _stockOffset),
				Quaternion.LookRotation(targetVector, _weaponUp));

			_orientation = IKPUtils.CalculateSmoothRelative(ref smoothOffset, _orientation, _origin, GetProperty(p_lookSpeed));

			//LERP_0f (HANDS):	
			if (ikpWeaponComponent)
			{ //if there's a weapon script that holds offset values, then apply that offset value
				_primaryHandTrDt.position -= ikpWeaponComponent.GetHandOffset(handSide);
				_primaryHandTrDt.rotation *= handleRotationOffset; //subtract the rotation difference
			}

			//LERP:
			float
				_weightRatio = GetProperty(p_weightRatio), //weight ratio lerp value
				_weight = GetProperty(p_weight);

			_orientation = TransformData.Lerp(_primaryHandTrDt, _orientation, _weightRatio);
			weapon.Set(TransformData.Lerp(weapon.ToTransformData(), _orientation, _weight));

			//weapon collision 
			if (weaponCollisionModule != null && weaponCollisionModule.IsActive()) {
				weaponCollisionModule.ApplyCollision(weaponCollisionRecord);
			}

			//re-calculate the hands
			for (uint i = 0; i < 2; i++) {
				//loop the sides
				TransformData _handTrDt = GetLimbTransformData((Side)i);

				_handTrDt.position -= upperBodyModule.ForwardOffset((Side)i);
				if ((Side)i != handSide) //set the offhand target to it's original source
					_handTrDt = TransformData.Lerp(upperBodyModule.GetLimbTransformData((Side)i, true), _handTrDt, GetProperty(p_weightRatio));
				
				upperBodyModule.SetTempLimbTargetInternal((Side)i, _handTrDt);
			}

			return ExecutionFlag.Continue;
		}


		internal void SetTarget(Vector3 targ, Relative relativeMode = Relative.World)
		{
			aimTarget.Set(targ, Quaternion.identity, relativeMode);
			aimTarget.mode = IKPAimTargetMode.NewTarget;
		}

		internal void SetTarget(Transform transform)
		{
			SetTarget(new IKPTarget(transform));
		}

		internal void SetTarget(IKPTarget ikpTarget)
		{
			aimTarget = (IKPAimTarget)ikpTarget;
			aimTarget.mode = IKPAimTargetMode.NewTarget;
		}

		internal void SetAimTarget(IKPAimTarget aimTarget)
		{
			aimTarget.mode = IKPAimTargetMode.NewTarget;
			this.aimTarget.Copy(aimTarget);
		}

		internal void SetAimTargetMode(IKPAimTargetMode aimTargetMode)
		{
			this.aimTarget.mode = aimTargetMode;
		}

		internal void SetHandSide(Side handSide)
		{
			this.handSide = handSide;
		}

		public Transform GetWeaponTransform()
		{
			return weapon;
		}

		public TransformData GetLimbTransformData(Side side)
		{
			if (!weapon) {
				Debug.LogError("[ERROR] No weapon found! Please assign a weapon.");
				return new TransformData();
			}

			if (ikpWeaponComponent) {
				return new TransformData(ikpWeaponComponent.GetHandPosition(side), ikpWeaponComponent.GetHandRotation(side));
			}

			return new TransformData(weapon); // if there's no weapon script, return fallback value
		}

		public IKPAimTarget GetAimTarget()
		{
			return aimTarget;
		}

		public override Transform[] CollectTransformDependencies()
		{
			Transform[] _dep = new Transform[] { weapon };
			return _dep;
		}

        public override bool Validate(List<ValidationResult> outValidationResult)
        {
			if (aimTarget == null)
			{
                aimTarget = new IKPAimTarget();
            }
            
			if (!weapon)
			{
				outValidationResult.Add(new ValidationResult()
				{
					message = $"{SETUtil.StringUtil.WordSplit(nameof(weapon))} is not assigned!",
					outcome = ValidationResult.Outcome.Warning,
				});
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

        public override void AutoSetup(BodySetupContext bodySetupContext, StringBuilder outLog)
		{
			if (GetProperty(p_lookSpeed) == 0) SetProperty(p_lookSpeed, 5f);
			base.AutoSetup(bodySetupContext, outLog);
        }

#if UNITY_EDITOR
        public override void DrawEditorGUI()
		{
			base.DrawEditorGUI();

			DrawAimTargetGUI();
		}

		protected override void DrawSetup()
		{
			base.DrawSetup();

			var m_so_weapon = serialized.FindProperty(nameof(_weapon));
			EditorGUILayout.PropertyField(m_so_weapon, new GUIContent("Weapon", "[IKPModule_Weapon.weapon]\nWeapon transform. Can be set in runtime (check the documentation).\nIKP_Weapon script is strongly advised."));

		}

		protected override void DrawSettings()
		{
			base.DrawSettings();

			SerializedProperty
				m_so_handSide = serialized.FindProperty(nameof(handSide)),
				m_so_weaponLiftHeight = serialized.FindProperty(nameof(weaponLiftHeight));

			EditorGUILayout.PropertyField(m_so_weaponLiftHeight, new GUIContent("Weapon Lift Height", "[IKPModule_Weapon.weaponLiftHeight]\nHow much does the character lift the weapon over his shoulder when trying to aim at a target behind him"));
			EditorGUILayout.PropertyField(m_so_handSide, new GUIContent("Hand Side", "[IKPModule_Weapon.handSide]\nWhich side should the weapon be held at"));
			
		}

		protected override void DrawProperties()
		{
			base.DrawProperties();
			GUILayout.Label("Main Properties:", EditorStyles.boldLabel);
			string[] pptNms = IKPUtils.GetPropertyNames(this);
			for (int i = 0; i < pptNms.Length; i++)
			{
				if (i != p_lookSpeed)
					DrawPropertyGUI(i, true);
			}

			GUILayout.Label("Other Properties:", EditorStyles.boldLabel);
			DrawPropertyGUI(p_lookSpeed);
		}

		public void DrawAimTargetGUI()
		{
			const string _targName = nameof(aimTarget);

			InitSerializedPropertiesIfNeeded();
			serialized.Update();

			SerializedProperty _targ_aimTargetMode = serialized.FindDeepProperty(string.Format("{0}/{1}", _targName, "mode"));

			GUI.color = IKPStyle.COLOR_TARGET;
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.Label(SETUtil.StringUtil.WordSplit(_targName, true), EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_targ_aimTargetMode);
			ApplyModifiedProperties();

			if (_targ_aimTargetMode.enumValueIndex == (int)IKPAimTargetMode.NewTarget)
			{
				IKPEditorUtils.DrawTargetGUI(serialized.FindProperty(_targName));
			}
			else
			{
				GUI.color = IKPStyle.COLOR_ACTIVE;
				GUILayout.Label("Will inherit from Head Target", EditorStyles.helpBox);
			}
			GUILayout.EndVertical();
		}
	#endif

		float AngleLerp(Vector3 a, Vector3 b, float _base = 90)
		{
			return Vector3.Angle(a, b) / _base;
		}
    }
}
