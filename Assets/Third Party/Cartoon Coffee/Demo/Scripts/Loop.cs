using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Demo_Project.SceneManager;


namespace Demo_Project
{
    public class Loop : MonoBehaviour
    {
        // Start is called before the first frame update
        public Vector3 loopPosition = new Vector3(0, 0, 0);
        void Start()
        {
            SceneManager.listOfLoops.Add(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
