using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStagesHandler : MonoBehaviour
{
    public static TutorialStagesHandler instance;
    public GameObject []tutorialStages;
    public List<IStage> stages= new List<IStage>();
    public IStage currentStage;
    private void Start()
    {
        if (instance != null)
        {
           Destroy(this);
           return;
        }
        instance = this;
    }

    private void Update()
    {
        if (currentStage==null)return;
        if (currentStage.hasDialogFinished && currentStage.hasUIIndicationsFinished)
        {
           currentStage.OnStageGoing(); 
        }
    }

    public void Init()
    {

        for (int i = 0; i < tutorialStages.Length; i++)
        {
            IStage currentStageToAdd = tutorialStages[i].GetComponent<IStage>();
            stages.Add(currentStageToAdd);
        }

        currentStage = stages[0];
        currentStage.OnDialogDisplayed();


    }


    public void SetTutorialStage(TutorialStage nextStage)
    {
        currentStage.OnStageEnded();
        currentStage = GetStage(nextStage);
        currentStage.OnDialogDisplayed();
    }

    public void FinishDialogs()
    {
        currentStage.OnDialogFinished();
        currentStage.OnUIInstruction();
    }
     public void FinishInstructions()
     {
         currentStage.hasDialogFinished = true;
     }
     public IStage GetStage(TutorialStage stageToFind)
    {
        for (int i = 0; i < stages.Count; i++)
        {
            IStage stage = stages[i];
            if (stage.stage==stageToFind)
            {
                return stage;
            }
        } 
        Debug.Log("No stage found");
        return null;
    }
}