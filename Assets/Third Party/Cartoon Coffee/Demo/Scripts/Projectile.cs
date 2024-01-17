using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Demo_Project.SceneManager;

namespace Demo_Project
{
    public class Projectile : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject impactObject = null;
        public GameObject muzzleFlashObject = null;
        public GameObject chargingObject = null;
        public bool isChargeable = false;
        public bool rotateSprite = true;
        public bool muzzleFlash = true;
        public Color muzzleFlashColor = Color.white;
        public Color chargeColor = Color.white;

        public bool explodeAtScreenEdge = true;

        public float moveAngle = 0;
        public float spriteAngle = 0;
        public float moveSpeed = 5;
        public float angleRandomness = 5;
        //float thetaStep = Mathf.PI / 32f;
        //float theta = 0f;
        //float amplitude = 4f;
        float xOffset = 0;
        //float waveFrequency = 2;
        //int waveDirection = 1;

        public Vector2 bulletOriginPoint = new Vector2(.36f, 0);
        public Vector2 muzzleFlashOriginPoint = new Vector2(0, 0);
        public Vector2 chargeOriginPoint = new Vector2(0, 0);

        //Faking Sine wave
        bool rotateClockwise = false;
        public float rotationSpeed = 0;
        public float rotationRange = 0;

        float timeSinceLastFrame = 0;

        void Start()
        {
            SceneManager.listOfBullets.Add(this.gameObject);
            xOffset = transform.position.x;
            if (rotateSprite == true)
            {
                transform.rotation = Quaternion.Euler(0, 0, spriteAngle * Mathf.Rad2Deg);
            }
        }


        // Checks if the projectile is off screen.
        void CheckIfOffScreen()
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);


            if (screenPosition.y < 0 || screenPosition.x > Screen.width || screenPosition.y > Screen.height)
            {
                if (explodeAtScreenEdge == true)
                {
                    Impact();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }


        // Moves the projectile
        void Move()
        {
            float tempMoveSpeed = (moveSpeed * timeSinceLastFrame) / 100;
            transform.Translate(new Vector3(Mathf.Cos(moveAngle) * tempMoveSpeed, Mathf.Sin(moveAngle) * tempMoveSpeed, 0), Space.World);
            /* 

             float newXPos = waveDirection * amplitude * Mathf.Sin(theta * waveFrequency) + xOffset;
             float xStep = newXPos - transform.position.x;

             transform.Translate(new Vector3(xStep/100, (moveSpeed * Time.deltaTime)/100));

             theta += thetaStep;
             */


        }
        // Generate the impact
        void Impact()
        {
            GameObject tempImpact = Instantiate(impactObject, transform.position, transform.rotation);
            SceneManager.listOfImpacts.Add(tempImpact);
            for(int i = 0; i < SceneManager.listOfBullets.Count; i++)
            {
                if(SceneManager.listOfBullets[i] == this.gameObject)
                {
                    SceneManager.listOfBullets.RemoveAt(i);
                    break;
                }
            }
            Destroy(gameObject);
        }

        // Check the collison with the target and the floor
        void OnCollisionEnter2D(Collision2D col)
        {
            for (int i = 0; i < SceneManager.listOfTargets.Count; i++)
            {
                if (col.gameObject == listOfTargets[i])
                {
                    Impact();
                }
            }

            for (int i = 0; i < SceneManager.listOfFloors.Count; i++)
            {
                if (col.gameObject == listOfFloors[i])
                {
                    Impact();
                }
            }
        }

        void RotateProjectile()
        {
            if (rotationRange > 0 && rotationSpeed > 0)
            {
                if (!rotateClockwise)
                {
                    transform.Rotate(new Vector3(0, 0, rotationSpeed * timeSinceLastFrame));

                    if (transform.rotation.z * Mathf.Rad2Deg >= rotationRange)
                    {
                        rotateClockwise = true;
                    }
                }
                else
                {
                    transform.Rotate(new Vector3(0, 0, -rotationSpeed * timeSinceLastFrame));

                    if (transform.rotation.z * Mathf.Rad2Deg <= -rotationRange)
                    {
                        rotateClockwise = false;
                    }
                }
            }
            //Debug.Break();
        }
        // Update is called once per frame
        void Update()
        {
            timeSinceLastFrame = Time.deltaTime / .001666f;
            Move();
            RotateProjectile();
            CheckIfOffScreen();

        }
    }
}