#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using UnityEngine;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using UnityEditor;

namespace KrisDevelopment.DistributedInternalUtilities
{
	public abstract class EditorTestScene : IDisposable
	{
		Scene scene;
		string initialScenePath;

		protected abstract string sceneResourceName { get; }

		public EditorTestScene()
		{
			var _activeScene = EditorSceneManager.GetActiveScene();
			initialScenePath = _activeScene.path;

			var _sceneAsset = SETUtil.ResourceLoader.EditorObjectResource.Get<SceneAsset>(sceneResourceName);
			Assert.IsTrue(_sceneAsset != null, "TestScene - _sceneAsset resource is null");
			var _sceneAssetPath = AssetDatabase.GetAssetPath(_sceneAsset);

			Assert.IsNotEmpty(_sceneAssetPath, "TestScene - _sceneAssetPath empty");

			scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(_sceneAssetPath, UnityEditor.SceneManagement.OpenSceneMode.Single);
		}

		public virtual void Dispose()
		{
			// clear everything from the scene
			foreach (var go in scene.GetRootGameObjects())
			{
				foreach (var t in go.GetComponentsInChildren<Transform>())
				{
					t.gameObject.hideFlags = HideFlags.DontSave;
				}
				SETUtil.SceneUtil.SmartDestroy(go);
			}

			if (!string.IsNullOrEmpty(initialScenePath))
			{
				EditorSceneManager.OpenScene(initialScenePath, OpenSceneMode.Single);
			}
		}
	}
}
#endif