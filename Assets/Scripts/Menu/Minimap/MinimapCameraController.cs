using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{

    public PlayerController playerController;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
    }
    public void FollowPlayer()
    {
        transform.position = new Vector3(playerController.transform.position.x, transform.position.y, playerController.transform.position.z);
    }
}
