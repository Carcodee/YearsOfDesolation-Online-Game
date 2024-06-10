// IKP - by Hristo Ivanov (Kris Development)

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{

    /// <summary>
    /// setup, settings, properties, look behavior
    /// </summary>
    public abstract class ModuleBase_LookAroundLogicSSP : ModuleBase_StandardBoneLogicSSP 
	{
		[HideInInspector, SerializeField] protected bool trackTargetOnBack = true;

		[HideInInspector, SerializeField]
		protected float
			angleLimit = 170f;

		[HideInInspector, SerializeField, 
			Tooltip("Snap the look rotation to it's last valid state if it goes outside of the max look angle.\nWARNING: May cause jittering.")]
		protected bool
			forcedAngleClamp = false;

#if UNITY_EDITOR
		protected SerializedProperty
			m_so_trackTargetOnBack,
			m_so_angleLimit,
			m_so_forcedAngleClamp;
#endif

#if UNITY_EDITOR
		protected override void DrawSettings()
		{
			//[!] method relies on previous SPInit call
			base.DrawSettings();

			m_so_trackTargetOnBack.boolValue = GUILayout.Toggle(m_so_trackTargetOnBack.boolValue, new GUIContent("Track Target On Back", "[ModuleBase_Look.trackTargetOnBack]\nAttempt to follow the target when it goes outside of the look angle limit"));
			EditorGUILayout.PropertyField(m_so_angleLimit, new GUIContent("Angle Limit", "[ModuleBase_Look.angleLimit]\nLook angle limit"));
			m_so_forcedAngleClamp.boolValue = EditorGUILayout.ToggleLeft(new GUIContent(m_so_forcedAngleClamp.displayName, m_so_forcedAngleClamp.tooltip), m_so_forcedAngleClamp.boolValue);
		}
#endif

#if UNITY_EDITOR
		protected override void InitializeSerializedProperties()
		{
			base.InitializeSerializedProperties();
			serialized.Update();
			m_so_trackTargetOnBack = serialized.FindProperty(nameof(trackTargetOnBack));
			m_so_angleLimit = serialized.FindProperty(nameof(angleLimit));
			m_so_forcedAngleClamp = serialized.FindProperty(nameof(forcedAngleClamp));
		}
#endif
	}
}
