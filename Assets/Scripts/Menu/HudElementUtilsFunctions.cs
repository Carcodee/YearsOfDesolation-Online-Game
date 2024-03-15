using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudElementUtilsFunctions : MonoBehaviour
{
   
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
    public void Activate()
    {
        gameObject.SetActive(true);
    }
}
