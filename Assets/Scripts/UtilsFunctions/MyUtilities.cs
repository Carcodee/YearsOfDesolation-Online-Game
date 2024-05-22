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
    
    public static bool IsAnimationPlaying(Animator animator, string stateName,int Layer)
    {
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(Layer);
        float currentTime = stateInfo.length * stateInfo.normalizedTime;
        bool result = currentTime < stateInfo.length;
        return result;
        
    }
    public static bool IsThisAnimationPlaying(Animator animator, string stateName, int layer)
    {
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        bool result = stateInfo.IsName(stateName);
        return result;
    }

    public static void PlaySoundAtFrame(Animator animator, AudioClip audioClip)
    {
        
    }
    public static void SetDefaultUpperLayer(Animator animator, string newLayerName, string oldLayerName)
    {
        animator.SetLayerWeight(animator.GetLayerIndex(oldLayerName), 0);
        animator.SetLayerWeight(animator.GetLayerIndex(newLayerName), 1);
    }

    public static IEnumerator LerpToValueMaterial(float startValue, float endValue, float duration, Material mat, string propertyName)
    {
        float time = 0;
        while (time < duration)
        {

            time += Time.deltaTime;
            float normalizedTime = time / duration;
            mat.SetFloat(propertyName, Mathf.Lerp(startValue, endValue, normalizedTime));
            yield return null;
        }
        mat.SetFloat(propertyName, endValue);
    }

    public static IEnumerator LerpToValueMaterialAnimationCurve(float startValue, float endValue, float duration, Material mat, string propertyName, AnimationCurve animationCurve) 
    {
        float time = 0;
        while (time < duration)
        {

            time += Time.deltaTime;
            float normalizedTime = time / duration;
            mat.SetFloat(propertyName, Mathf.Lerp(startValue, endValue, animationCurve.Evaluate(normalizedTime)));
            yield return null;
        }
        mat.SetFloat(propertyName, endValue);
    }
    public static IEnumerator EasyLerpToValue<T>(float startValue, float endValue, float duration, AnimationCurve animationCurve, T valueToLerp)
    {
        float time = 0;
        while (time < duration)
        {

            time += Time.deltaTime;
            float normalizedTime = time / duration;
            
            Mathf.Lerp(startValue, endValue, Mathf.Lerp(0, 1,animationCurve.Evaluate(normalizedTime)));
            yield return null;
        }
    }
    public static void LerpTo(float startVal, float endVal, ref float time, ref float valueToChange, float duration, AnimationCurve animationCurve)
    {
        time += Time.deltaTime;
        float normalizedTime = time / duration;
        valueToChange = Mathf.Lerp(startVal, endVal, animationCurve.Evaluate(normalizedTime));
    }
}