using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we store helper methods for finding bones
    /// </summary>
    public partial class FLookAnimator
    {
        /// <summary>
        /// Defining object's transform as base
        /// </summary>
        public void FindBaseTransform()
        {
            BaseTransform = transform;

            if (!GetComponentInChildren<Animator>())
            {
                if (!GetComponentInChildren<Animation>())
                    Debug.LogWarning(gameObject.name + " don't have animator. '" + name + "' is it root transform for your character?");
            }
        }



        /// <summary>
        /// Trying to find other's transform's head position using different methods
        /// </summary>
        public Vector3 TryFindHeadPositionInTarget(Transform other)
        {
            // First, let's see if other object have look animator attached to it
            FLookAnimator look = other.GetComponent<FLookAnimator>();

            if (look)
            {
                if (look.LeadBone) return look.GetLookStartMeasurePosition();
            }

            // Let's see if other object is using humanoid animator
            Animator animator = other.GetComponentInChildren<Animator>();
            if (animator)
            {
                if (animator.isHuman)
                {
                    if (animator.GetBoneTransform(HumanBodyBones.LeftEye)) return animator.GetBoneTransform(HumanBodyBones.LeftEye).position;
                    else
                    if (animator.GetBoneTransform(HumanBodyBones.Head)) return animator.GetBoneTransform(HumanBodyBones.Head).position;
                }
            }

            // Trying to predict head position using other object's bounds
            Renderer rend = other.GetComponentInChildren<Renderer>();
            if (!rend) if (other.childCount > 0) rend = other.GetChild(0).GetComponentInChildren<Renderer>();

            if (rend)
            {
                Vector3 boundsHead = other.position;
                boundsHead += other.TransformVector(Vector3.up * (rend.bounds.max.y * 0.9f));
                boundsHead += other.TransformVector(Vector3.forward * (rend.bounds.max.z * 0.75f));
                return boundsHead;
            }

            return other.position;
        }

    }
}