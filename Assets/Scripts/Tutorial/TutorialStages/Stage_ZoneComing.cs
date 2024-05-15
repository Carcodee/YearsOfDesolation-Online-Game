using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_ZoneComing : MonoBehaviour,IStage
{
     public TutorialStage stage { get; set; }
     public bool hasDialogFinished { get; set; }
     public bool hasUIIndicationsFinished { get; set; }
     public bool wasStageCompleted { get; set; }
     public int dialogCounter = 6;
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
        GameController.instance.started.Value = true;
        GameController.instance.mapLogic.Value.isBattleRoyale = true;
        TutorialManager.instance.playerRef.canMove = false;
        TutorialManager.instance.playerRef.stateMachineController.SetState("Idle");

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
         
     }       
}
