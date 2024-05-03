using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartZone : MonoBehaviour, IStage
{
    public TutorialStage stage { get; set; }
    public bool hasDialogFinished { get; set; }
    public bool hasUIIndicationsFinished { get; set; }
    public bool wasStageCompleted { get; set; }
    public int dialogCounter = 1;
    public TutorialStage stageToSet;
    public TutorialStage nextStage ;
    
    public Transform objectToFollow;

     private void Start()
     {
         stage = stageToSet;
     }

    public void OnDialogDisplayed()
    {
        TutorialManager.instance.DisplayTutorialData(dialogCounter);
        PlayerComponentsHandler.IsCurrentDeviceMouse = true;
        F_In_F_Out_Obj.OnInfoTextDisplayed?.Invoke(TutorialManager.instance.tutorialTextData.text);
        TutorialManager.instance.playerRef.canMove = false;
    }

    public void OnDialogFinished()
    {
        TaskList.instance.gameObject.SetActive(true);
        TaskList.instance.StartTaskList();   
        TutorialManager.instance.playerRef.canMove = true;
        CanvasController.currentObjToFollow = objectToFollow;
    }

    public void OnUIInstruction()
    {
        
    }

    public void OnStageGoing()
    {
        
    }

    public void OnStageEnded()
    {
        TaskList.instance.RemoveTaksObjFromKey("Go there!", 0);
    }
    
}

