using System.Collections.Generic;
using UnityEngine;

public class TaskList : MonoBehaviour
{
    public static TaskList instance;
    public TaskOject taskPrefab;
    public List<taskData> tasksToCreate=new List<taskData>();
    public Transform container;
    
    public Dictionary<string,TaskOject> realTask = new Dictionary<string,TaskOject>();

    public Animator animator;
    public int maxListCount=3;

    private void Awake()
    {
        if (instance==null)
        {
           instance = this;
           
           return; 
        } 
        Destroy(this); 
       
    }

    public void Start()
    {
    }

    public void RemoveTaksObjFromKey(string key,int indexToRemove)
    {
        TaskList.instance.realTask.GetValueOrDefault(key).Done();
        TaskList.instance.tasksToCreate.RemoveAt(indexToRemove);
    }
    public void StartTaskList()
    {
        animator.Play("TaskListFadeIn");
    }
    public void InitTaskList()
    {
        if (container.childCount>0)
        {
            foreach (Transform child in container)
            {
               Destroy(child.gameObject); 
            }
        }

        int currentListCount = 0;
        realTask.Clear();
        foreach (var data in tasksToCreate)
        {
            currentListCount++;
            TaskOject newTask = Instantiate(taskPrefab, container);
            newTask.description = data.description;
            newTask.details = data.details;
            newTask.number = data.number;
            newTask.Init();
            realTask.Add(newTask.description,newTask);
            if (currentListCount>=maxListCount)
            {
               break;
            }
        } 
        
    }
    

    [System.Serializable]
    public struct taskData
    {
        public string description;
        public string details;
        public int number;

    } 
    
    
}
