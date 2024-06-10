
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace KrisDevelopment.AnimationTools
{
	public class SkinnedMeshBonesInspector : EditorWindow
	{
		private SkinnedMeshRenderer targetSkin;
		private Vector2 scrollView;
		private string search;

		[MenuItem("Window/Kris Development/Animation Utilities/Skinned Mesh Bones Inspector")]
		public static void ShowWindow()
		{
			var _window = GetWindow<SkinnedMeshBonesInspector>("Skin Bones");
			_window.Select(Selection.activeGameObject);
		}

		/// <summary>
		/// Select a game object for context
		/// </summary>
		private void Select(GameObject gameObject)
		{
			targetSkin = GetTargetSkin(gameObject);
			Repaint();
		}

		private void OnGUI()
		{

			if (targetSkin != null)
			{
				GUILayout.Label("Skinned Mesh Bones Inspctor", EditorStyles.largeLabel);
				search = EditorGUILayout.TextField(new GUIContent("Search"), search);

				scrollView = GUILayout.BeginScrollView(scrollView);
				{
					EditorGUILayout.ObjectField("Root", targetSkin.rootBone, typeof(UnityEngine.Object), true);
					GUILayout.Space(8);
					foreach (var _bone in targetSkin.bones)
					{
						if (string.IsNullOrEmpty(search) || _bone.name.IndexOf(search, System.StringComparison.InvariantCultureIgnoreCase) >= 0)
						{
							EditorGUILayout.ObjectField(_bone, typeof(UnityEngine.Object), true);
						}
					}
				}
				GUILayout.EndScrollView();
			}
			else
			{
				GUILayout.Label("No skinned mesh");
			}
		}

		private SkinnedMeshRenderer GetTargetSkin(GameObject targetGameObject)
		{
			if (targetGameObject)
			{
				return targetGameObject.GetComponent<SkinnedMeshRenderer>();
			}

			return null;
		}

		private void OnSelectionChange()
		{
			Select(Selection.activeGameObject);
		}
	}
}
