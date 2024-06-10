using UnityEngine;

using TransformData = SETUtil.Types.TransformData;
using EditorUtil = IKPn.IKPEditorUtils;
using System.Text;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace IKPn
{
    /// <summary>
    /// Handles weapon collision effects
    /// </summary>
    [IKPModule(ModuleSignatures.WEAPON_COLLISION, inspectorOrder = 35, updateOrder = 40, iconPath = "IKP/ikp_weaponcollision_icon")]
	[AddComponentMenu(IKPUtils.MODULE_COMPONENT_MENU + "/Weapon Collision")]
	[DisallowMultipleComponent]
	public class IKPModule_WeaponCollision : ModuleBase_StandardBoneLogicSSP
	{
		/// <summary>
		/// interface SETUtil.iOrderedComponent
		/// </summary>
		public override int OrderIndex()
		{
			return IKP.ORDER_INDEX + ModuleManager.GetInspectorOrder(ModuleSignatures.WEAPON_COLLISION);
		}

		public enum Property
		{
			ReactionSpeed
		}

		//property names cache
		/// <summary>
		/// Cached property index
		/// </summary>
		public static int
			p_reactionSpeed = (int)Property.ReactionSpeed;
		//--

		public enum UseWeapon
		{
			FromWeaponModule,
			OriginalTransform
		}

		public enum CollisionBehavior
		{
			Procedural,
			GoToPose
		}

		public enum PushDirection
		{
			Up,
			Down,
			Dynamic,
		}


		protected override bool requiresBoneRotationOffsetValues => false;

        [SerializeField] private Transform weapon;

		[HideInInspector]
		[SerializeField]
		private float
			defaultWeaponLength = .3f,
			defaultWeaponWidth = .1f;

		[HideInInspector] [SerializeField] private UseWeapon useWeapon = UseWeapon.FromWeaponModule;
		[HideInInspector] [SerializeField] private CollisionBehavior collisionBehavior = CollisionBehavior.Procedural;
		[HideInInspector] [SerializeField] private PushDirection pushDirection = PushDirection.Down;
		[HideInInspector] [SerializeField] private IKPTarget collisionPose = new IKPTarget();

		private IKPModule_Weapon weaponModule = null;
		private IKPModule_UpperBody upperModule = null;

		private WeaponCollisionRecord _weaponCollisionRecord;
		private WeaponCollisionRecord weaponCollisionRecord
		{
			get
			{
				if (useWeapon == UseWeapon.FromWeaponModule && weaponModule != null)
				{
					// use weapon module record
					return weaponModule.weaponCollisionRecord;
				}

				// use local
				if (_weaponCollisionRecord == null || _weaponCollisionRecord.transform != weapon)
				{
					_weaponCollisionRecord = new WeaponCollisionRecord(weapon);
				}

				return _weaponCollisionRecord;
			}
		}

		internal override void Init(Transform origin)
		{
			weaponModule = (IKPModule_Weapon)ikp.GetModule(ModuleSignatures.WEAPON);
			upperModule = (IKPModule_UpperBody)ikp.GetModule(ModuleSignatures.UPPER_HUMANOID);
		}

		public override ExecutionFlag IKPUpdate()
		{
			if (weaponModule)
			{
				if (useWeapon == UseWeapon.FromWeaponModule)
					weapon = weaponModule.GetWeaponTransform();
			}

			if (base.IKPUpdate() == ExecutionFlag.Break || weapon == null)
			{
				return ExecutionFlag.Break;
			}

			if (!weaponCollisionRecord.invokedThisFrame)
			{
				/*As IKP_Weapon_Module's IKPUpdate is called before this module,
					invokedThisFrame would only be true if the Calculate method was called externally.*/
				ApplyCollision(weaponCollisionRecord); //autonomous collision processing, in case IKPModule_Weapon is missing

				if (upperModule != null)
				{
					upperModule.Calculate(IKPModule_UpperBody.c_hands);
				}
			}

			weaponCollisionRecord.invokedThisFrame = false; //reset the value for the next frame
			return ExecutionFlag.Continue;
		}

		public override bool Validate(List<ValidationResult> outValidationResult)
		{
			if (useWeapon == UseWeapon.OriginalTransform && !weapon)
			{
				outValidationResult.Add(new ValidationResult()
				{
					message = $"{SETUtil.StringUtil.WordSplit(nameof(weapon))} is not assigned!",
					outcome = ValidationResult.Outcome.Warning,
				});
			}


			if (GetProperty(p_reactionSpeed) <= 0)
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

		protected override void DrawSetup()
		{
			base.DrawSetup();
			
			var m_so_weapon = serialized.FindProperty(nameof(weapon));
			var m_so_useWeapon = serialized.FindProperty(nameof(useWeapon));
			
			UnityEditor.EditorGUILayout.PropertyField(m_so_useWeapon, new GUIContent("Use Weapon", "IKPModule_WeaponCollision.useWeapon"));
			if (useWeapon == UseWeapon.OriginalTransform)
				UnityEditor.EditorGUILayout.PropertyField(m_so_weapon, new GUIContent("Weapon", "[IKPModule_Weapon.weapon]\nThe transform that will be modified.\n( A configured IKP_Weapon component is advised)"));

		}

		protected override void DrawSettings()
		{
			base.DrawSettings();

			UnityEditor.SerializedProperty
				m_so_collisionBehavior = serialized.FindProperty(nameof(collisionBehavior)),
				m_so_pushDirection = serialized.FindProperty(nameof(pushDirection));


			UnityEditor.EditorGUILayout.PropertyField(m_so_collisionBehavior, new GUIContent("Collision Behavior", "IKPModule_WeaponCollision.collisionBehavior"));
			if (collisionBehavior == CollisionBehavior.Procedural)
			{
				UnityEditor.EditorGUILayout.PropertyField(m_so_pushDirection, new GUIContent("Push Direction", "IKPModule_WeaponCollision.pushDirection"));
			}
			else if (collisionBehavior == CollisionBehavior.GoToPose)
			{
				IKPEditorUtils.DrawTargetGUI(serialized.FindProperty(nameof(collisionPose)));
			}
		}

		protected override void DrawProperties()
		{
			base.DrawProperties();
			DrawPropertyGUI(p_reactionSpeed);
		}
#endif

		public override Transform[] CollectTransformDependencies()
		{
			if (weapon)
			{
				Transform[] _dep = new Transform[1];
				_dep[0] = weapon;
				return _dep;
			}
			return new Transform[0];
		}

        public override void AutoSetup(BodySetupContext bodySetupContext, StringBuilder outLog)
		{
			if (GetProperty(p_reactionSpeed) == 0) SetProperty(p_reactionSpeed, 5f);
			base.AutoSetup(bodySetupContext, outLog);
        }

        internal void SetWeapon(Transform weapon)
		{
			this.weapon = weapon;

#if UNITY_EDITOR
			using (var _so = new SerializedObject(this))
			{
				var _p = _so.FindProperty(nameof(weapon));
				_p.objectReferenceValue = weapon;
				_so.ApplyModifiedProperties();
			}
#endif
		}

		internal void ApplyCollision(WeaponCollisionRecord record, float? currentForwardOffset = null)
		{
			if (!IsActive())
			{
				return;
			}

			Transform _rcTransform = record.transform;

			// roll back pose if no external modifications to the weapon pose were done
			var _weaponLocalTrDt = new TransformData(_rcTransform.localPosition, _rcTransform.localRotation);
			if (_weaponLocalTrDt.position == record.modifiedState.position && _weaponLocalTrDt.rotation == record.modifiedState.rotation)
			{
                // no modifications were done
                _rcTransform.localPosition = record.persistentState.position;
                _rcTransform.localRotation = record.persistentState.rotation;
			}
			else
			{
				record.persistentState = _weaponLocalTrDt;
			}

			var _ikpWeapon = record.ikpWeapon;
			bool _hasIkpWeapon = (_ikpWeapon != null);

			float _stockOffset = _hasIkpWeapon ? _ikpWeapon.GetStockOffset() : 0;
            float _castDiameter = _hasIkpWeapon ? _ikpWeapon.GetWidth() : defaultWeaponWidth;
			float _castRadius = _castDiameter / 2;
			float _weaponLength = _hasIkpWeapon ? _ikpWeapon.GetLength() : defaultWeaponLength;
			float _castDistance = _weaponLength + _castDiameter;
			float _raycastPointOffset = _castDiameter - _stockOffset;

			var _raycastMask = ikp.raycastingMask;
			var _reactionSpeed = GetProperty(p_reactionSpeed);

			RaycastHit _hitInfo;

			Vector3 _raycastPoint = _rcTransform.position - _rcTransform.forward * _raycastPointOffset;
			bool _hasHit = Physics.SphereCast(_raycastPoint, _castRadius, _rcTransform.forward, out _hitInfo, _castDistance, _raycastMask);

            TransformData _targetTrDt;
            if (_hasHit)
			{
				if (collisionBehavior == CollisionBehavior.GoToPose)
				{
					Vector3 _localPos;
					var _poseTrDt = collisionPose.Get(ikp.origin);

					if (_rcTransform.parent != null)
					{
						var _worldToLocal = _rcTransform.parent.worldToLocalMatrix;
						_localPos = _worldToLocal.MultiplyPoint(_poseTrDt.position);
					}
					else
					{
						_localPos = _poseTrDt.position;
					}

					var _localRot = Quaternion.Inverse(_rcTransform.rotation) * _poseTrDt.rotation;
					_targetTrDt = new TransformData(_localPos, _localRot);
				}
				else
				{
					var _pushBack = 1 - IKPUtils.Pw2(_hitInfo.distance / (_weaponLength + _castDiameter));
					var _dir = record.chosenPushDirection;

					if (record.isNeutralState)
					{
						if (pushDirection == PushDirection.Up)
						{
							_dir = Vector3.up;
						}
						else if (pushDirection == PushDirection.Down)
						{
							_dir = Vector3.down;
						}
						else if (pushDirection == PushDirection.Dynamic)
						{
							RaycastHit _hitUp;
							RaycastHit _hitDown;

							float _distanceUp = float.MaxValue;
							float _distanceDown = float.MaxValue;

							// check up
							if (Physics.Raycast(_raycastPoint, (_rcTransform.forward + _rcTransform.up), out _hitUp, _castDistance, _raycastMask))
							{
								_distanceUp = _hitUp.distance;
							}

							// check down
							if (Physics.Raycast(_raycastPoint, (_rcTransform.forward - _rcTransform.up), out _hitDown, _castDistance, _raycastMask))
							{
								_distanceDown = _hitDown.distance;
							}

							if (_distanceUp > _distanceDown)
							{
								_dir = Vector3.up;
							}
							else
							{
								_dir = Vector3.down;
							}
						}

						record.chosenPushDirection = _dir;
					}

					var _worldDir = _rcTransform.rotation * _dir;
					_targetTrDt = new TransformData(
						_rcTransform.localPosition,
						Quaternion.Lerp(
							Quaternion.identity,
							Quaternion.Inverse(_rcTransform.rotation) * Quaternion.LookRotation(_worldDir, Vector3.Cross(_worldDir, _rcTransform.right)),
							_pushBack)
					);
				}
			}
			else
			{
				_targetTrDt = new TransformData(record.persistentState.position, Quaternion.Inverse(_rcTransform.localRotation) * record.persistentState.rotation);
			}

			record.dampedState = TransformData.Lerp(record.dampedState, _targetTrDt, Time.deltaTime * _reactionSpeed);

			// apply position and rotation
			var _modifiedPos = record.dampedState.position;

			if (_rcTransform.parent != null)
			{
				_modifiedPos = _rcTransform.parent.localToWorldMatrix.MultiplyPoint(_modifiedPos);
			}

			_rcTransform.position = _modifiedPos;
			_rcTransform.rotation *= record.dampedState.rotation;

			record.modifiedState = new TransformData(_rcTransform.localPosition, _rcTransform.localRotation);
			record.isNeutralState = !_hasHit;
			record.invokedThisFrame = true;
		}
    }
}
