using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RendererUtils;

class CustomUIPass : CustomPass
{

    public LayerMask uiLayer;
    public Material bloomMaterial;
    private RTHandle temporaryRenderTarget;
    
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        temporaryRenderTarget=RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.R16G16B16A16_SFloat, useDynamicScale: true, name: "SelectiveBloom"
        );
        // Setup code here
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (bloomMaterial == null) return;

        // Ensure we only affect the UI layer
        RendererListDesc result = new RendererListDesc
        {
            rendererConfiguration = PerObjectData.None,
            renderQueueRange = RenderQueueRange.all,
            sortingCriteria = SortingCriteria.CommonTransparent,
            layerMask = uiLayer,
            excludeObjectMotionVectors = false,
        };
        
        CoreUtils.SetRenderTarget( ctx.cmd , temporaryRenderTarget, ClearFlag.All, Color.clear);
        // Executed every frame for all the camera inside the pass volume.
        // Apply your custom bloom effect
        ctx.cmd.Blit(temporaryRenderTarget, ctx.cameraColorBuffer, bloomMaterial);

        // The context contains the command buffer to use to enqueue graphics commands.
        
    }

    protected override void Cleanup()
    {
        temporaryRenderTarget.Release();
        // Cleanup code
    }
}

