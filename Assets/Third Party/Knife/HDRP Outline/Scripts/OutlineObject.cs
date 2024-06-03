using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Rendering;

namespace Knife.HDRPOutline.Core
{
    [ExecuteAlways]
    [AddComponentMenu("Knife/Outline Object")]
    [RequireComponent(typeof(Renderer))]
    public class OutlineObject : MonoBehaviour
    {
        /// <summary>
        /// OnRender event calls from MegaOutline Render method
        /// </summary>
        internal static Action<CommandBuffer, Plane[]> onRender;

        [Tooltip("Outline material")]
        [SerializeField] private Material material;
        [Tooltip("Color of outline")]
        [SerializeField] private Color color = Color.white;
        [Tooltip("Alpha or Color mask for outline, BaseColor parameter must be setted to Alpha or Color")]
        [SerializeField] private Texture2D mask = null;
        [Tooltip("Scale of fresnel multiplier")]
        [SerializeField] private float fresnelScale = 2;
        [Tooltip("Power of fresnel multiplier")]
        [SerializeField] private float fresnelPower = 2;
        [Tooltip("Mask alpha threshold")]
        [SerializeField] [Range(0f, 1f)] private float alphaThreshold = 0.5f;

        private Renderer attachedRenderer;
        private Mesh attachedMesh;
        private MaterialPropertyBlock propertyBlock;

        /// <summary>
        /// Outline object material (recommended to use unlit materials)
        /// </summary>
        public Material Material
        {
            get
            {
                return material;
            }
            set
            {
                material = value;
            }
        }
        /// <summary>
        /// Per renderer color of outline object
        /// </summary>
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                PushParameters();
            }
        }
        /// <summary>
        /// Per renderer mask of outline object (material must have BaseColor parameter value Alpha or Color)
        /// </summary>
        public Texture2D Mask
        {
            get
            {
                return mask;
            }
            set
            {
                mask = value;
                PushParameters();
            }
        }
        /// <summary>
        /// Per renderer mask alpha threshold value (cutout)
        /// </summary>
        public float AlphaThreshold
        {
            get
            {
                return alphaThreshold;
            }
            set
            {
                alphaThreshold = value;
                PushParameters();
            }
        }
        /// <summary>
        /// Scale of fresnel multiplier  (material must have Fresnel parameter enabled)
        /// </summary>
        public float FresnelScale
        {
            get
            {
                return fresnelScale;
            }
            set
            {
                fresnelScale = value;
                PushParameters();
            }
        }
        /// <summary>
        /// Power of fresnel multiplier
        /// </summary>
        public float FresnelPower
        {
            get
            {
                return fresnelPower;
            }
            set
            {
                fresnelPower = value;
                PushParameters();
            }
        }

        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            attachedRenderer = GetComponent<Renderer>();
            var meshFilter = GetComponent<MeshFilter>();
            if(meshFilter != null)
            {
                attachedMesh = meshFilter.sharedMesh;
            }
            else
            {
                attachedMesh = attachedRenderer.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }
            // first unsubscribe to prevent double subscriptions
            onRender -= OnRender;
            onRender += OnRender;
            propertyBlock = new MaterialPropertyBlock();

            PushParameters();
        }

        /// <summary>
        /// Push parameters in MaterialPropertyBlock
        /// </summary>
        public void PushParameters()
        {
            onRender -= OnRender;
            onRender += OnRender;
            // setup per renderer properties (color, texture, threshold)
            try
            {
                attachedRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", Color);
                propertyBlock.SetFloat("_Threshold", AlphaThreshold);
                propertyBlock.SetFloat("_FresnelScale", FresnelScale);
                propertyBlock.SetFloat("_FresnelPower", FresnelPower);
                if (Mask == null)
                {
                    propertyBlock.SetTexture("_MainTex", Texture2D.whiteTexture);
                }
                else
                {
                    propertyBlock.SetTexture("_MainTex", Mask);
                }
                attachedRenderer.SetPropertyBlock(propertyBlock);
            } catch (NullReferenceException ex)
            {
                if(propertyBlock == null)
                {
                    Debug.LogError($"You must call Initialize or just enable object before modify properties\n{ex}", this);
                }
                else
                {
                    throw ex;
                }
            }
        }

        private void OnRender(CommandBuffer cmd, Plane[] frustumPlanes)
        {
            // check if object was destroyed, but object was not unsubscribe from event
            if(this == null)
            {
                onRender -= OnRender;
                return;
            }
            // Cull if invisible by camera
            if (GeometryUtility.TestPlanesAABB(frustumPlanes, attachedRenderer.bounds))
            {
                // we don't need update MaterialPropertyBlock in every frame
                // we can update it only when we change properties
                // PushParameters(); 

                // draw to current command buffer
                try
                {
                    for (int i = 0; i < attachedMesh.subMeshCount; i++)
                    {
                        cmd.DrawRenderer(attachedRenderer, Material, i, 0);
                    }
                    float depthTesting = Material.GetFloat("_ZTest");
                    int depthTestingIndex = (int)depthTesting;
                    CompareFunction compareFunction = (CompareFunction)depthTestingIndex;
                    if (compareFunction == CompareFunction.Greater || compareFunction == CompareFunction.GreaterEqual)
                    {
                        for (int i = 0; i < attachedMesh.subMeshCount; i++)
                        {
                            cmd.DrawRenderer(attachedRenderer, Material, i, 1);
                        }
                    }
                } catch (ArgumentNullException ex)
                {
                    Debug.LogError($"Did you assign material?\n{ex}", this);
                }
            }
        }

        private void OnDisable()
        {
            onRender -= OnRender;
        }

        private void OnDestroy()
        {
            onRender -= OnRender;
        }
    }
}