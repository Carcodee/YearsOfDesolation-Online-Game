using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_KillEnemy : MonoBehaviour,IStage
{

    public TutorialStage stage { get; set; }
    public bool hasDialogFinished { get; set; }
    public bool hasUIIndicationsFinished { get; set; }
    public int dialogCounter = 5;
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
        
        Debug.Log("Dialog displayed");
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
        TaskList.instance.realTask.GetValueOrDefault("Pick a build").Done();
        TaskList.instance.tasksToCreate.RemoveAt(0);
        
    }

}
