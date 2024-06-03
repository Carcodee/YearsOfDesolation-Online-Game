using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Knife.HDRPOutline.Core
{
    [Serializable, VolumeComponentMenu("Knife/HDRP Outline")]
    public class HDRPOutline : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        /// <summary>
        /// Outline mode
        /// </summary>
        [Tooltip("Outline mode")]
        public OutlineModeParameter mode = new OutlineModeParameter(OutlineMode.Hard, false);
        /// <summary>
        /// Width of outline
        /// </summary>
        [Tooltip("Width of outline")]
        public FloatParameter width = new FloatParameter(3);
        /// <summary>
        /// Fill amount of outlined objects
        /// </summary>
        [Tooltip("Fill amount of outlined objects")]
        public ClampedFloatParameter fillAmount = new ClampedFloatParameter(0.2f, 0, 1);

        /// <summary>
        /// Pattern texture of outline
        /// </summary>
        [Tooltip("Pattern texture of outline")]
        public TextureParameter patternTexture = new TextureParameter(null);
        /// <summary>
        /// Fill amount of outline pattern
        /// </summary>
        [Tooltip("Fill amount of outline pattern")]
        public ClampedFloatParameter patternFillAmount = new ClampedFloatParameter(0.2f, 0, 1);
        /// <summary>
        /// Pattern tile
        /// </summary>
        [Tooltip("Pattern tile")]
        public Vector2Parameter patternTile = new Vector2Parameter(new Vector2(1, 1));

        /// <summary>
        /// Outline iterations count
        /// </summary>
        [Tooltip("Outline iterations count")]
        public OutlineIterationsParameter iterations = new OutlineIterationsParameter(OutlineIterations.X8, false);

        /// <summary>
        /// Soft outline blur radius
        /// </summary>
        [Tooltip("Blur radius")]
        public FloatParameter blurRadius = new FloatParameter(1f);
        /// <summary>
        /// Soft outline blur iterations count
        /// </summary>
        [Tooltip("Blur iterations count")]
        public IntParameter blurIterations = new IntParameter(1);
        /// <summary>
        /// Soft outline overglow value
        /// </summary>
        [Tooltip("Overglow of outline after calculation")]
        public FloatParameter overglow = new FloatParameter(1);
        /// <summary>
        /// Apply softness for alpha before outline calculating
        /// </summary>
        [Tooltip("Softness of outline after calculation")]
        public BoolParameter softnessEnabled = new BoolParameter(false);
        /// <summary>
        /// Soft outline softness value
        /// </summary>
        [Tooltip("Softness of outline after calculation")]
        public ClampedFloatParameter softness = new ClampedFloatParameter(0.5f, 0.001f, 1f);
        public BoolParameter singlePassInstanced = new BoolParameter(false);

        private Material depthCopyMaterial;
        private Material blurMaterial;
        private Material maskApplierMaterial;
        private Material outlinePostProcessMaterial;

        private bool currentCameraIsSinglePassInstanced = false;

        private static class ShaderIDs
        {
            public static readonly int outlineTargetId = Shader.PropertyToID("_OutlineTarget");
            public static readonly int outlineTargetIdCopy = Shader.PropertyToID("_OutlineTargetCopy");
            public static readonly int gequalMaskId = Shader.PropertyToID("_GEqualMaskTarget");
            public static readonly int blurredOutlineTarget1Id = Shader.PropertyToID("_BlurredOutlineTarget");
            public static readonly int blurredOutlineTarget2Id = Shader.PropertyToID("_BlurredOutlineTarget2");
            public static readonly int blurRadiusId = Shader.PropertyToID("_Radius");
            public static readonly int overglowId = Shader.PropertyToID("_Overglow");
            public static readonly int softnessId = Shader.PropertyToID("_Softness");
            public static readonly int inputTextureId = Shader.PropertyToID("_InputTexture");
            public static readonly int widthId = Shader.PropertyToID("_Width");
            public static readonly int fillAmountId = Shader.PropertyToID("_FillAmount");
            public static readonly int patternId = Shader.PropertyToID("_Pattern");
            public static readonly int patternTileId = Shader.PropertyToID("_PatternTile");
            public static readonly int patternFillAmountId = Shader.PropertyToID("_PatternFillAmount");

            public static readonly int blitTexture = Shader.PropertyToID("_BlitTexture");
            public static readonly int blitScaleBias = Shader.PropertyToID("_BlitScaleBias");
        }

        public bool IsActive()
        {
            return mode.value != OutlineMode.Disabled;
        }

        public override void Setup()
        {
            // create materials with shaders from resources folder

            // material for depth copy
            depthCopyMaterial = new Material(Resources.Load<Shader>("Shaders/Knife-HDRPOutline DepthCopy"));
            // material for blur
            blurMaterial = new Material(Resources.Load<Shader>("Shaders/Knife-Blur"));
            maskApplierMaterial = new Material(Resources.Load<Shader>("Shaders/Knife-MaskApplier"));
            // material for calculate and blend outline to color buffer
            outlinePostProcessMaterial = new Material(Resources.Load<Shader>("Shaders/Knife-HDRPOutline"));
            outlinePostProcessMaterial.enableInstancing = true;
            depthCopyMaterial.enableInstancing = true;
            depthCopyMaterial.EnableKeyword("STEREO_INSTANCING_ON");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            currentCameraIsSinglePassInstanced = Application.isPlaying && camera.camera.cameraType != UnityEngine.CameraType.SceneView && singlePassInstanced.value;
            // if outline mode is Disabled just blit source to destination
            if (mode.value == OutlineMode.Disabled)
            {
                HDUtils.BlitCameraTexture(cmd, source, destination);
                return;
            }

            CreateBuffers(cmd, camera); // create outline color and depth buffer
            CopyDepth(cmd); // copy current depth to outline depth buffer
            //HDUtils.DrawFullScreen(cmd, HDUtils.GetBlitMaterial(destination.rt.dimension), destination);
            //return;
            SetRenderTarget(cmd); // assign outline buffers to render targets

            // call static Action to render all subscribed objects
            if (OutlineObject.onRender != null)
                OutlineObject.onRender(cmd, GeometryUtility.CalculateFrustumPlanes(camera.camera));

            cmd.SetGlobalTexture(ShaderIDs.gequalMaskId, ShaderIDs.gequalMaskId);
            cmd.SetGlobalTexture(ShaderIDs.blitTexture, ShaderIDs.outlineTargetId);
            cmd.SetGlobalVector(ShaderIDs.blitScaleBias, new Vector4(1, 1, 0, 0));
            cmd.Blit(ShaderIDs.outlineTargetId, ShaderIDs.outlineTargetIdCopy, HDUtils.GetBlitMaterial(TextureDimension.Tex2DArray), 0, -1);
            cmd.Blit(ShaderIDs.outlineTargetIdCopy, ShaderIDs.outlineTargetId, maskApplierMaterial, 0, -1);

            // blit outline buffers to color buffer
            BlitOutlineToColorBuffer(cmd, source, destination);
        }

        private void CreateBuffers(CommandBuffer cmd, HDCamera camera)
        {
            RenderTextureDescriptor renderTargetDescriptor = new RenderTextureDescriptor();
            renderTargetDescriptor.width = camera.actualWidth;
            renderTargetDescriptor.height = camera.actualHeight;
            renderTargetDescriptor.depthBufferBits = 32;
            renderTargetDescriptor.colorFormat = RenderTextureFormat.ARGBFloat;
            renderTargetDescriptor.msaaSamples = 1;
            renderTargetDescriptor.dimension = TextureDimension.Tex2DArray;

            if (currentCameraIsSinglePassInstanced)
            {
                renderTargetDescriptor.volumeDepth = 2;
                renderTargetDescriptor.vrUsage = VRTextureUsage.TwoEyes;
            }
            else
            {
                renderTargetDescriptor.volumeDepth = 1;
                renderTargetDescriptor.vrUsage = VRTextureUsage.None;
            }

            // this texture contains 2 buffers - color and depth, because 32 bits for depth provided
            //cmd.GetTemporaryRT(ShaderIDs.outlineTargetId, camera.actualWidth, camera.actualHeight, 32, FilterMode.Point, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1, false);
            cmd.GetTemporaryRT(ShaderIDs.outlineTargetId, renderTargetDescriptor);
            cmd.GetTemporaryRT(ShaderIDs.gequalMaskId, renderTargetDescriptor);
            cmd.GetTemporaryRT(ShaderIDs.outlineTargetIdCopy, renderTargetDescriptor);
            //cmd.GetTemporaryRT(ShaderIDs.gequalMaskId, camera.actualWidth, camera.actualHeight, 32, FilterMode.Point, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1, false);

            // if mode is Soft we need create 2 blur RenderTextures
            switch (mode.value)
            {
                case OutlineMode.Soft:
                    RenderTextureDescriptor blurTexDescriptor = renderTargetDescriptor;
                    blurTexDescriptor.depthBufferBits = 0;
                    //cmd.GetTemporaryRT(ShaderIDs.blurredOutlineTarget1Id, camera.actualWidth, camera.actualHeight, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1, false);
                    //cmd.GetTemporaryRT(ShaderIDs.blurredOutlineTarget2Id, camera.actualWidth, camera.actualHeight, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1, false);
                    cmd.GetTemporaryRT(ShaderIDs.blurredOutlineTarget1Id, blurTexDescriptor);
                    cmd.GetTemporaryRT(ShaderIDs.blurredOutlineTarget2Id, blurTexDescriptor);
                    break;
            }
        }

        // Copy screen depth to outline depth
        // also in depthCopyMaterial outline color buffer is clearing
        // Output o;
        // o.color = 0;
        // o.depth = screenDepth;
        // return o;
        private void CopyDepth(CommandBuffer cmd)
        {
            cmd.Blit(0, ShaderIDs.outlineTargetId, depthCopyMaterial, 0, -1);
            //HDUtils.DrawFullScreen(cmd, new Rect(0, 0, 1, 1), depthCopyMaterial, new RenderTargetIdentifier(ShaderIDs.outlineTargetId));
        }
        
        private void SetRenderTarget(CommandBuffer cmd)
        {
            if(currentCameraIsSinglePassInstanced)
                cmd.SetRenderTarget(new RenderTargetIdentifier[2] { ShaderIDs.outlineTargetId, ShaderIDs.gequalMaskId }, ShaderIDs.outlineTargetId, 0, CubemapFace.Unknown, -1);
            else
                cmd.SetRenderTarget(new RenderTargetIdentifier[2] { ShaderIDs.outlineTargetId, ShaderIDs.gequalMaskId }, ShaderIDs.outlineTargetId);
            //cmd.SetRenderTarget(ShaderIDs.outlineTargetId, ShaderIDs.outlineTargetId, 0, CubemapFace.Unknown, -1);
            cmd.ClearRenderTarget(false, true, Color.clear);
        }

        private void BlitOutlineToColorBuffer(CommandBuffer cmd, RTHandle source, RTHandle destination)
        {
            // there we push some properties to materials
            // and after that we calculate and blend outline with HDUtils.DrawFullScreen method
            outlinePostProcessMaterial.SetTexture(ShaderIDs.patternId, patternTexture.value);
            outlinePostProcessMaterial.SetVector(ShaderIDs.patternTileId, patternTile.value);
            outlinePostProcessMaterial.SetFloat(ShaderIDs.patternFillAmountId, patternFillAmount.value);
            switch (mode.value)
            {
                case OutlineMode.Hard:
                    cmd.SetGlobalTexture(ShaderIDs.outlineTargetId, ShaderIDs.outlineTargetId);
                    EnableIterationsKeywords(outlinePostProcessMaterial, iterations.value);

                    // calculate and blend outline
                    outlinePostProcessMaterial.SetFloat(ShaderIDs.widthId, width.value);
                    outlinePostProcessMaterial.SetFloat(ShaderIDs.fillAmountId, fillAmount.value);
                    outlinePostProcessMaterial.SetTexture(ShaderIDs.inputTextureId, source);
                    HDUtils.DrawFullScreen(cmd, outlinePostProcessMaterial, destination, null, 0);
                    break;
                case OutlineMode.Soft:

                    // blur outlineTargetId
                    // do: blurredOutlineTarget - outlineTargetId
                    // alpha blend to color buffer

                    // blur start
                    if (!Mathf.Approximately(blurRadius.value, 0))
                    {
                        blurMaterial.SetFloat(ShaderIDs.blurRadiusId, blurRadius.value);
                        cmd.Blit(ShaderIDs.outlineTargetId, ShaderIDs.blurredOutlineTarget1Id, blurMaterial, 0, -1);
                        for (int i = 0; i < blurIterations.value; i++)
                        {
                            cmd.Blit(ShaderIDs.blurredOutlineTarget1Id, ShaderIDs.blurredOutlineTarget2Id, blurMaterial, 0, -1);
                            cmd.Blit(ShaderIDs.blurredOutlineTarget2Id, ShaderIDs.blurredOutlineTarget1Id, blurMaterial, 0, -1);
                        }
                    } else
                    {
                        cmd.Blit(ShaderIDs.outlineTargetId, ShaderIDs.blurredOutlineTarget1Id, -1, -1);
                    }
                    // blur end

                    // calculate and blend outline
                    EnableSoftnessKeyword(outlinePostProcessMaterial, softnessEnabled.value);
                    cmd.SetGlobalTexture(ShaderIDs.outlineTargetId, ShaderIDs.outlineTargetId);
                    cmd.SetGlobalTexture(ShaderIDs.blurredOutlineTarget1Id, ShaderIDs.blurredOutlineTarget1Id);
                    outlinePostProcessMaterial.SetFloat(ShaderIDs.overglowId, overglow.value);
                    outlinePostProcessMaterial.SetFloat(ShaderIDs.softnessId, softness.value);
                    outlinePostProcessMaterial.SetFloat(ShaderIDs.fillAmountId, fillAmount.value);
                    outlinePostProcessMaterial.SetTexture(ShaderIDs.inputTextureId, source);
                    HDUtils.DrawFullScreen(cmd, outlinePostProcessMaterial, destination, null, 1);

                    break;
            }
        }

        private void EnableIterationsKeywords(Material material, OutlineIterations iterations)
        {
            // Hard outline iterations count keywords
            // if all keywords disabled by default 4 iterations will be used
            material.DisableKeyword("ITERATIONS_8");
            material.DisableKeyword("ITERATIONS_16");
            material.DisableKeyword("ITERATIONS_32");
            switch (iterations)
            {
                case OutlineIterations.X4:
                    break;
                case OutlineIterations.X8:
                    material.EnableKeyword("ITERATIONS_8");
                    break;
                case OutlineIterations.X16:
                    material.EnableKeyword("ITERATIONS_16");
                    break;
                case OutlineIterations.X32:
                    material.EnableKeyword("ITERATIONS_32");
                    break;
            }
        }

        private void EnableSoftnessKeyword(Material material, bool enabled)
        {
            if (enabled)
                material.EnableKeyword("SOFTNESS");
            else
                material.DisableKeyword("SOFTNESS");
        }

        public override void Cleanup()
        {
            // destroy materials
            CoreUtils.Destroy(outlinePostProcessMaterial);
            CoreUtils.Destroy(depthCopyMaterial);
            CoreUtils.Destroy(blurMaterial);
        }

        public override CustomPostProcessInjectionPoint injectionPoint
        {
            get
            {
                return CustomPostProcessInjectionPoint.AfterPostProcess;
            }
        }
    }
}