using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.TextCore.Text;
using TextAsset = UnityEngine.TextAsset;

public class TutorialManager : MonoBehaviour
{

    public static TutorialManager instance;


    public string fileName;
    private int dialogCounter=1;
    public int HUBCounter=0;
    public int maxHubCounter = 5;
    public TextAsset dialogs;
    public DialogData tutorialTextData;
    public TableData tableData;
    public HUDStage currentHUDStage = HUDStage.BeforeProvingGrounds;
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

    
    private void Start()
    {
        
        InitData();
        DisplayTutorialData(1);
        PlayerComponentsHandler.IsCurrentDeviceMouse = true;
        F_In_F_Out_Obj.OnInfoTextDisplayed?.Invoke(tutorialTextData.text);
        
        
    }

    public void NextHUB()
    {
        if (HUBCounter<maxHubCounter)
        {
            DisplayTutorialData(dialogCounter);
            HUBCounter++;
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
        HUBCounter = 0;
        F_In_F_Out_Obj.OnCleanScreen?.Invoke();
    }
    void DisplayTutorialData(int currentDialogCounter)
    {
        string HUBText= GetValueFromIndex(dialogCounter, 2+HUBCounter);
        dialogCounter = currentDialogCounter;
        if (HUBText=="none" && HUBCounter<maxHubCounter)
        {
            HUBCounter = maxHubCounter;
            return;
        }
        tutorialTextData.id = int.Parse(GetValueFromIndex(dialogCounter, 0));
        Debug.Log(tutorialTextData.id);
        tutorialTextData.specification = GetValueFromIndex(dialogCounter, 1);
        Debug.Log(tutorialTextData.specification);
        tutorialTextData.text =HUBText;
        Debug.Log(tutorialTextData.text);
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

public enum HUDStage
{
    BeforeProvingGrounds,
    BuildPick,
    OnShootingGrounds,
    NoAmmo,
    AmmoSearch,
    UpgradeablePanel,
    BattleRoyale
}
 
