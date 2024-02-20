using Menu.StatsPanel;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    // [CustomEditor(typeof(BuildObjectHandler))]
    public class BuildObjectEditor : UnityEditor.Editor
    {
        // private BuildObjectHandler buildObjectHandler;
        // public override void OnInspectorGUI()
        // {
        //     base.OnInspectorGUI();  
        //     buildObjectHandler = (BuildObjectHandler)target;
        //     // buildObjectHandler.buildType = (BuildType)EditorGUILayout.EnumPopup("Build Type", buildObjectHandler.buildType);
        //
        //     //create a Image label
        //     EditorGUILayout.LabelField("First Weapon Image");
        //     buildObjectHandler.BorderImage = (UnityEngine.UI.Image)EditorGUILayout.ObjectField(buildObjectHandler.BorderImage, typeof(UnityEngine.UI.Image), true);
        //     
        //     if(GUI.changed)
        //     {
        //         // Do something when the value changes
        //         Debug.Log("Dropdown value changed");
        //     }
        //
    }
}


