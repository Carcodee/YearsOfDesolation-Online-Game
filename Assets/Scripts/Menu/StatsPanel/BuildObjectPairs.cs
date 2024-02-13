using System.Collections;
using System.Collections.Generic;
using Menu.StatsPanel;
using UnityEngine;

public class BuildObjectPairs : MonoBehaviour
{
    public BuildObjectHandler[] buildObjectHandlers;
    
    public void InitialiseBuilds()
    {
        foreach (var buildObjectHandler in buildObjectHandlers)
        {
            buildObjectHandler.InitiliseBuild();
        }
    }
    public void SetReferences(PlayerStatsController playerStatsController)
    {
        foreach (var buildObjectHandler in buildObjectHandlers)
        {
            buildObjectHandler.playerStatsController = playerStatsController;
        }
    }
    
    public void DisplayBuilds()
    {
        foreach (var buildObjectHandler in buildObjectHandlers)
        {
            buildObjectHandler.shopButtonManager.UpdateUI();
            buildObjectHandler.DisplayData();
            buildObjectHandler.shopButtonManager.UpdateUI();
            
        }
    }
    
    public bool IsInitialized ()
    {
        foreach (var buildObjectHandler in buildObjectHandlers)
        {
            if (buildObjectHandler.playerBuildObjectSelected == null)
            {
                return false;
            }
        }
        return true;
    } 
}
