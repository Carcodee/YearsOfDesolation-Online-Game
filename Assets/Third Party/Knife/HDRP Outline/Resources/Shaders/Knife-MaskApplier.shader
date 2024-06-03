Shader "Hidden/Knife/Knife-MaskApplier"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D_X(_MainTex);
    TEXTURE2D_X(_GEqualMaskTarget);

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

    float4 Fragment(Varyings input) : SV_TARGET
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // screen space pixel position
        uint2 positionSS = input.texcoord * _ScreenSize.xy;
		
		float4 color = LOAD_TEXTURE2D_X(_MainTex, positionSS);
		float4 mask = LOAD_TEXTURE2D_X(_GEqualMaskTarget, positionSS);

		color *= 1 - mask.a;
        
        return color;
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