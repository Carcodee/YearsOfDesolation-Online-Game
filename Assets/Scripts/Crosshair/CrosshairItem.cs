using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairItem : MonoBehaviour
{
    public Image[] crosshairImages;
    public CanvasGroup damageImages;
    bool posRefreshed;



    void Start()
    {
        
    }
    public void SetLenght(float lenght)
    {
        foreach (Image image in crosshairImages)
        {
            image.rectTransform.sizeDelta = new Vector2(lenght, image.rectTransform.sizeDelta.y);
        }
    }
    
    public void SetColor(Color newColor)
    {
        foreach (Image image in crosshairImages)
        {
            image.color = newColor;
        }
    }
    public void SetWidth(float width)
    {
        foreach (Image image in crosshairImages)
        {
            image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, width);
        }
    }

    public void SetGap(float gap)
    {
        crosshairImages[0].rectTransform.anchoredPosition = new Vector2(crosshairImages[0].rectTransform.anchoredPosition.x, gap);
        crosshairImages[1].rectTransform.anchoredPosition = new Vector2(crosshairImages[1].rectTransform.anchoredPosition.x, -gap);
        crosshairImages[2].rectTransform.anchoredPosition = new Vector2(gap, crosshairImages[2].rectTransform.anchoredPosition.y);
        crosshairImages[3].rectTransform.anchoredPosition = new Vector2(-gap, crosshairImages[3].rectTransform.anchoredPosition.y);

    }
    public IEnumerator DisplayDamage(float time)
    {
        damageImages.alpha = 1;
        while (damageImages.alpha>0)
        {
            damageImages.alpha -= Time.deltaTime / time;
            yield return null;
        }
        damageImages.alpha = 0;
    }
}
