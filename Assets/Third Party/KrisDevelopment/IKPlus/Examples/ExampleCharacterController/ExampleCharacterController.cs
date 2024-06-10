using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKPn.Example
{
    [RequireComponent(typeof(CharacterController))]
    public class ExampleCharacterController : MonoBehaviour
    {
        private CharacterController controller;

        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
            controller.Move((transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal") - Vector3.up) * 10 * Time.deltaTime);
        }
    }
}
