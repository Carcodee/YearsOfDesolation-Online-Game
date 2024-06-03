Shader "Hidden/Knife/Knife-HDRPOutline DepthCopy"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vertex(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // multi target output structure
    // SV_TARGET - color buffer
    // SV_DEPTH - depth buffer
    struct Output
    {
        float4 color : SV_TARGET;
        float depth : SV_DEPTH;
    };

    Output Fragment(Varyings input)
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // screen space pixel position
        input.texcoord.y = 1 - input.texcoord.y;

        uint2 positionSS = input.texcoord * _ScreenSize.xy;

        // read depth from camera read buffer
        float depth = LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS).r;
        
        // create outpute structure and set color clear and write same depth value
        // we must set other RenderTarget with CommandBuffer.SetRenderTarget
        // this RenderTexture must have depth buffer size more than 0 (16, 24 or 32)
        Output o;
        o.color = 0;
        o.depth = depth;

        return o;
    }

    ENDHLSL

    SubShader
    {
        // ZWrite On is required because we need write to depth and do it manually
        Cull Off ZWrite On ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #pragma multi_compile __ STEREO_INSTANCING_ON
            ENDHLSL
        }
    }

    Fallback Off
}
