using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerHandler : MonoBehaviour
{
    public static GameObject instance;

    private void Awake()
    {
        if (instance==null)
        {
            instance = gameObject;
        }
        else
        {
            Destroy(gameObject);
        } 
        DontDestroyOnLoad(gameObject);
    }
}

