using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_TakeAmmo : MonoBehaviour, IStage
{
    public TutorialStage stage { get; set; }
    public bool hasDialogFinished { get; set; }
    public bool hasUIIndicationsFinished { get; set; }
    public int dialogCounter = 3;
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
        
    }

    public void OnUIInstruction()
    {
        TutorialManager.instance.SpawnCoinAtPos();
    }

    public void OnStageGoing()
    {
        
    }

    public void OnStageEnded()
    {
        
    }
}
