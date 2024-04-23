using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_PickBuild : MonoBehaviour,IStage
{
    public TutorialStage stage { get; set; }
    public bool hasDialogFinished { get; set; }
    public bool hasUIIndicationsFinished { get; set; }
    public int dialogCounter = 2;
     public TutorialStage stageToSet;
    public TutorialStage nextStage ;

     private void Start()
     {
         stage = stageToSet;
     }

    public void OnDialogDisplayed()
    {
        
        PlayerComponentsHandler.IsCurrentDeviceMouse = true;
    }

    public void OnDialogFinished()
    {
        
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
