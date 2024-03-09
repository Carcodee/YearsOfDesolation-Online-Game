using System;
using System.Collections;
using System.Collections.Generic;
using Menu.StatsPanel;
using UnityEditor;
using UnityEngine;



public class DebugManagerEditor : EditorWindow
{
    
    DebugManager debugManager;
    
    
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
            if (GUILayout.Button("Level UP")&&debugManager.playerStatsController != null)
            {
                debugManager.LevelUp();
            }
            
        
    }
    
}