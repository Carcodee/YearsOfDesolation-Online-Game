using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{

    public static TutorialManager instance;

    private void Awake()
    {
        if (instance!=null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

}