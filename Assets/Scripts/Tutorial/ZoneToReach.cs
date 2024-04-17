using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneToReach : MonoBehaviour, IInteractable
{
    public TutorialStage stageToTrigger;
    public bool hasThisZoneReached = false;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Interact()  
    {
        if (hasThisZoneReached) return;
        TutorialStagesHandler.instance.SetTutorialStage(stageToTrigger);
        hasThisZoneReached = true;
    }

    void PlayerZoneAction(){} 
    
}
