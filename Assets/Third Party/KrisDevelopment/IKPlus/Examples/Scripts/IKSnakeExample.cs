using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKPn.Example
{
    /// <summary>
    /// Snake IKP Example
    /// </summary>
    [RequireComponent(typeof(IKP))]
    public class IKSnakeExample : MonoBehaviour
    {
        private const float attackInterval = 5f;
        private const float attackSpeed = 5f;
        private float attackTimer = attackInterval;
        private bool attacking;

        private IKP ikp;
        [SerializeField] private Transform target;
        [SerializeField] private GameObject attackIndicator;


        void Start()
        {
            // Good to bind your IKP reference upon initialization. Avoid calling GetComponent in Update.
            ikp = GetComponent<IKP>();
            Debug.Assert(ikp != null);
            Debug.Assert(target != null);
            Debug.Assert(attackIndicator != null);

            attackIndicator.SetActive(false);
        }

        void Update()
        {
            // Simple timer for attack invervals
            if (!attacking)
            {
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
                else
                {
                    attackTimer = attackInterval;
                    StartCoroutine(Attack());
                }
            }
        }

        private IEnumerator Attack()
        {
            // It's good to check if you have a module before you begin.
            // IKP will handle it silently if you don't.
            // This is because of the IKP Blender system, where properties and modules aren't always available.
            if (!ikp.HasModule(ModuleSignatures.GENERIC_LIMBS))
            {
                Debug.LogError("Can't find IKP " + ModuleSignatures.GENERIC_LIMBS);
                yield break;
            }

            // tell that we are attacking so another coroutine doesn't start while we do this attack
            attacking = true;
            attackIndicator.SetActive(true);

            // Pick a target at the start of the attack, so it gives the player a chance to dodge
            Vector3 _targetPoint = target.position;
            ikp.SetGenericLimbTarget(_targetPoint);
            ikp.SetLookTarget(_targetPoint);
            attackIndicator.transform.position = _targetPoint;

            // Make the snake look at the attack spot for a second
            yield return new WaitForSeconds(1f);

            float _weight = ikp.GetProperty(IKPModule_GenericLimb.p_weight, ModuleSignatures.GENERIC_LIMBS);

            while (true)
            {
                // Smoothly interpolate the weight, this will make the attack look organic.
                _weight += Time.deltaTime * attackSpeed;
                ikp.SetProperty(IKPModule_GenericLimb.p_weight, Mathf.Min(_weight, 1), ModuleSignatures.GENERIC_LIMBS);
                if (_weight >= 1f)
                {
                    break;
                }

                yield return null;
            }

            attackIndicator.SetActive(false);

            // Make the snake stay with head at the attack spot for a while
            yield return new WaitForSeconds(1.5f);

            // transition back to idle
            while (true)
            {
                // Smoothly interpolate the weight, this will make the attack look organic.
                _weight -= Time.deltaTime;
                ikp.SetProperty(IKPModule_GenericLimb.p_weight, Mathf.Max(_weight, 0), ModuleSignatures.GENERIC_LIMBS);
                
                if (_weight <= 0)
                {
                    break;
                }

                yield return null;
            }

            // make the snake look at the target again
            ikp.SetLookTarget(target);

            attacking = false;
        }
    }
}