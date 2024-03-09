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
