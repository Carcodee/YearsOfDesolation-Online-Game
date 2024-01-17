using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Demo_Project.SceneManager;

namespace Demo_Project
{
    public class Target : MonoBehaviour
    {
        public GameObject hitObj = null;
        public float increaseSizeRate = .01f;
        public float targetRandomOffset = .5f;
        public float targetShrinkSize = .75f;
        // Start is called before the first frame update
        void Start()
        {
            SceneManager.listOfTargets.Add(this.gameObject);
        }

        // Increase the target back to its original size
        void ReturnToNormalSize()
        {
            if (transform.localScale.x < 1)
            {
                Vector3 localScale = transform.localScale;
                localScale.x += increaseSizeRate;
                localScale.y += increaseSizeRate;
                localScale.z += increaseSizeRate;
                transform.localScale = localScale;
            }
        }
        // Displays the hit pop-up
        void GenerateHitObj()
        {
            float randomX = Random.Range(-targetRandomOffset, targetRandomOffset);
            float randomY = Random.Range(-targetRandomOffset, targetRandomOffset);

            GameObject tempHitTarget = Instantiate(hitObj, new Vector3(transform.position.x + randomX, transform.position.y + randomY, transform.position.z), Quaternion.Euler(0, 0, 0));
            SceneManager.listOfImpacts.Add(tempHitTarget);

        }

        // Shrinks the target size
        void ShrinkTarget()
        {
            transform.localScale = new Vector3(targetShrinkSize, targetShrinkSize, targetShrinkSize);
        }

        // Determine the collision
        void OnCollisionEnter2D(Collision2D col)
        {
            ShrinkTarget();
            GenerateHitObj();
        }


        // Update is called once per frame
        void Update()
        {
            ReturnToNormalSize();
        }
    }
}