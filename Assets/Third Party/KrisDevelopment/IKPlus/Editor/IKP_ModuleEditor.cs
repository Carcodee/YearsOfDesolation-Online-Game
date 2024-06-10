
#if UNITY_EDITOR

// IKP - by Hristo Ivanov (Kris Development)

using UnityEditor;

namespace IKPn
{
	[CustomEditor(typeof(IKPModule), true, isFallback = true)]
	public class ModuleEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			// Draw nothing
		}
	}
}

#endif
