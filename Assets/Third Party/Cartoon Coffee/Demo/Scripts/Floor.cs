using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Demo_Project.SceneManager;

namespace Demo_Project
{

    public class Floor : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SceneManager.listOfFloors.Add(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}