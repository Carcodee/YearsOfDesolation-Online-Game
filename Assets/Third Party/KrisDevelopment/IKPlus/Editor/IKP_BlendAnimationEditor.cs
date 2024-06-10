// IKP - by Hristo Ivanov (Kris Development)
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using IKPn.Blend;

namespace IKPn
{
	[CustomEditor(typeof(IKPBlendAnimation))]
	public class BlendAnimationEditor : Editor
	{ //inspector editor
		private static int editAs = 0;
		[NonSerialized] private static string[] moduleSignatures;
		[NonSerialized] private static string[] moduleNames;


		public override void OnInspectorGUI()
		{
			if (moduleSignatures == null || moduleNames == null) {
				moduleSignatures = ModuleManager.GetSignatures().ToArray();
				moduleNames = moduleSignatures.Select(ModuleManager.GetName).ToArray();
			}

			IKPBlendAnimation blendAnim = (IKPBlendAnimation)target;
			Draw(blendAnim, serializedObject);
		}

		private static void Draw(IKPBlendAnimation blendAnim, SerializedObject serializedObject)
		{
			//use this to draw the properties
			editAs = EditorGUILayout.Popup("Edit As:", editAs, moduleNames);
			SETUtil.EditorUtil.HorizontalRule();
			Draw(blendAnim, moduleSignatures[editAs], serializedObject);
		}

		private static void Draw(IKPBlendAnimation blendAnim, string moduleSignature, SerializedObject serializedObject)
		{
			SerializedProperty
				_so_toggle = serializedObject.FindProperty(nameof(IKPBlendAnimation.toggle)),
				_so_speed = serializedObject.FindProperty(nameof(IKPBlendAnimation.speed)),
				_so_use = serializedObject.FindProperty(IKPBlendAnimation.PROPERTY_NAME_Use),
				_so_properties = serializedObject.FindProperty(IKPBlendAnimation.PROPERTY_NAME_Properties);


			GUILayout.BeginVertical(EditorStyles.helpBox);
			_so_toggle.enumValueIndex = (int)(ToggleAnimation)EditorGUILayout.EnumPopup(new GUIContent("Toggle Module", "Toggle state of the module when the animation gets called.\nDefault - leave as it is\nOn - enable\nOff - disable\n[(IKPBlend::ToggleAnimation) IKPBlendAnimation.toggle]"), blendAnim.toggle);
			EditorGUILayout.PropertyField(_so_speed, new GUIContent("Animation Speed", "Speed modifier that allows to fine-tune the speed at which the animation is running. (Default is 1.0)\n[IKPBlendAnimation.speed]"));

			serializedObject.ApplyModifiedProperties();

			string[] propertyNames = IKPUtils.GetPropertyNames(moduleSignature);

			if (_so_use.arraySize < propertyNames.Length)
				_so_use.arraySize = propertyNames.Length;
			if (_so_properties.arraySize < propertyNames.Length)
				_so_properties.arraySize = propertyNames.Length;

			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();

			for (int i = 0; i < propertyNames.Length; i++)
			{
				GUILayout.BeginHorizontal();
				SerializedProperty
					_so_prop_i_ac = (_so_properties.arraySize > i) ? _so_properties.GetArrayElementAtIndex(i) : (SerializedProperty)null,
					_so_use_i = (_so_use.arraySize > i) ? _so_use.GetArrayElementAtIndex(i) : (SerializedProperty)null;

				if (_so_use_i != null)
					_so_use_i.boolValue = EditorGUILayout.Toggle(blendAnim.GetUse(i), GUILayout.MaxWidth(30));

				if (_so_prop_i_ac != null)
					_so_prop_i_ac.animationCurveValue = EditorGUILayout.CurveField(SETUtil.StringUtil.WordSplit(propertyNames[i]), blendAnim.GetProperty(i));

				GUILayout.EndHorizontal();
			}
			serializedObject.ApplyModifiedProperties();

			GUILayout.EndVertical();
		}
	}
}

#endif
