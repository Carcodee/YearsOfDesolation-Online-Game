// IKP - by Hristo Ivanov (Kris Development)

using UnityEngine;
using SETUtil.Types;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
	[RequireComponent(typeof(IKP))]
	public abstract class IKPModule : MonoBehaviour, iOrderedComponent
	{
		///<summary>
		/// SETUtil.OrderedComponent
		///</summary>
		public virtual int OrderIndex()
		{
			return IKP.ORDER_INDEX + ModuleManager.MODULE_ORDER_BUFFER + 1;
		}

		[SerializeField, HideInInspector] protected bool
			active = true,
			expandGUI = true;

		private IKP m_ikp;

		protected IKP ikp
		{
			get
			{
				if (m_ikp == null) {
					m_ikp = GetComponent<IKP>();
				}

				return m_ikp;
			}
		}

#if UNITY_EDITOR
		[NonSerialized] 
		protected SerializedObject
			serialized;

		[NonSerialized]
		protected SerializedProperty
			m_so_active,
			m_so_expandGUI;
#endif

		/// <summary>
		/// Checks if the module is valid and able to run. No params, this is in case you don't care about output message.
		/// </summary>
		public bool Validate()
        {
			return Validate(new List<ValidationResult>());
        }

		/// <summary>
		/// Checks if the module is valid and able to run. Outs a list of validation errors and warnings.
		/// </summary>
		public virtual bool Validate(List<ValidationResult> outValidationResult)
		{
			return outValidationResult == null || !outValidationResult.Any(a => a.outcome == ValidationResult.Outcome.CriticalError);
		}

		public void SetActive(bool state)
		{
			//toggle the modules
			bool editor = false;
#if UNITY_EDITOR
			editor = (!Application.isPlaying);
			InitSerializedPropertiesIfNeeded();
			serialized.Update();
			m_so_active.boolValue = state;
			ApplyModifiedProperties();
#endif

			if (!editor) //if the script is called in a standalone build
				active = state;
		}

		/// <summary>
		/// the bool 'active' is set via Serialzed Object interactions 
		/// so it can't just be a property with private setter. Use this method to read it instead.
		/// </summary>
		public bool IsActive()
		{
			return active;
		}

		/// <summary>
		/// Invoked before animations are applied (from MonoBehaviour.Update)
		/// </summary>
		public virtual ExecutionFlag IKPPreUpdate()
		{
			if (!IsActive())
			{
				return ExecutionFlag.Break;
			}

			return ExecutionFlag.Continue;
		}

		/// <summary>
		/// Perform IK actions during MonoBehaviour.LateUpdate
		/// </summary>
		public virtual ExecutionFlag IKPUpdate()
		{
			if (!IsActive())
			{
				return ExecutionFlag.Break;
			}

			return ExecutionFlag.Continue;
		}

#if UNITY_EDITOR
		public virtual void DrawEditorGUI()
        {
			InitSerializedPropertiesIfNeeded();
			DrawValidation();
		}

		protected void DrawValidation()
		{
			var _list = new List<ValidationResult>();
			Validate(_list);

			foreach (ValidationResult validationResult in _list)
			{
				switch (validationResult.outcome)
				{
					case ValidationResult.Outcome.Valid:
						break;
					case ValidationResult.Outcome.Warning:
						EditorGUILayout.HelpBox(validationResult.message, MessageType.Warning);
						break;
					case ValidationResult.Outcome.CriticalError:
						EditorGUILayout.HelpBox(validationResult.message, MessageType.Error);
						break;
				}
			}
		}
#endif

		/// <summary>
		/// Apply the active serialized object's modified properties
		/// </summary>
		protected void ApplyModifiedProperties()
		{
#if UNITY_EDITOR
			serialized.ApplyModifiedProperties();
#endif
		}

		/// <summary>
		/// used by auto-assign to pass around the setup info
		/// </summary>
		public virtual void AutoSetup(BodySetupContext bodySetupContext, System.Text.StringBuilder outLog)
		{
		}

		/// <summary>
		/// Setup initialization when the module is added to the object
		/// </summary>
		internal abstract void Init(Transform origin);

		/// <summary>
		/// when recording the current pose a dependency check is done 
		/// where each module provides transforms that need to be recorded.
		/// </summary>
		public virtual Transform[] CollectTransformDependencies()
		{
			return new Transform[0];
		}

#if UNITY_EDITOR

		/// <summary>
		/// Serialized Properties initialization
		/// </summary>
		protected virtual void InitializeSerializedProperties()
		{
			serialized = new SerializedObject(this);
			m_so_active = serialized.FindProperty(nameof(active));
			m_so_expandGUI = serialized.FindProperty(nameof(expandGUI));
		}

		public void SetExpand(bool state)
		{
			InitSerializedPropertiesIfNeeded();
			serialized.Update();
			m_so_expandGUI.boolValue = state;
			ApplyModifiedProperties();
		}

		public bool GetExpand()
		{
			return expandGUI;
		}
#endif

		/// <summary>
		/// Serialized Properties initialization with check
		/// </summary>
		protected void InitSerializedPropertiesIfNeeded()
		{
#if UNITY_EDITOR
			bool _needsInit = (serialized == null);
			if (_needsInit) {
				InitializeSerializedProperties();
			} else {
				ApplyModifiedProperties(); //prepare for further use
				serialized.Update();
			}
#endif
		}

		protected virtual void OnEnable()
		{
			ikp.Attach(this);
		}

		protected virtual void OnDestroy()
		{
			ikp.Detach(this);
		}



		/// <summary>
		/// Pick a bone based on prefered human bone or a list of filters.
		/// </summary>
		protected static Transform PickBone(BodySetupContext bodySetupContext, HumanBodyBones preferHumanBone, IEnumerable<string> boneNameFiltersBlacklist = null, params IEnumerable<string>[] boneNameFiltersWhitelist)
		{
			Transform _matchingBone;
			if (bodySetupContext.animator == null || !(_matchingBone = bodySetupContext.animator.GetBoneTransform(preferHumanBone)))
			{
				// filter out
				Debug.Assert(boneNameFiltersWhitelist.Length > 0, "Name filters missing!");

				{
					var _pool = new HashSet<Transform>(bodySetupContext.allBones);

					for (int i = 0; i < boneNameFiltersWhitelist.Length; i++)
					{
						_pool.RemoveWhere(_bone => !IKPUtils.MatchBoneName(_bone, boneNameFiltersWhitelist[i], boneNameFiltersBlacklist));
					}

					_matchingBone = _pool.FirstOrDefault();
				}

                // fallback if failed to find
                //if(!_matchingBone)
				//{
				//	var _pool = new HashSet<Transform>(bodySetupContext.allBones);
				//
				//	for (int i = 0; i < boneNameFiltersWhitelist.Length; i++)
				//	{
				//		_pool.RemoveWhere(_bone => !IKPUtils.MatchBoneName(_bone, boneNameFiltersWhitelist[i]));
				//	}
				//
				//	_matchingBone = _pool.FirstOrDefault();
				//
				//}
			}
			return _matchingBone;
		}
	}
}
