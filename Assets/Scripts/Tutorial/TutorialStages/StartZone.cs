using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartZone : MonoBehaviour, IStage
{
    public TutorialStage stage { get; set; }
    public bool hasDialogFinished { get; set; }
    public bool hasUIIndicationsFinished { get; set; }
    public int dialogCounter = 1;
     public TutorialStage stageToSet;
    public TutorialStage nextStage ;

     private void Start()
     {
         stage = stageToSet;
     }

    public void OnDialogDisplayed()
    {
        TutorialManager.instance.DisplayTutorialData(dialogCounter);
        PlayerComponentsHandler.IsCurrentDeviceMouse = true;
        F_In_F_Out_Obj.OnInfoTextDisplayed?.Invoke(TutorialManager.instance.tutorialTextData.text);
    }

    public void OnDialogFinished()
    {
        hasDialogFinished = true;
        TaskList.instance.gameObject.SetActive(true);
        TaskList.instance.StartTaskList();   
        
    }

    public void OnUIInstruction()
    {
        
    }

    public void OnStageGoing()
    {
        
    }

    public void OnStageEnded()
    {
        
    }
}
