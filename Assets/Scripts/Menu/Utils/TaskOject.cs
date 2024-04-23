using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskOject : MonoBehaviour
{


    public TextMeshProUGUI descriptionText;  
    public TextMeshProUGUI detailsText;  
    public TextMeshProUGUI numberText;  
    
    public string description;
    public string details;
    public int number;

    public Animator animator;

    public void Init()
    {
        
        descriptionText.text = description;
        detailsText.text = details;
        numberText.text = number.ToString();
        animator.Play("TaskObjFadeIn");
    }
    public void Done()
    {
        animator.Play("TaskObjFadeOut");
    }

    public void DestroyObj()
    {
        transform.parent = null;
        TaskList.instance.InitTaskList();   
        Destroy(gameObject);
    }

}
