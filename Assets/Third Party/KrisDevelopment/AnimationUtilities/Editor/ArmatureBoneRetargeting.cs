using KrisDevelopment.DistributedInternalUtilities;
using SETUtil;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KrisDevelopment.AnimationTools
{
	public class ArmatureBoneRetargeting : EditorWindow
	{
		private enum Source
		{
			FromTransform,
			FromSkinnedMesh,
		}


		private const string WEBSITE_URL = "https://krisdevelopment.wordpress.com/";

		private GameObject targetGameObject;

		private Source sourceType = Source.FromSkinnedMesh;
		private Transform sourceTransform;
		private SkinnedMeshRenderer sourceSkin;


		[MenuItem("Tools/Kris Development/Animation Utilities/Armature Bone Retargeting")]
		public static void ShowWindow()
		{
			var _window = SETUtil.EditorUtil.ShowUtilityWindow<ArmatureBoneRetargeting>("Armature Bone Retargeting");
			_window.Select(Selection.activeGameObject);
		}

		/// <summary>
		/// Select a game object for context
		/// </summary>
		private void Select(GameObject gameObject)
		{
			targetGameObject = gameObject;
		}

		private void OnGUI()
		{
			EditorGUILayout.HelpBox("Drag the object you want to fix into Target. Drag the object you want to copy from into Source/Armature.", MessageType.None);

			{
				var t = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target"), targetGameObject, typeof(GameObject), true);
				if (t != targetGameObject)
				{
					targetGameObject = t;
				}
			}

			bool _targetSkinNull = GetTargetSkin() == null;

			if (_targetSkinNull)
			{
				EditorGUILayout.HelpBox("Skinned Mesh Renderer mising in target!", MessageType.Error);
			}

			sourceType = (Source)EditorGUILayout.EnumPopup("Source Type:", sourceType);

			if (sourceType == Source.FromTransform)
			{
				sourceTransform = (Transform)EditorGUILayout.ObjectField(new GUIContent("Armature"), sourceTransform, typeof(Transform), true);
			}
			else if (sourceType == Source.FromSkinnedMesh)
			{
				sourceSkin = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(new GUIContent("Source Skin"), sourceSkin, typeof(SkinnedMeshRenderer), true);
			}

			EditorGUI.BeginDisabledGroup(_targetSkinNull || GetRootBone() == null);
			{
				if (GUILayout.Button("GO!"))
				{
					Run(targetGameObject.transform);
				}
			}
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(8);
			GUIStyle _linkLabelStyle;
#if UNITY_2019_1_OR_NEWER
			_linkLabelStyle = EditorStyles.linkLabel;
#else
			_linkLabelStyle = EditorStyles.miniButton;
#endif

			EditorUtil.HorizontalRule();

			if (GUILayout.Button("Website", _linkLabelStyle))
			{
				Application.OpenURL(WEBSITE_URL);
			}

			BugReporting.SmallBugReportButton();
		}

		private void Run(params Transform[] targets)
		{
			var _log = new StringBuilder();

			foreach (var _target in targets)
			{
				try
				{
					var _targetSkin = _target.GetComponent<SkinnedMeshRenderer>();
					if (_targetSkin == null)
					{
						_log.AppendLine(string.Format("No skinned mesh found at {0}. Skipping.", _target));
					}

					AssignBonesToTarget(_targetSkin, GetRootBone(), GetBones());
					_log.AppendLine(string.Format("Bones assigned to {0} successfully", _targetSkin));
				}
				catch (Exception e)
				{
					_log.AppendLine(e.GetType().Name);
					Debug.LogError(e);
				}
			}

			SETUtil.EditorUtil.ShowOperationLogWindow("Armature Complete", _log);
		}

		private static void AssignBonesToTarget(SkinnedMeshRenderer targetSkin, Transform rootBone, Transform[] bones)
		{
			targetSkin.rootBone = rootBone;
			targetSkin.bones = bones;

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				EditorUtility.SetDirty(targetSkin);
			}
#endif
		}

		private SkinnedMeshRenderer GetTargetSkin()
		{
			if (targetGameObject)
			{
				return targetGameObject.GetComponent<SkinnedMeshRenderer>();
			}

			return null;
		}

		private Transform GetRootBone()
		{
			switch (sourceType)
			{
				case Source.FromTransform:
					return sourceTransform;

				case Source.FromSkinnedMesh:
					return sourceSkin != null ? sourceSkin.rootBone : null;
			}

			return null;
		}

		private Transform[] GetBones()
		{
			if (sourceSkin != null)
			{
				return sourceSkin.bones;
			}
			else
			{
				var _output = new List<Transform>();
				foreach (var _bone in SETUtil.SceneUtil.CollectAllChildren(sourceTransform, true))
				{
					_output.Add(_bone);
				}

				return _output.ToArray();
			}
		}
	}
}
