using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_PlayerZone : MonoBehaviour, IStage
{ 
     public TutorialStage stage { get; set; }
     public bool hasDialogFinished { get; set; }
     public bool hasUIIndicationsFinished { get; set; }
     public bool wasStageCompleted { get; set; }
     public int dialogCounter = 2;
     public TutorialStage stageToSet;
     public TutorialStage nextStage ;

     private void Start()
     {
         stage = stageToSet;
     }

     public void OnDialogDisplayed()
     {
         TutorialManager.instance.DisplayTutorialData(dialogCounter);
         TutorialManager.instance.playerRef.canMove = false;
         PlayerComponentsHandler.IsCurrentDeviceMouse = true;
         F_In_F_Out_Obj.OnInfoTextDisplayed?.Invoke(TutorialManager.instance.tutorialTextData.text);
         TutorialManager.instance.playerRef.stateMachineController.SetState("Idle");
     }
 
     public void OnDialogFinished()
     {
         TutorialStagesHandler.instance.SetTutorialStage(TutorialStage.PickBuild);
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
