using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_PickBuild : MonoBehaviour,IStage
{
    public TutorialStage stage { get; set; }
    public bool hasDialogFinished { get; set; }
    public bool hasUIIndicationsFinished { get; set; }
    public bool wasStageCompleted { get; set; }
    public int dialogCounter = 2;
    public TutorialStage stageToSet;
    public TutorialStage nextStage ;
    public GameObject nextStageObjToFollow;

     private void Start()
     {
         stage = stageToSet;
     }

    public void OnDialogDisplayed()
    {
        
        TutorialManager.instance.playerRef.canMove = false;
        TutorialManager.instance.playerRef.stateMachineController.SetState("Idle");
        PlayerComponentsHandler.IsCurrentDeviceMouse = true;
        CanvasController.currentObjToFollow = null;
    }

    public void OnDialogFinished()
    {
        
        TutorialManager.instance.playerRef.canMove = true;
        TutorialManager.instance.playerRef.stateMachineController.SetState("Movement");
        
    }

    public void OnUIInstruction()
    {
        
    }

    public void OnStageGoing()
    {
        
    }

    public void OnStageEnded()
    {
        
        TaskList.instance.RemoveTaksObjFromKey("Pick a build", 0);
    }
}
