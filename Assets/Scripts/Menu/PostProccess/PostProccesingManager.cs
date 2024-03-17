using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProccesingManager : MonoBehaviour
{
    public static PostProccesingManager instance;
    
    [Header("PostProcessing")]
    public Volume volume;
    // Start is called before the first frame update

    public Material fullScreenPassMaterial;
    public Material fullScreenPassMaterialMenu;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Update is called once per frame

    public void DeactivateMenuBlur()
    {
        fullScreenPassMaterialMenu.SetFloat("_ActivateBlur", 0);
    }

    public void ActivateBlur(float intensity)
    {
        fullScreenPassMaterial.SetFloat("_ActivateBlur", intensity);
    }

    private void OnApplicationQuit()
    {
        fullScreenPassMaterial.SetFloat("_ActivateBlur", 0);
        fullScreenPassMaterialMenu.SetFloat("_ActivateBlur", 1);

        
    }

    public void ApplyPostProcessing(bool isActive)
    {
        if (isActive)
        {
            if (volume.profile.TryGet<OutsideOfZone>(out var zonePostprocessing))
            {
                zonePostprocessing.vigneeteIntensity.value = 1.0f;
                zonePostprocessing.intensity.value = 1.0f;

            }    
        }
        else
        {
            if (volume.profile.TryGet<OutsideOfZone>(out var zonePostprocessing))
            {
                zonePostprocessing.intensity.value =  0.0f;
                zonePostprocessing.vigneeteIntensity.value =0.0f;
            }
        }
        
        
    }

}
