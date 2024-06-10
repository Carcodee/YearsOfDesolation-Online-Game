
#if UNITY_EDITOR

using SETUtil.Types;
using UnityEditor;
using UnityEngine;

namespace IKPn
{
	[CustomEditor(typeof(IKP_Target))]
	public class IKPTargetEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			IKP_Target mainScript = (IKP_Target) target;

			DrawDefaultInspector();

			Quaternion q = mainScript.currentOffset;

			GUILayout.Box("Current Rotation Offset:\n X: " + q.x + " Y: " + q.y + " Z: " + q.z + " W: " + q.w, GUILayout.MaxWidth(Screen.width));
			GUILayout.BeginHorizontal();
			{
				using (new ColorPocket()) {
					if (!mainScript.isInEditMode) {
						GUI.color = IKPStyle.COLOR_ACTIVE;
						if (GUILayout.Button("Paste Rotation")) {
							if (EditorUtility.DisplayDialog("Load", "This will set the offset values to the ones stored in the memory. Continue?", "Yes", "No"))
								mainScript.LoadRotationOffset();
						}

						GUI.color = Color.white;
						if (GUILayout.Button("Edit Rotation Offset")) {
							mainScript.StartEdit();
						}

						if (GUILayout.Button("Clear")) {
							if (EditorUtility.DisplayDialog("Clear", "Clear offset values?", "Yes", "No"))
								mainScript.ClearRotationOffset();
						}
					} else {
						GUI.color = IKPStyle.COLOR_ACTIVE;
						if (GUILayout.Button("Copy Rotation")) {
							mainScript.EndEdit();
						}

						GUI.color = IKPStyle.COLOR_RESET;
						if (GUILayout.Button("Done")) {
							mainScript.EndEdit(false);
						}

						GUI.color = Color.white;
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		public void OnInspectorUpdate()
		{
			this.Repaint();
		}
	}
}

#endif
