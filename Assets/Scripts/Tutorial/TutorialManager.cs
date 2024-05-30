using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.TextCore.Text;
using TextAsset = UnityEngine.TextAsset;

public class TutorialManager : MonoBehaviour
{

    public static TutorialManager instance;
    public ZoneToGo currentHUDStage = ZoneToGo.PlayerZone;

    public PlayerController playerRef;

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
    public Transform spawnAmmoPoint;
    private TutorialStagesHandler tutorialStagesHandler => TutorialStagesHandler.instance; 
    
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
        if (GameManager.Instance.localPlayerRef==null)return;
        if (GameManager.Instance.localPlayerRef.playerStats.hasPlayerSelectedBuild && currentHUDStage==ZoneToGo.PickBuildZone)
        {
            //the player selected a build then it continue with the next step
            currentHUDStage = ZoneToGo.TakeCoinZone;
            wasTutorialStepDone = false;
        }
        // CheckCurrentZoneToGo();
    }

    private void Start()
    {
        InitData();
        StartCoroutine(SetPlayerInPos());
        // CheckCurrentZoneToGo();
    }

    public void SpawnCoinAtPos()
    { 
        playerRef.playerStats.SpawnCoin(playerRef.playerStats.coin, spawnAmmoPoint.position);
    }

    public IEnumerator SetPlayerInPos()
    {
        
        if (GameManager.Instance.localPlayerRef==null)
        {
            yield return null;
        }

        GameManager.Instance.localPlayerRef.transform.position = spawnPoint.position;
        GameManager.Instance.localPlayerRef.transform.rotation= Quaternion.Euler(0,-180,0);
        playerRef = GameManager.Instance.localPlayerRef;
        AudioManager.instance.menuListener.enabled = false;
        yield return new WaitForSeconds(2.0f);
        GameManager.Instance.ActivateLoadingScreen(false);
        
        tutorialStagesHandler.Init();
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
        TutorialStagesHandler.instance.FinishDialogs();
        AudioManager.instance.EventSucceded();
        F_In_F_Out_Obj.OnCleanScreen?.Invoke();
        

    }
    public void FinishUIIntruction()
    {
        TutorialStagesHandler.instance.FinishInstructions();
    }
    public void DisplayTutorialData(int currentDialogCounter)
    {
        string CheckNextHUB = GetValueFromIndex(dialogCounter, 2 + HUBCounter + 1);
        dialogCounter = currentDialogCounter;
        tutorialTextData.id = int.Parse(GetValueFromIndex(dialogCounter, 0));
        // Debug.Log(tutorialTextData.id);
        tutorialTextData.specification = GetValueFromIndex(dialogCounter, 1);
        // Debug.Log(tutorialTextData.specification);
        tutorialTextData.text =GetValueFromIndex(dialogCounter, 2+HUBCounter); 
        tutorialTextData.text = AnaliseText(tutorialTextData.text, "red", true);
        // Debug.Log(tutorialTextData.text);
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

    public string AnaliseText(string text, string color, bool bold)
    {

        List<string> slicedStrings = new List<string>();
        slicedStrings = text.Split('$', StringSplitOptions.None).ToList();
        for (int i = 0; i < slicedStrings.Count; i++)
        {
            if (slicedStrings[i].Contains("key"))
            {
                int kPos = -1;
                foreach (char charsInKey in slicedStrings[i])
                {
                    kPos++;
                    if (charsInKey=='k')
                    { 
                        break; 
                    }
                } 
                slicedStrings[i]= slicedStrings[i].Remove(kPos,3);
                string prefix = $"<color={color}>"; 
                string final = "</color>";
                if (bold)
                {
                    prefix = prefix + "<b>";
                    final = final + "</b>";
                }

                slicedStrings[i] = prefix + slicedStrings[i] + final;   
            }
        }
        string formatedString = "";
        foreach (var slicedString in slicedStrings)
        {
            formatedString += slicedString;
        }
        return formatedString;
    }
    public struct DialogData
    {
        public int id;
        public string title;
        public string specification;
        public string text;
    }
    [System.Serializable]
    public struct TableData
    {
        public int tableSize;
        public string[] data;
    }
}

public enum TutorialStage
{
    Intro,
    PlayerZone,
    PickBuild,
    PickAmmo,
    UpgradeBuild,
    EnemyKill,
    ZoneComing
}
public enum ZoneToGo
{
    PlayerZone,
    PickBuildZone,
    TakeCoinZone,
    UpgradeZone,
    EEnemyZone
    
}
 
