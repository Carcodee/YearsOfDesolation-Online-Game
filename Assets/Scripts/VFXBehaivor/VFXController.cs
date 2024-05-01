using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class VFXController : MonoBehaviour
{
    public bool concatenateVFX = false;
    public float time;
    public int id =-1;
    public HandleVFX handleVFX;
    private void Start()
    {
        //destroy after 2 seconds
        if (concatenateVFX)
        {
            handleVFX = PlayerVFXController.GetVFXHandle(id);
        }
        Destroy(gameObject, time);
    }

    private void OnDestroy()
    {
        if  (concatenateVFX)
        {
            
            handleVFX.CreateVFX(transform.position, transform.rotation, false);
        }
    }
}
