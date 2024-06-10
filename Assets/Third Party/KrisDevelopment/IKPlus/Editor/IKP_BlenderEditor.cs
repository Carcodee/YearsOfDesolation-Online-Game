// IKP - by Hristo Ivanov (Kris Development)

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace IKPn
{
	[CustomEditor(typeof(IKP_Blender))]
	public class BlenderEditor : Editor
	{

		private IKP_Blender blender;

		public override void OnInspectorGUI()
		{
			blender = (IKP_Blender)target;

			DrawDefaultInspector();

			if (blender.HasBlendMachine())
			{
				if (GUILayout.Button(new GUIContent(" Edit Blend Machine", IKPStyle.blendMachineIcon)))
				{
					IKP_BlendMachineEditor.ShowWindow(blender.GetBlendMachine());
				}
				GUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.Label("Current State: " + blender.GetCurrentState());
				GUILayout.EndVertical();
			}
			else
			{
				GUILayout.Label("No Blend Machine assigned!", EditorStyles.helpBox);
			}
		}
	}
}
#endif
