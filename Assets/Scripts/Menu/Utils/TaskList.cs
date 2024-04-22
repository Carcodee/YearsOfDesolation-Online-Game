using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class TaskList : MonoBehaviour
{
    public static TaskList instance;
    public TaskOject taskPrefab;
    public taskData [] tasksToCreate;
    public Transform container;
    
    public Dictionary<string,TaskOject> realTask = new Dictionary<string,TaskOject>();

    public Animator animator;

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

    public void StartTaskList()
    {
        animator.Play("TaskListFadeIn");
    }
    public void InitTaskList()
    {
        foreach (var data in tasksToCreate)
        {
            TaskOject newTask = Instantiate(taskPrefab, container);
            newTask.description = data.description;
            newTask.details = data.details;
            newTask.number = data.number;
            newTask.Init();
            realTask.Add(newTask.description,newTask);
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
