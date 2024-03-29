using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_In_F_Out_Obj : MonoBehaviour
{
    public Animator animator;
    public static Action OnFadeInSkillElements;
    public static Action OnFadeInStatsElementsWeapon_1;
    public static Action OnFadeInStatsElementsWeapon_2;
    public static Action OnSetElementsWithWeapon_1;
    public static Action OnSetElementsWithWeapon_2;
    
    
    
    public UIElement uiElement;


    private void OnEnable()
    {
        if (uiElement==UIElement.SkillPanel)
        {
            OnFadeInSkillElements+=FadeIn;
        }
        else if (uiElement==UIElement.StatsPanel)
        {
            OnFadeInStatsElementsWeapon_1+=FadeIn;
            OnFadeInStatsElementsWeapon_2+=FadeIn;
            OnSetElementsWithWeapon_1+=SetStatsPanel;
            OnSetElementsWithWeapon_2+=SetStatsPanel;
        }
    }

    private void OnDisable()
    {
         if (uiElement==UIElement.SkillPanel)
         {
             OnFadeInSkillElements+=FadeIn;
         }
         else if (uiElement==UIElement.StatsPanel) 
         {
            OnFadeInStatsElementsWeapon_1-=FadeIn;
            OnFadeInStatsElementsWeapon_2-=FadeIn; 
            OnSetElementsWithWeapon_1-=SetStatsPanel;
            OnSetElementsWithWeapon_2-=SetStatsPanel;
         }
         
        
    }

    public void SetStatsPanel()
    {
        animator.Play("SetStats");
    }
    
    public void FadeIn()
    {
        
        animator.Play("FadeIn");
    }
    
    public void FadeOut()
    {
        animator.Play("FadeOut");
    }
    
    public void Activate()
    {
        gameObject.SetActive(true);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
    
}

public enum UIElement
{
    SkillPanel,
    StatsPanel,
    BuildPanel,
    Pause
}