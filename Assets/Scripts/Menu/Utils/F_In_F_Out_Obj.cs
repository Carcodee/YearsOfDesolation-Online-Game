using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_In_F_Out_Obj : MonoBehaviour
{
    public Animator animator;
    public static Action OnFadeInSkillElements;
    public static Action OnFadeOutSkillElements;
    public static Action OnBuyDecliend;
    public static Action OnBuyAccepted;
    public static Action OnFadeInStatsElementsWeapon_1;
    public static Action OnFadeInStatsElementsWeapon_2;
    public static Action OnSetElementsWithWeapon_1;
    public static Action OnSetElementsWithWeapon_2;
    public static Action OnBuildSelected;
    public static Action OnWeapontChangedAnim;
    public static Action <string> OnInfoTextDisplayed;
    public static Action OnCleanScreen;
    
    public UIElement [] uiElement;


    private void OnEnable()
    {
        if (uiElement.Length==0)
        {
           Debug.Log("No UIElement selected from: "+ gameObject.name); 
        }
        for (int i = 0; i < uiElement.Length; i++)
        {
            if (uiElement[i]==UIElement.CleanableObject)
            {

                OnCleanScreen += RemoveTutorialInfo;
            }
            if (uiElement[i]==UIElement.SkillPanel)
            {
                OnFadeInSkillElements+=FadeIn;
            }
            if (uiElement[i]==UIElement.StatsPanel)
            {
                OnFadeInStatsElementsWeapon_1+=FadeInStats;
                OnFadeInStatsElementsWeapon_2+=FadeInStats;
                OnSetElementsWithWeapon_1+=SetStatsPanel;
                OnSetElementsWithWeapon_2+=SetStatsPanel;
            }

            if (uiElement[i]==UIElement.BuildSelected)
            {
                OnBuildSelected+=BuildSelected;
            }

            if (uiElement[i]==UIElement.FadeOutSkillElements)
            {
                OnFadeOutSkillElements+=FadeOut;
            }

            if (uiElement[i] == UIElement.DeclinedBuy)
            {
                OnBuyDecliend+=BuyDeclined;
            }

            if (uiElement[i]==UIElement.AcceptedBuy)
            {
                OnBuyAccepted+=BuyAccepted;
            }

            if (uiElement[i] == UIElement.WeaponChanged)
            {
                OnWeapontChangedAnim += WeaponChanged;
            }

            if (uiElement[i]==UIElement.DisplayTutorial)
            {
                OnInfoTextDisplayed += DisplayTutorialInfo;
            }
            
                
            
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < uiElement.Length; i++)
        {
            if (uiElement[i]==UIElement.CleanableObject)
            {
            
                OnCleanScreen -= RemoveTutorialInfo;
            }
            if (uiElement[i]==UIElement.SkillPanel)
            {
                  OnFadeInSkillElements+=FadeIn;
            }
            if (uiElement[i]==UIElement.StatsPanel) 
            {
                 OnFadeInStatsElementsWeapon_1-=FadeInStats;
                 OnFadeInStatsElementsWeapon_2-=FadeInStats; 
                 OnSetElementsWithWeapon_1-=SetStatsPanel;
                 OnSetElementsWithWeapon_2-=SetStatsPanel;
            }
            if (uiElement[i]==UIElement.BuildSelected)
            {
                OnBuildSelected-=BuildSelected;
            }

            if (uiElement[i]==UIElement.FadeOutSkillElements)
            {
                OnFadeOutSkillElements-=FadeOut;
            }
            if (uiElement[i] == UIElement.DeclinedBuy)
            {
                OnBuyDecliend-=BuyDeclined;
            }

            if (uiElement[i]==UIElement.AcceptedBuy)
            {
                OnBuyAccepted-=BuyAccepted;
            }

            if (uiElement[i]==UIElement.WeaponChanged)
            {
                OnWeapontChangedAnim -= WeaponChanged;
            }

            if (uiElement[i]==UIElement.DisplayTutorial)
            {


                OnInfoTextDisplayed -= DisplayTutorialInfo;
            }
        }
        
        
    }

    public void FadeInStats()
    {
        animator.Play("FadeInStats");
    }
    public void SetStatsPanel()
    {
        animator.Play("SetStats");
    }
    public void BuildSelected()
    {
        animator.Play("BuildSelected");
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
    public void BuyAccepted()
    {
        animator.Play("BuyAccepted");
    }
    public void BuyDeclined()
    {
        animator.Play("BuyDeclined");
    }

    public void WeaponChanged()
    {
        animator.Play("WeaponChange");
    }

    public void DisplayTutorialInfo(string text)
    {
        NotificationItem.OnTextDisplay?.Invoke(text);
        animator.Play("DisplayText");
    }

    public void RemoveTutorialInfo()
    {
        animator.Play("QuitText"); 
    }
    
}

public enum UIElement
{
    SkillPanel,
    StatsPanel,
    BuildSelected,
    FadeOutSkillElements,
    DeclinedBuy,
    AcceptedBuy,
    WeaponChanged,
    DisplayTutorial,
    CleanableObject,
    Pause
}