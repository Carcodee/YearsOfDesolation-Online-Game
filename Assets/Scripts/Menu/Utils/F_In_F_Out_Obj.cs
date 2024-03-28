using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_In_F_Out_Obj : MonoBehaviour
{
    public Animator animator;
    public static Action OnFadeInSkillElements;
    public UIElement uiElement;


    private void OnEnable()
    {
        if (uiElement==UIElement.SkillPanel)
        {
            OnFadeInSkillElements+=FadeIn;
        }
    }

    private void OnDisable()
    {
         if (uiElement==UIElement.SkillPanel)
         {
             OnFadeInSkillElements+=FadeIn;
         }       
        
    }

    public void FadeIn()
    {
        
        animator.Play("FadeIn");
    }
    
    public void FadeOut()
    {
        animator.Play("FadeOut");
    }
}

public enum UIElement
{
    SkillPanel,
    StatsPanel,
    BuildPanel,
    Pause
}