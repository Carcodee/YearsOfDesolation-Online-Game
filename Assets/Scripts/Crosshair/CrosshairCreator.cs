using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairCreator : MonoBehaviour
{
    public CrosshairScriptableObj crosshairScriptableObj;
    public CrossHair crossHair;
    public CrosshairItem playerCrosshair;
    public float currentGapPrecision;
    public static Action <hitType>OnHitDetected;
    public float time = 0.5f;
    
    public bool isForEditor=false;

    public PlayerController playerController;
    private void OnEnable()
    {
        if (isForEditor)
        {
            return;
        }
        OnHitDetected += DisplayDamage;
    }

    private void OnDisable()
    {
        if (isForEditor)
        {
            return;
        }
        OnHitDetected -= DisplayDamage;
    }
    void Start()
    {
        crossHair = new CrossHair(crosshairScriptableObj.width,  crosshairScriptableObj.length, 
            crosshairScriptableObj.gap,crosshairScriptableObj.isStatic, crosshairScriptableObj.color);
        LoadCrosshair(crossHair);
        if (isForEditor)
        {
            return;
        }
        playerController = GetComponentInParent<PlayerController>();
    }


    void Update()
    {
        if (isForEditor)
        { 
            currentGapPrecision = crosshairScriptableObj.gap;
            LoadCrosshair(crossHair);
            return;
        }
        currentGapPrecision = playerController.currentAimShootPercentage;
        LoadCrosshair(crossHair);

    }
    public void DisplayDamage(hitType hitType)
    {
        switch (hitType)
        {
            case hitType.Head:
                StartCoroutine(playerCrosshair.DisplayDamage(time, Color.red));
                break;
            case hitType.Chest:
                StartCoroutine(playerCrosshair.DisplayDamage(time, Color.yellow));
                break;
            // case hitType.Legs:
            //     StartCoroutine(playerCrosshair.DisplayDamage(time, Color.green));
            //     break;
            default:
                StartCoroutine(playerCrosshair.DisplayDamage(time, Color.yellow));
                break;                
        }
    }
    void LoadCrosshair(CrossHair crossHair)
    {
        playerCrosshair.SetLenght(crossHair.length);
        playerCrosshair.SetWidth(crossHair.width);
        crossHair.SetRecoilGap(currentGapPrecision);
        if (isForEditor)
        {
            crossHair.SetRecoilGap(1);
        }
        playerCrosshair.SetGap(crossHair.gap);
        playerCrosshair.SetColor(crossHair.color);
        
    }
    void SetCrosshairExpansion(float expansion)
    {

    }
}

[System.Serializable]
public class CrossHair
{
    public float width;
    public float gap;
    public float length;
    public bool isStatic;
    public float gapBuffer;
    public Color color;
    public CrossHair(float width, float length, float gap, bool isStatic, Color color)
    {
        this.width = width;
        this.gap = gap;
        this.length = length;
        this.isStatic = isStatic;
        this.color = color;
        gapBuffer = gap;
    }

    public void SetWidth(float newWidth)
    {
        this.width = width;
    }
    public void SetGap(float gap)
    {
        this.gap = gap;
    }
    public void SetLength(float newLength)
    {
        this.length = length;
    }

    public void SetRecoilGap(float currentGapPrecision)
    {
        if (!isStatic)
        {
            gap =  gapBuffer * currentGapPrecision ;
        }
        
    }

}