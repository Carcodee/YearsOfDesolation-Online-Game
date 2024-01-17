using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Demo_Project
{
    public class DeactivateChildren : MonoBehaviour
    {
        // Start is called before the first frame update
        // Deactivates the particles, and burst upon the start of the scene
        void Start()
        {
            int childCount = transform.childCount;

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}