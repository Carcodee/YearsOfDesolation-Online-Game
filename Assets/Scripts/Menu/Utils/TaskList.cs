using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskList : MonoBehaviour
{ 
    public List<TaskOject> tasks=new List<TaskOject>();

    public void InitTaskList()
    {
        foreach (var task in tasks)
        {
            task.Init();
        } 
    }
}
