using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class scrEdit_objectGridRepositioneer : ScriptableWizard
{
    public float movGridStep = 1.0f;

    bool toggleAxis_X = true;
    bool toggleAxis_Y = false;
    bool toggleAxis_Z = true;

    // add menu item
    [MenuItem("ZerinLabs/Object repositioneer utility")]

    //-----------------------------------------------------------------------------------------------------------------
    static void Init()
    {
        scrEdit_objectGridRepositioneer window = (scrEdit_objectGridRepositioneer)EditorWindow.GetWindow(typeof(scrEdit_objectGridRepositioneer));
        window.Show();
    }

    //-----------------------------------------------------------------------------------------------------------------
    void OnGUI()
    {
        GUILayout.Label("Information:", EditorStyles.boldLabel);

        EditorStyles.label.wordWrap = true;
        GUILayout.Label("This script snaps any number of selected object to a grid based on the 'step units' defined by the user.\nYou can set the axis where you want to snap the objects using the 'Axis' checkboxes.", EditorStyles.label);
        
        GUILayout.Space(10);

        GUILayout.Label("Snap pameters", EditorStyles.boldLabel);

        movGridStep = EditorGUILayout.FloatField("Move grid step (units):", movGridStep);

        GUILayout.Space(10);

        GUILayout.Label("Axis:");

        toggleAxis_X = GUILayout.Toggle(toggleAxis_X, "X axis");
        toggleAxis_Y = GUILayout.Toggle(toggleAxis_Y, "Y axis");
        toggleAxis_Z = GUILayout.Toggle(toggleAxis_Z, "Z axis");

        GUILayout.Space(10);
        
        //movement handlers
        if (GUILayout.Button("\nSNAP OBJECTS! \n"))
        {
            SnapObjects();
        }
    }

    //-----------------------------------------------------------------------------------------------------------------
    void SnapObjects()
    {
        for (int i = 0; i < Selection.transforms.Length; i++)
        {
            GameObject go = Selection.transforms[i].gameObject;

            Vector3 shift = new Vector3(0f, 0f, 0f);

            float stepVal = movGridStep;

            if (toggleAxis_X == true)
            {
                shift.x = ShiftSnap(go.transform.position.x, stepVal);
            }

            if (toggleAxis_Y == true)
            {
                shift.y = ShiftSnap(go.transform.position.y, stepVal);
            }

            if (toggleAxis_Z == true)
            {
                shift.z = ShiftSnap(go.transform.position.z, stepVal);
            }

            Undo.RegisterCompleteObjectUndo(go, "Changed position");

            go.transform.position = go.transform.position + shift;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------
    float ShiftSnap(float pos, float stepVal)
    {
        float val = 0f;

        float reminder = pos % stepVal;

        if (reminder > stepVal * 0.5)
        {
            val = stepVal - reminder;
        }
        else
        {
            val = reminder * -1f;
        }

        return val;
    }
}
#endif