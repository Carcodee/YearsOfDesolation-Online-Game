using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement feature for "Moment Targets" easily customizable methods to be used by game developers 
    /// for designing custom behaviour for targeting different objects in game world
    /// </summary>
    public partial class FLookAnimator
    {
        /// <summary> Generated helper transform to look at other than main focus object </summary>
        public Transform MomentLookTransform { get; private set; }

        private GameObject generatedMomentTarget;

        /// <summary> Variable to detect when moment look target disappears</summary>
        private bool wasMomentLookTransform = false;



        /// <summary>
        /// Creating custom head follow target object (position or pinned to transform) to look at
        /// </summary>
        /// <param name="parent"> Transform to which should be attached moment look target, you can leave it null if don't want attach to anything </param>
        /// <param name="position"> Target position to be followed, you can leave it null if just want to follow parent transform </param>
        /// <param name="destroyTimer"> Time in seconds, how long head should look at this moment target then return to main target, you can leave it null if you don't want to destroy this object - then you have to call ForceDestroyMomentTarget() to go back looking at main target </param>
        /// <param name="worldPosition"> If "position" variable is defining world position, set it to true </param>
        public GameObject SetMomentLookTarget(Transform parent = null, Vector3? position = null, float? destroyTimer = 3f, bool worldPosition = false)
        {
            Transform pre = MomentLookTransform;
            if (MomentLookTransform) pre = MomentLookTransform.parent;

            GameObject target;

            if (destroyTimer == null)
            {
                if (!generatedMomentTarget)
                    generatedMomentTarget = new GameObject(transform.gameObject.name + "-MomentLookTarget " + Time.frameCount);
                else
                    generatedMomentTarget.name = transform.gameObject.name + "-MomentLookTarget " + Time.frameCount;

                target = generatedMomentTarget;
            }
            else
            {
                target = new GameObject(transform.gameObject.name + "-MomentLookTarget " + Time.frameCount);
            }

            // Handling different states of provided variables
            if (parent != null)
            {
                // Creating follower at position and then attaching to target transform
                target.transform.SetParent(parent);

                if (position != null)
                {
                    if (worldPosition)
                        target.transform.position = (Vector3)position;
                    else
                        target.transform.localPosition = (Vector3)position;
                }
                else
                    target.transform.localPosition = Vector3.zero;
            }
            else // If parent isn't assigned
            {
                if (position != null)
                    target.transform.position = (Vector3)position;
            }

            MomentLookTransform = target.transform;
            wasMomentLookTransform = true;

            //if (parent != null && pre != parent)
            //    if (pre != target.transform) 
            TargetChangedMeasures();
            //SmoothChangeTarget(0.1f, 0.1f, true);

            if (destroyTimer != null)
            {
                // Destroying generated object after 
                Destroy(target, (float)destroyTimer);
            }

            return target;
        }


        /// <summary>
        /// Assigning moment look transform with custom transform
        /// </summary>
        public void SetMomentLookTransform(Transform transform, float timeToLeft = 0f)
        {
            MomentLookTransform = transform;
            wasMomentLookTransform = true;
            TargetChangedMeasures();

            //SmoothChangeTarget(0.1f, 0.1f, true);

            if (timeToLeft > 0f)
            {
                StartCoroutine(CResetMomentLookTransform(null, timeToLeft));
            }
        }


        /// <summary>
        /// Destroying moment look transform manually
        /// </summary>
        public void ForceDestroyMomentTarget()
        {
            if (generatedMomentTarget)
            {
                Destroy(generatedMomentTarget);
            }
            else
                if (MomentLookTransform) MomentLookTransform = null;
        }



    }
}