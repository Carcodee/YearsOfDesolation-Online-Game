using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Demo_Project.SceneManager;


namespace Demo_Project
{
    public class Head : MonoBehaviour
    {
        float minimumAngle = 0;
        float maximumAngle = 0;
        public float dividerRate = 3;
        // Start is called before the first frame update
        void Start()
        {
            SceneManager.listOfHeads.Add(this.gameObject);
        }

        // Moves the head based on the angle of the mouse
        public void MoveHead()
        {
            Quaternion rot = new Quaternion();
            float shootAngle = GetAngle() * Mathf.Rad2Deg;
            if (shootAngle > maximumAngle)
            {
                shootAngle = maximumAngle;
            }

            if (shootAngle < minimumAngle)
            {
                shootAngle = minimumAngle;
            }
            shootAngle = shootAngle / dividerRate;
            rot.eulerAngles = new Vector3(0, 0, shootAngle);
            transform.rotation = rot;
        }

        // Gets the angles in radians
        public float GetAngle()
        {
            Vector3 trueMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            return Mathf.Atan2(trueMousePosition.y - transform.position.y, trueMousePosition.x - transform.position.x);
        }

        // Sets the angles based off what was received from the arm
        public void SetAngles(float min, float max)
        {
            minimumAngle = min;
            maximumAngle = max;
        }

        // Update is called once per frame
        void Update()
        {
            MoveHead();
        }
    }
}