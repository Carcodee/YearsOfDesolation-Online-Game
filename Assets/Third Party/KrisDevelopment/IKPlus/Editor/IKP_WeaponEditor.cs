
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IKPn{
	[CustomEditor(typeof(IKP_Weapon)), CanEditMultipleObjects]
	public class WeaponEditor : Editor {

		public override void OnInspectorGUI (){
			DrawDefaultInspector();
			IKP_Weapon scr = (IKP_Weapon)target;
			scr.SetForwardOffset(EditorGUILayout.Slider("Forward Offset", scr.GetForwardOffset(), 0f, 1f));
		}
	}
}

#endif
