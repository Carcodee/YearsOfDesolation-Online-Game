using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneToReach : MonoBehaviour, IInteractable
{
    public ZoneToGo nextZoneType;
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
        PlayerZoneAction();
        TutorialManager.instance.currentHUDStage = nextZoneType;
        TutorialManager.instance.wasTutorialStepDone= false;
        hasThisZoneReached = true;
    }

    void PlayerZoneAction(){} 
    
}
