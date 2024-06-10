using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KrisDevelopment.AnimationTools
{
	[InitializeOnLoad]
	public static class ArmatureBoneDrawing
	{
		private const string PREFS_KEY = "KD_ArmatureBoneDrawing";
		private static IEnumerable<SkinnedMeshRenderer> skinnedObjects = null;
		private static bool? _disabled;
		private static bool disabled { get { return _disabled ?? EditorPrefs.GetInt(PREFS_KEY) != 0; } set { EditorPrefs.SetInt(PREFS_KEY, ((bool)(_disabled = value)) ? 1 : 0);  } }


		static ArmatureBoneDrawing()
		{
			Selection.selectionChanged += SelectionChanged;
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += DuringSceneGUI;
#else
			SceneView.onSceneGUIDelegate += DuringSceneGUI;
#endif
		}

		public static void PaintBone(Vector3 pointA, Vector3 pointB, Action onClick = null, Color? preferColor = null)
		{
#if UNITY_EDITOR
			Color _gizmoColor = preferColor ?? Color.cyan;

			Handles.color = _gizmoColor;
			var _dir = pointA - pointB;
			var _deltaSize = HandleUtility.GetHandleSize(pointA);
			var _size = _deltaSize * 0.2f;
			if (_dir != Vector3.zero)
			{
				if (Handles.Button(pointB + _dir / 2, Quaternion.LookRotation(_dir), _size, _size, Handles.ConeHandleCap))
				{
					if (onClick != null)
					{
						onClick.Invoke();
					}
				}
			}
#if UNITY_2020_1_OR_NEWER
			Handles.DrawLine(pointA, pointB, 2);
#else
			Handles.DrawLine(pointA, pointB);
#endif
			Handles.color = Color.white;
#endif
		}

		private static void DuringSceneGUI(SceneView obj)
		{
			if(disabled || skinnedObjects == null)
			{
				return;
			}

			foreach (var _skin in skinnedObjects)
			{
				if (!_skin)
				{
					continue;
				}

				foreach (var _bone in _skin.bones)
				{
					if(_bone == null)
                    {
						continue;
                    }
                
					if (_bone == _skin.rootBone || _bone.parent == null)
					{
						continue;
					}

					PaintBone(_bone.position, _bone.parent.position, () => { Selection.activeObject = _bone.parent; });

					// also draw to children
					foreach (Transform _child in _bone)
					{
						if (_skin.bones.Contains(_child))
						{
							continue;
						}

						PaintBone(_child.position, _bone.position, () => { Selection.activeObject = _child.parent; }, Color.yellow);
					}
				}
			}
		}

		private static void SelectionChanged()
		{
			if (skinnedObjects != null)
			{
				foreach (var _skin in skinnedObjects)
				{
					if (!_skin)
					{
						continue;
					}

					foreach (var _bone in _skin.bones)
					{
						if (_bone != null && Selection.gameObjects.Contains(_bone.gameObject))
						{
							skinnedObjects = new List<SkinnedMeshRenderer>() { _skin };
							return;
						}
					}
				}
			}

			if (Selection.gameObjects != null)
			{
				skinnedObjects = Selection.gameObjects.Select(a => a.GetComponent<SkinnedMeshRenderer>()).Where(a => a != null).ToList();
			}
		}

		[MenuItem("Tools/Kris Development/Animation Utilities/Toggle Bone Drawing")]
		private static void ToggleArmatureBoneDrawing()
		{
			disabled = !disabled;
			EditorUtility.DisplayDialog("Skinned Mesh Bone Drawing", disabled ? "Disabled" : "Enabled", "Ok");
		}
	}
}
