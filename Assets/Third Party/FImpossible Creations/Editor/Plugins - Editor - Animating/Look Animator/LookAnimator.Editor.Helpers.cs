using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FLook
{
    public partial class FLookAnimator_Editor
    {
        // RESOURCES ----------------------------------------

        public static Texture2D _TexLookAnimIcon { get { if (__texLookAnimIcon != null) return __texLookAnimIcon; __texLookAnimIcon = Resources.Load<Texture2D>("Look Animator/LookAnimator_SmallIcon"); return __texLookAnimIcon; } }
        private static Texture2D __texLookAnimIcon = null;
        public static Texture2D _TexBackbonesIcon { get { if (__texBacBonIcon != null) return __texBacBonIcon; __texBacBonIcon = Resources.Load<Texture2D>("Look Animator/BackBones"); return __texBacBonIcon; } }
        private static Texture2D __texBacBonIcon = null;
        public static Texture2D _TexCompensationIcon { get { if (__texCompensIcon != null) return __texCompensIcon; __texCompensIcon = Resources.Load<Texture2D>("Look Animator/Compensation"); return __texCompensIcon; } }
        private static Texture2D __texCompensIcon = null;
        public static Texture2D _TexBirdIcon { get { if (__texBirdIcon != null) return __texBirdIcon; __texBirdIcon = Resources.Load<Texture2D>("Look Animator/BirdMode"); return __texBirdIcon; } }
        private static Texture2D __texBirdIcon = null;
        public static Texture2D _TexEyesIcon { get { if (__texEyeIcon != null) return __texEyeIcon; __texEyeIcon = Resources.Load<Texture2D>("Look Animator/LookEyes"); return __texEyeIcon; } }
        private static Texture2D __texEyeIcon = null;


        private static UnityEngine.Object _manualFile;


        // HELPER VARIABLES ----------------------------------------
        private FLookAnimator Get { get { if (_get == null) _get = target as FLookAnimator; return _get; } }
        private FLookAnimator _get;

        static bool drawDefaultInspector = false;
        private Color unchangedC = new Color(1f, 1f, 1f, 0.65f);
        private Color limitsC = new Color(1f, 1f, 1f, 0.88f);
        private Color c;
        private Color bc;

        List<SkinnedMeshRenderer> skins;
        SkinnedMeshRenderer largestSkin;
        Animator animator;
        Animation animation;
        Behaviour componentAnimator;


        /// <summary>
        /// Searching through component's owner to find clavicle / shoulder and upperarm bones
        /// </summary>
        private void FindCompensationBones(FLookAnimator headLook)
        {
            // First let's check if it's humanoid character, then we can get head bone transform from it
            Transform root = headLook.transform;
            if (headLook.BaseTransform) root = headLook.BaseTransform;

            Animator animator = root.GetComponentInChildren<Animator>();

            List<Transform> compensationBones = new List<Transform>();

            Transform headBone = headLook.LeadBone;

            if (animator)
            {
                if (animator.isHuman)
                {
                    Transform b = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                    if (b) compensationBones.Add(b);

                    b = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
                    if (b) compensationBones.Add(b);

                    b = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                    if (b) compensationBones.Add(b);

                    b = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                    if (b) compensationBones.Add(b);

                    if (!headBone) animator.GetBoneTransform(HumanBodyBones.Head);
                }
                else
                {
                    if (animator)
                    {
                        foreach (Transform t in animator.transform.GetComponentsInChildren<Transform>())
                        {
                            if (t.name.ToLower().Contains("clav"))
                            {
                                if (!compensationBones.Contains(t)) compensationBones.Add(t);
                            }
                            else
                            if (t.name.ToLower().Contains("shoulder"))
                            {
                                if (!compensationBones.Contains(t)) compensationBones.Add(t);
                            }
                            else
                            if (t.name.ToLower().Contains("uppera"))
                            {
                                if (!compensationBones.Contains(t)) compensationBones.Add(t);
                            }
                        }
                    }
                }
            }

            if (compensationBones.Count != 0)
            {
                for (int i = 0; i < compensationBones.Count; i++)
                {
                    // Checking if this bone is not already in compensation bones list
                    bool already = false;
                    for (int c = 0; c < headLook.CompensationBones.Count; c++)
                    {
                        if (compensationBones[i] == headLook.CompensationBones[c].Transform)
                        {
                            already = true;
                            break;
                        }
                    }

                    if (already) continue;

                    // Fill nulls if available
                    bool filled = false;
                    for (int c = 0; c < headLook.CompensationBones.Count; c++)
                    {
                        if (headLook.CompensationBones[c].Transform == null)
                        {
                            headLook.CompensationBones[c] = new FLookAnimator.CompensationBone(compensationBones[i]);
                            filled = true;
                            serializedObject.Update();
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }
                    }

                    if (!filled)
                        headLook.CompensationBones.Add(new FLookAnimator.CompensationBone(compensationBones[i]));
                }

                for (int c = headLook.CompensationBones.Count - 1; c >= 0; c--)
                {
                    if (headLook.CompensationBones[c].Transform == null) headLook.CompensationBones.RemoveAt(c);
                }

                compensationBonesCount = headLook.CompensationBones.Count;
            }
        }


        void CheckForComponents()
        {
            if (skins == null) skins = new List<SkinnedMeshRenderer>();
            if (Get.BaseTransform == null) Get.BaseTransform = Get.transform;

            foreach (var t in Get.BaseTransform.GetComponentsInChildren<Transform>())
            {
                SkinnedMeshRenderer s = t.GetComponent<SkinnedMeshRenderer>(); if (s) skins.Add(s);
                if (!animator) animator = t.GetComponent<Animator>();
                if (!animator) if (!animation) animation = t.GetComponent<Animation>();
            }

            if ((skins != null && largestSkin != null) && (animator != null || animation != null)) return;

            if (Get.BaseTransform != Get.transform)
            {
                foreach (var t in Get.transform.GetComponentsInChildren<Transform>())
                {
                    SkinnedMeshRenderer s = t.GetComponent<SkinnedMeshRenderer>(); if (!skins.Contains(s)) if (s) skins.Add(s);
                    if (!animator) animator = t.GetComponent<Animator>();
                    if (!animator) if (!animation) animation = t.GetComponent<Animation>();
                }
            }

            if (skins.Count > 1)
            {
                largestSkin = skins[0];
                for (int i = 1; i < skins.Count; i++)
                    if (skins[i].bones.Length > largestSkin.bones.Length)
                        largestSkin = skins[i];
            }
            else
                if (skins.Count > 0) largestSkin = skins[0];

        }


        /// <summary>
        /// Updating component from custom inspector helper method
        /// </summary>
        private bool UpdateCustomInspector(FLookAnimator Get, bool allowInPlaymode = true)
        {
            if (!allowInPlaymode)
            {
                if (Application.isPlaying) return false;
            }
            else
                Get.UpdateForCustomInspector();

            return true;
        }


        /// <summary>
        /// Searching through component's owner to find head or neck bone
        /// </summary>
        private void FindHeadBone(FLookAnimator headLook)
        {
            // First let's check if it's humanoid character, then we can get head bone transform from it
            Transform root = headLook.transform;
            if (headLook.BaseTransform) root = headLook.BaseTransform;

            Animator animator = root.GetComponentInChildren<Animator>();
            Transform animatorHeadBone = null;
            if (animator)
            {
                if (animator.isHuman)
                    animatorHeadBone = animator.GetBoneTransform(HumanBodyBones.Head);
            }

            List<SkinnedMeshRenderer> sMeshs = new List<SkinnedMeshRenderer>();// = root.GetComponentInChildren<SkinnedMeshRenderer>();

            foreach (var tr in root.GetComponentsInChildren<Transform>())
            {
                if (tr == null) continue;
                SkinnedMeshRenderer sMesh = tr.GetComponent<SkinnedMeshRenderer>();
                if (sMesh) sMeshs.Add(sMesh);
            }

            Transform leadBone = null;
            Transform probablyWrongTransform = null;

            for (int s = 0; s < sMeshs.Count; s++)
            {
                Transform t;

                for (int i = 0; i < sMeshs[s].bones.Length; i++)
                {
                    t = sMeshs[s].bones[i];
                    if (t.name.ToLower().Contains("head"))
                    {
                        if (t.parent == root) continue; // If it's just mesh object from first depths

                        leadBone = t;
                        break;
                    }
                }

                if (!leadBone)
                    for (int i = 0; i < sMeshs[s].bones.Length; i++)
                    {
                        t = sMeshs[s].bones[i];
                        if (t.name.ToLower().Contains("neck"))
                        {
                            leadBone = t;
                            break;
                        }
                    }
            }


            foreach (Transform t in root.GetComponentsInChildren<Transform>())
            {
                if (t.name.ToLower().Contains("head"))
                {
                    if (t.GetComponent<SkinnedMeshRenderer>())
                    {
                        if (t.parent == root) continue; // If it's just mesh object from first depths
                        probablyWrongTransform = t;
                        continue;
                    }

                    leadBone = t;
                    break;
                }
            }

            if (!leadBone)
                foreach (Transform t in root.GetComponentsInChildren<Transform>())
                {
                    if (t.name.ToLower().Contains("neck"))
                    {
                        leadBone = t;
                        break;
                    }
                }

            if (leadBone == null && animatorHeadBone != null)
                leadBone = animatorHeadBone;
            else
            if (leadBone != null && animatorHeadBone != null)
            {
                if (animatorHeadBone.name.ToLower().Contains("head")) leadBone = animatorHeadBone;
                else
                    if (!leadBone.name.ToLower().Contains("head")) leadBone = animatorHeadBone;
            }

            if (leadBone)
            {
                headLook.LeadBone = leadBone;
            }
            else
            {
                if (probablyWrongTransform)
                {
                    Get.LeadBone = probablyWrongTransform;
                    Debug.LogWarning("[LOOK ANIMATOR] Found " + probablyWrongTransform + " but it's probably wrong transform");
                }
                else
                {
                    Debug.LogWarning("[LOOK ANIMATOR] Couldn't find any fitting bone");
                }
            }
        }


        public Vector3 RoundVector(Vector3 v)
        {
            return new Vector3((float)Math.Round(v.x, 1), (float)Math.Round(v.y, 1), (float)Math.Round(v.z, 1));
        }

    }
}