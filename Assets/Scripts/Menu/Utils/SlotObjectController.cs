using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.Heat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotObjectController : MonoBehaviour
{

    public string UpgradeName;
    public TextMeshProUGUI UpgradeText;
    public Image[] slotsFill;
    public Color emptySlotColor= Color.white;
    public Color fullSlotColor= Color.blue;
    public Color previewSlotColor= Color.green;
    public float previewTimeBlink = 0.7f;
    public bool isOnPreview = false;
    public int currentSlotsUnlocked=0; 
    public int slotsToPreview=0;
    public UpgradeType upgradeType;    
    
    // public ButtonManager addButton;
    private void OnValidate()
    {
        // UpgradeText.text = UpgradeName;
    }


    public void FillSlots(int start, int end, Color color)
    {
        for (int i = start; i < end; i++)
        {
            Debug.Log("Filling slot");
            slotsFill[i].color = color;
        }
    }

    IEnumerator PreviewUpgrade()
    {
        float time = 0;
        while (isOnPreview)
        {
        
            time += Time.deltaTime;
            for (int i = currentSlotsUnlocked; i < slotsToPreview; i++)
            {
                slotsFill[i].color = Color.Lerp(emptySlotColor, fullSlotColor, Mathf.Sin(time*2)+1);
            }
            yield return null;
        }
        for (int i = currentSlotsUnlocked; i < slotsToPreview; i++)
        {
            slotsFill[i].color = Color.blue;
        }
        slotsToPreview = 0;
    }
    
}
