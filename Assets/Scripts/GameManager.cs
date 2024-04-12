using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager :NetworkBehaviour
{
    public static GameManager Instance;

    public bool isOnTutorial=false;

    private void Awake()
    {

        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    public override void OnDestroy()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadTutorialConfigs()
    {
        
    }

    public void LoadDeafaults()
    {
        
    }
    void Start()
    {
        
        

    }

    void Update()
    {
        
    }
}
