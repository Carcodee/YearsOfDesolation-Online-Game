using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager :NetworkBehaviour
{
    public static GameManager Instance;


    private void Awake()
    {

        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    void Start()
    {
        

    }

    void Update()
    {
        
    }
}
