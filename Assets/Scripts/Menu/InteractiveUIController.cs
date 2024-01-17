using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveUIController : MonoBehaviour
{
    public Transform targetPos;
    void Start()
    {
        targetPos = transform;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            targetPos.Rotate(new Vector3(0, 0.1f, 0)) ;
        }
        if (Input.GetKey(KeyCode.A))
        {
            targetPos.Rotate(new Vector3(0, -0.1f, 0)) ;
        }
    }
}
