using System;
using System.Collections;
using System.Collections.Generic;
using Menu.StatsPanel;
using UnityEditor;
using UnityEngine;



public class DebugManagerEditor : EditorWindow
{
    
    DebugManager debugManager;


    private void OnInspectorUpdate()
    {

        if (debugManager!=null)
        {
            if (debugManager.postProccesingManager==null)
            {
                
                debugManager.postProccesingManager = FindObjectOfType<PostProccesingManager>();
            }
        }
    }

    [MenuItem("Window/Game Debugger Window")]
    static void Init()
    {
        var debugManager = (DebugManagerEditor)EditorWindow.GetWindow(typeof(DebugManagerEditor));
        debugManager.Show();

    }

    private void OnEnable()
    {
        if (debugManager == null)
        {
            debugManager = FindObjectOfType<DebugManager>();
        }
    }

    void OnGUI()
    {


            if (debugManager == null)
            {
                debugManager = FindObjectOfType<DebugManager>();
            }
            
            EditorGUILayout.LabelField("CoinUtility",EditorStyles.boldLabel);
            
            if (GUILayout.Button("SpawnCoin to follow in front")&&debugManager.playerStatsController != null)
            {
                debugManager.SpawnCoin();
            }
            
            
            EditorGUILayout.LabelField("GameLogic",EditorStyles.boldLabel);
            
            if (GUILayout.Button("Freeze time")&&debugManager.playerStatsController != null)
            {
                debugManager.FreezeTime();
            }
            if (GUILayout.Button("FastRoyale time")&&debugManager.playerStatsController != null)
            {
                debugManager.FastBattleRoyale();
            }
            if (GUILayout.Button("Level UP")&&debugManager.playerStatsController != null)
            {
                debugManager.LevelUp();
            }
            EditorGUILayout.LabelField("Post Processing",EditorStyles.boldLabel);
            if (GUILayout.Button("Remove menu blur")&&debugManager.postProccesingManager != null)
            {
                Debug.Log("hello");
                debugManager.postProccesingManager.ActivateMenuBlur(0);
            }
            if (GUILayout.Button("Activate menu blur")&&debugManager.postProccesingManager != null)
            {
                debugManager.postProccesingManager.ActivateMenuBlur(1);
            }
            
        
    }
    
}