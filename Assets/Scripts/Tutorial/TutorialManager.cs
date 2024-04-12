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

    private int HUBCounter=2;
    
    public TextAsset dialogs;
    
    public DialogData tutorialTextData;

    public TableData tableData;

   private void Awake()
   {
        fileName = Application.dataPath + "/TextFiles/Tutorial/TutorialDialog.csv";
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
        dialogCounter = 1;
        HUBCounter = 0; 
        SetUpTutorialData();
        F_In_F_Out_Obj.OnInfoTextDisplayed?.Invoke(tutorialTextData.text);
        
        
    }

    void SetUpTutorialData()
    {
        if (HUBCounter>4)
        {
            HUBCounter = 0;
        }
        tutorialTextData.id = int.Parse(GetValueFromIndex(dialogCounter, 0));
        Debug.Log(tutorialTextData.id);
        tutorialTextData.specification = GetValueFromIndex(dialogCounter, 1);
        Debug.Log(tutorialTextData.specification);
        tutorialTextData.text = GetValueFromIndex(dialogCounter, 2+HUBCounter);
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
       // foreach (var VARIABLE in tableData.data)
       // {
       //    Debug.Log("Index: "+ counter +": " +VARIABLE); 
       //     counter++;
       // }
       
       

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
 
