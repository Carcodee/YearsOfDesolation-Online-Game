using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKPn.Example.Example2D
{
    public class IK2DUpdater : MonoBehaviour
    {
        private SpriteExample[] sprites;
        private IKP ikp;

        void Start()
        {
            sprites = GetComponentsInChildren<SpriteExample>();
            ikp = GetComponent<IKP>();
            ikp.manuallyUpdated = true;
        }

        void Update()
        {
            ikp.IKPPreUpdate();
        }

        void LateUpdate()
        {
            ikp.IKPUpdate();
            foreach (SpriteExample sprite in sprites)
            {
                sprite.UpdateState();
            }
        }
    }
}