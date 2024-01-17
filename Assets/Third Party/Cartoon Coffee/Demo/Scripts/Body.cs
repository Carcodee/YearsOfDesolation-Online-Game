using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Demo_Project.SceneManager;

namespace Demo_Project
{
    public class Body : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SceneManager.listOfBodies.Add(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}