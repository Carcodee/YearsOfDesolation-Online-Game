using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingTextController : MonoBehaviour
{
    
    public Camera mainCam;
    public TextMeshPro text;
    //TODO: Add animation properties
    void Start()
    {
        Destroy(gameObject, 1f);   
    }

    void Update()
    {
        LookAtCamera();
        transform.position += Vector3.up * Time.deltaTime * 2;
        
        text.alpha -= Time.deltaTime;
    }
    void LookAtCamera()
    {
        transform.LookAt(mainCam.transform.position, Vector3.up);
        //create a better solution for this
        transform.forward = -transform.forward;
        
    
    }
    
}
