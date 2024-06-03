Shader "Hidden/Knife/Knife-Blur"
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
    float _Radius;
    //float4 _MainTex_TexelSize;

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

    float4 gaussianBlur(float2 dir, float2 uv, float res)
    {
        // this will be our RGBA sum
        float4 sum = float4(0, 0, 0, 0);
        
        // the amount to blur, i.e. how far off center to sample from 
        // 1.0 -> blur by one pixel
        // 2.0 -> blur by two pixels, etc.
        float blur = _Radius / res; 
        
        // the direction of our blur
        // (1.0, 0.0) -> x-axis blur
        // (0.0, 1.0) -> y-axis blur
        float hstep = dir.x;
        float vstep = dir.y;
        
        // apply blurring, using a 9-tap filter with predefined gaussian weights
        uint2 positionSS = uv * _ScreenSize.xy;

        uint2 positionSS1 = uint2(float2(uv.x - 4 * blur * hstep, uv.y - 4 * blur * vstep) * _ScreenSize.xy);
        uint2 positionSS2 = uint2(float2(uv.x - 3 * blur * hstep, uv.y - 3 * blur * vstep) * _ScreenSize.xy);
        uint2 positionSS3 = uint2(float2(uv.x - 2 * blur * hstep, uv.y - 2 * blur * vstep) * _ScreenSize.xy);
        uint2 positionSS4 = uint2(float2(uv.x - 1 * blur * hstep, uv.y - 1 * blur * vstep) * _ScreenSize.xy);
        uint2 positionSS5 = positionSS;
        uint2 positionSS6 = uint2(float2(uv.x + 1 * blur * hstep, uv.y + 1 * blur * vstep) * _ScreenSize.xy);
        uint2 positionSS7 = uint2(float2(uv.x + 2 * blur * hstep, uv.y + 2 * blur * vstep) * _ScreenSize.xy);
        uint2 positionSS8 = uint2(float2(uv.x + 3 * blur * hstep, uv.y + 3 * blur * vstep) * _ScreenSize.xy);
        uint2 positionSS9 = uint2(float2(uv.x + 4 * blur * hstep, uv.y + 4 * blur * vstep) * _ScreenSize.xy);
        
        // sample colors
        float4 v1 = LOAD_TEXTURE2D_X(_MainTex, positionSS1);
        float4 v2 = LOAD_TEXTURE2D_X(_MainTex, positionSS2);
        float4 v3 = LOAD_TEXTURE2D_X(_MainTex, positionSS3);
        float4 v4 = LOAD_TEXTURE2D_X(_MainTex, positionSS4);
        
        float4 v5 = LOAD_TEXTURE2D_X(_MainTex, positionSS5);
        
        float4 v6 = LOAD_TEXTURE2D_X(_MainTex, positionSS6);
        float4 v7 = LOAD_TEXTURE2D_X(_MainTex, positionSS7);
        float4 v8 = LOAD_TEXTURE2D_X(_MainTex, positionSS8);
        float4 v9 = LOAD_TEXTURE2D_X(_MainTex, positionSS9);

        sum.rgb = lerp(sum.rgb, v1.rgb, v1.a);
        sum.rgb = lerp(sum.rgb, v2.rgb, v2.a);
        sum.rgb = lerp(sum.rgb, v3.rgb, v3.a);
        sum.rgb = lerp(sum.rgb, v4.rgb, v4.a);
        sum.rgb = lerp(sum.rgb, v5.rgb, v5.a);
        sum.rgb = lerp(sum.rgb, v6.rgb, v6.a);
        sum.rgb = lerp(sum.rgb, v7.rgb, v7.a);
        sum.rgb = lerp(sum.rgb, v8.rgb, v8.a);
        sum.rgb = lerp(sum.rgb, v9.rgb, v9.a);

        // blur alpha with zero values
        sum.a = v1.a * 0.0162162162 + v2.a * 0.0540540541 + v3.a * 0.1216216216 + v4.a * 0.1945945946 + v5.a * 0.2270270270 + v6.a * 0.1945945946 + v7.a * 0.1216216216 + v8.a * 0.0540540541 + v9.a * 0.0162162162;

        return sum;
    }

    float4 Fragment(Varyings input) : SV_TARGET
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        //float resX = _MainTex_TexelSize.z;
        //float resY = _MainTex_TexelSize.w;
        input.texcoord.y = 1 - input.texcoord.y;
        float4 blurX = gaussianBlur(float2(1,0), input.texcoord, _ScreenSize.x);
        float4 blurY = gaussianBlur(float2(0,1), input.texcoord, _ScreenSize.y);
        
        // we get average value between blurs, because we don't want get colors lighter than outline color
        return (blurX + blurY) / 2;
    }

    ENDHLSL

    SubShader
    {
        // ZWrite On is required because we need write to depth and do it manually
        Cull Off ZWrite Off ZTest Always
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

/*
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #include "UnityCG.cginc"

			// Properties
            //sampler2D _MainTex;
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            // (1/pixelHidth, 1/pixelHeight, width, height)
			uniform float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
			float _Radius;

            struct appdata_t 
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                return o;
            }

            float4 gaussianBlur(float2 dir, float2 uv, float res)
            {
                // this will be our RGBA sum
                float4 sum = float4(0, 0, 0, 0);
                
                // the amount to blur, i.e. how far off center to sample from 
                // 1.0 -> blur by one pixel
                // 2.0 -> blur by two pixels, etc.
                float blur = _Radius / res; 
                
                // the direction of our blur
                // (1.0, 0.0) -> x-axis blur
                // (0.0, 1.0) -> y-axis blur
                float hstep = dir.x;
                float vstep = dir.y;
                
                // apply blurring, using a 9-tap filter with predefined gaussian weights
                
				// sample colors
				float4 v1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x - 4*blur*hstep, uv.y - 4.0*blur*vstep));
                float4 v2 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x - 3.0*blur*hstep, uv.y - 3.0*blur*vstep));
                float4 v3 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x - 2.0*blur*hstep, uv.y - 2.0*blur*vstep));
                float4 v4 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x - 1.0*blur*hstep, uv.y - 1.0*blur*vstep));
                
                float4 v5 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x, uv.y));
                
                float4 v6 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x + 1.0*blur*hstep, uv.y + 1.0*blur*vstep));
                float4 v7 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x + 2.0*blur*hstep, uv.y + 2.0*blur*vstep));
                float4 v8 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x + 3.0*blur*hstep, uv.y + 3.0*blur*vstep));
                float4 v9 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float2(uv.x + 4.0*blur*hstep, uv.y + 4.0*blur*vstep));

                sum.rgb = lerp(sum.rgb, v1.rgb, v1.a);
                sum.rgb = lerp(sum.rgb, v2.rgb, v2.a);
                sum.rgb = lerp(sum.rgb, v3.rgb, v3.a);
                sum.rgb = lerp(sum.rgb, v4.rgb, v4.a);
                sum.rgb = lerp(sum.rgb, v5.rgb, v5.a);
                sum.rgb = lerp(sum.rgb, v6.rgb, v6.a);
                sum.rgb = lerp(sum.rgb, v7.rgb, v7.a);
                sum.rgb = lerp(sum.rgb, v8.rgb, v8.a);
                sum.rgb = lerp(sum.rgb, v9.rgb, v9.a);

				// blur alpha with zero values
				sum.a = v1.a * 0.0162162162 + v2.a * 0.0540540541 + v3.a * 0.1216216216 + v4.a * 0.1945945946 + v5.a * 0.2270270270 + v6.a * 0.1945945946 + v7.a * 0.1216216216 + v8.a * 0.0540540541 + v9.a * 0.0162162162;
                
                return sum;
            }

			float4 frag(v2f_img input) : COLOR
			{
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float resX = _MainTex_TexelSize.z;
				float resY = _MainTex_TexelSize.w;
                float4 blurX = gaussianBlur(float2(1,0), input.uv, resX);
                float4 blurY = gaussianBlur(float2(0,1), input.uv, resY);
				
				// we get average value between blurs, because we don't want get colors lighter than outline color
				return (blurX + blurY) / 2;
				//return blurX + blurY;
			}

			ENDCG
		}
	}
    */
}