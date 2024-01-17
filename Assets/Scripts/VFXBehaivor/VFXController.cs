using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    public float time;
    private void Start()
    {
        //destroy after 2 seconds
        Destroy(gameObject, time);
    }
}
