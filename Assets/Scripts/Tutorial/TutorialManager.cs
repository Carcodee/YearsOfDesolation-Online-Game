using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEngine.TextCore.Text;
using TextAsset = UnityEngine.TextAsset;

public class TutorialManager : MonoBehaviour
{

    public static TutorialManager instance;
    public ZoneToGo currentHUDStage = ZoneToGo.PlayerZone;


    [Header("Dialogs System")]
    public string fileName;
    private int dialogCounter=1;
    public int HUBCounter=0;
    public int maxHubCounter = 5;
    public TextAsset dialogs;
    public DialogData tutorialTextData;
    public TableData tableData;
    public bool isLastText;

    public bool wasTutorialStepDone;

    [Header("Map")] 
    public Transform spawnPoint;
    
    
    private void Awake()
    {
        fileName = Application.dataPath + "/TextFiles/Tutorial/TutorialDialog.csv";
        dialogCounter = 1;
        HUBCounter = 0; 
        if (instance!=null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        
    }

    private void Update()
    {
        CheckCurrentZoneToGo();
    }

    private void Start()
    {
        StartCoroutine(SetPlayerInPos());
        InitData();
        CheckCurrentZoneToGo();
        
    }

    public void CheckCurrentZoneToGo()
    {
        if (wasTutorialStepDone) return;
        PlayerComponentsHandler.IsCurrentDeviceMouse = true;
        switch (currentHUDStage)
        {
            case ZoneToGo.PlayerZone:
                DisplayTutorialData(1);
                break;
            case ZoneToGo.PickBuildZone:
                DisplayTutorialData(2);
                break;
            case ZoneToGo.TakeCoinZone:
                DisplayTutorialData(3);
                break;
            case ZoneToGo.UpgradeZone:
                DisplayTutorialData(4);
                break;
            case ZoneToGo.EEnemyZone:
                DisplayTutorialData(5);
                break;
        }

        F_In_F_Out_Obj.OnInfoTextDisplayed?.Invoke(tutorialTextData.text);
        wasTutorialStepDone = true;

    }
    public IEnumerator SetPlayerInPos()
    {
        
        if (GameManager.Instance.localPlayerRef==null)
        {
            yield return null;
        }

        GameManager.Instance.localPlayerRef.transform.position = spawnPoint.position;
        GameManager.Instance.localPlayerRef.transform.rotation= Quaternion.Euler(0,-180,0);

    }
    public void NextHUB()
    {
        if (isLastText)return;
        HUBCounter++;
        if (HUBCounter<maxHubCounter-1)
        {
            DisplayTutorialData(dialogCounter);
        }
    }
    public void GoBackHUB()
    {
        if (HUBCounter<=0)
        {
           return; 
        }
        HUBCounter--;
        DisplayTutorialData(dialogCounter);
    }

    public void StepDone()
    {
        PlayerComponentsHandler.IsCurrentDeviceMouse = false;
        HUBCounter = 0;
        F_In_F_Out_Obj.OnCleanScreen?.Invoke();

    }
    void DisplayTutorialData(int currentDialogCounter)
    {
        string CheckNextHUB = GetValueFromIndex(dialogCounter, 2 + HUBCounter + 1);
        dialogCounter = currentDialogCounter;
        tutorialTextData.id = int.Parse(GetValueFromIndex(dialogCounter, 0));
        Debug.Log(tutorialTextData.id);
        tutorialTextData.specification = GetValueFromIndex(dialogCounter, 1);
        Debug.Log(tutorialTextData.specification);
        tutorialTextData.text =GetValueFromIndex(dialogCounter, 2+HUBCounter);
        Debug.Log(tutorialTextData.text);
        if (CheckNextHUB=="none")
        {
            isLastText = true;
        }
        else
        {
            isLastText = false;
        }
    }
    
    string GetValueFromIndex(int row, int column)
    {
        int index = (row*7)+ column;
        return tableData.data[index];
    }
    void InitData()
    { 
       string[] textData= dialogs.text.Split(new string [] {",", "\n"}, StringSplitOptions.None);
       tableData.data = textData;
       int counter = 0;
    }
    void OpenTableFile(String path)
    {
        string[] lines = File.ReadAllLines(path);
        Debug.Log("lines: "+ lines.Length);
        foreach (var line in lines)
        { 
            line.Split(",");
        }
  
    }

    void OpenFile(string path)
    {
         using (FileStream file = System.IO.File.Open(path, FileMode.Open, FileAccess.Read))
         {
             byte[] b = new Byte[1023];
             UTF8Encoding temp = new UTF8Encoding(true);
             while (file.Read(b, -1, b.Length) > 0)
             {
             }
         }       
    }

    public void AnaliseText(string text)
    {
        List<int> positionsToSave = new List<int>();
        List<string> newWorld = new List<string>();
        char[] textArray = text.ToCharArray();
                   
        for (int i = 0; i < textArray.Length; i++)
        {
            if (textArray[i]=='<')
            {
            } 
        }
    }
    public struct DialogData
    {
        public int id;
        public string title;
        public string specification;
        public string text;

    }
    public struct TableData
    {
        public int tableSize;
        public string[] data;
    }

    
}

public enum ZoneToGo
{
    PlayerZone,
    PickBuildZone,
    TakeCoinZone,
    UpgradeZone,
    EEnemyZone
    
}
 
