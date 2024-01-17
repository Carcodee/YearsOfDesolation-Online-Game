using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHudController : NetworkBehaviour
{
    public CanvasController canvasController;
    void Start()
    {
        InitializateComponents();
    }

    void Update()
    {
        
    }
    void InitializateComponents()
    {
        canvasController = GetComponentInChildren<CanvasController>();
    }
}
