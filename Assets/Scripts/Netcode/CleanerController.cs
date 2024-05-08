using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CleanerController : MonoBehaviour 
{

    public static CleanerController instance;
    private List<INetObjectToClean> objectsToClean;

    [ContextMenu("Show items names")]
    public void DisplayData()
    {
        Debug.Log("Pending Objects to clean: " + objectsToClean.Count);
    }
    private void Awake()
    {
        if (instance==null)
        {
            instance = this;
            objectsToClean = new List<INetObjectToClean>();
            Debug.Log("ObjectFinded");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void AddObjectToList(INetObjectToClean objectToClean)
    {
        this.objectsToClean.Add(objectToClean);
    }
    public void StopLogic(bool val)
    {
        for (int i = 0; i < objectsToClean.Count; i++)
        {
            objectsToClean[i].shutingDown = val;
        }
    }

    public void Clean()
    { 
        for (int i = 0; i < objectsToClean.Count; i++)
        {
           objectsToClean[i].CleanData();
        } 
        objectsToClean.Clear();
    }
}
