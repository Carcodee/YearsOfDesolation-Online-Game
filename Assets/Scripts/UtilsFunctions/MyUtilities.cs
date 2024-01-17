using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Cinemachine.Utility;
using Unity.Netcode;

public static class MyUtilities
{

    
    public static void StartCameraShake(this CinemachineVirtualCamera virtualCamera, float amplitudeGain, float frequencyGain, float duration)
    {

        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = amplitudeGain;
        cinemachineBasicMultiChannelPerlin.m_FrequencyGain = frequencyGain;
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(cinemachineBasicMultiChannelPerlin.m_AmplitudeGain, 0f, duration);
        cinemachineBasicMultiChannelPerlin.m_FrequencyGain = Mathf.Lerp(cinemachineBasicMultiChannelPerlin.m_FrequencyGain, 0f, duration);

    }
    public static void StopCameraShake(this CinemachineVirtualCamera virtualCamera)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        cinemachineBasicMultiChannelPerlin.m_FrequencyGain = 0;
    }
    
    
}