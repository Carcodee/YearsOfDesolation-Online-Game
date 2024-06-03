Shader "Hidden/Knife/Knife-HDRPOutline"
{
    // First pass and main variables
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

    TEXTURE2D_X(_InputTexture); // screen color texture
    //sampler2D _OutlineTarget; // outline color texture (also in screen space of course)
    TEXTURE2D_X(_OutlineTarget);
    sampler2D _Pattern; // outline pattern texture
    float2 _PatternTile; // outline pattern tiling
    float _PatternFillAmount; // outline pattern fill amount (alpha)
    float _Width; // outline width
    float _FillAmount; // outline color fill amount

    float4 LOAD_OUTLINE_X(uint2 positionSS)
    {
        uint2 outlinePixel = positionSS;
        outlinePixel.y = _ScreenSize.y - outlinePixel.y;

        return LOAD_TEXTURE2D_X(_OutlineTarget, outlinePixel);
    }
    
    // Sample screen color buffer and outline color buffer
    void Sample(Varyings input, uint2 positionSS, out float4 outlineColor, out float4 color)
    {
        float2 uv = positionSS / _ScreenSize.xy;

        float4 c = LOAD_TEXTURE2D_X(_InputTexture, positionSS);


        //outlineColor = tex2D(_OutlineTarget, uv);
        outlineColor = LOAD_OUTLINE_X(positionSS);
        color = LOAD_TEXTURE2D_X(_InputTexture, positionSS);
    }
    
    // Sample outline color buffer
    void Sample(Varyings input, uint2 positionSS, out float4 outlineColor)
    {
        float2 uv = positionSS / _ScreenSize.xy;

        // outlineColor = tex2D(_OutlineTarget, uv);
        outlineColor = LOAD_OUTLINE_X(positionSS);
    }

    float4 Fragment(Varyings input) : SV_Target0
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // Main idea:
        // 1. render outline
        // 2. in current pixel we check around 4-32 pixels
        // 3. if these pixels have outline alpha more than zero we draw outline
        // 4. blend all colors with alpha blend, but alpha we must blend with MAX function

        // screen space pixel position
        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        
        // iterations count
        // you can not use variable array size for offsets, so there are constants with multi-compile
        #if ITERATIONS_8
        const int iterations = 8;
        #elif ITERATIONS_16
        const int iterations = 16;
        #elif ITERATIONS_32
        const int iterations = 32;
        #else
        const int iterations = 4;
        #endif
        
        float width = _Width;

        // calculate offsets to sample "by circle" around of current pixel
        // can be optimized by predefining offsets array for x4, x8, x16, x32 iterations
        // but need change int2 to float2, and round values in sampling

        // also can be optimized by merging 2 loops in one (with next loop where we sample outline)
        int2 offsets[iterations];
        // calculate angleStep for one offset (in radians)
        float angleStep = radians(360 / iterations);
        for(int j = 0; j < iterations; j++)
        {
            // calculate angle for [j] iteration
            float angle = j * angleStep;

            // calculate pixel position offset
            float x = cos(angle) * width;
            float y = sin(angle) * width;
            
            // round it and write to offsets array
            offsets[j] = int2(round(x), round(y));
        }

        float4 c; // screen color buffer value
        float4 outlineColor; // outline color buffer value

        // sample screen color buffer and outline color buffer for current pixel
        Sample(input, positionSS, outlineColor, c);

        float4 outlineFinalColor = 0; // outline final color

        // outline loop
        for(int i = 0; i < iterations; i++)
        {
            int2 offset = offsets[i]; // offset for current iteration

            float4 outlineColor1; // outline color buffer value that sampled with offset
            int2 position = positionSS + offset; // screen space position with offset
            // clamp position to prevent looping of outline if current pixel is too close to borders of screen
            position.x = clamp(position.x, 0, _ScreenSize.x);
            position.y = clamp(position.y, 0, _ScreenSize.y);

            // sample outline color buffer with offset
            Sample(input, position, outlineColor1);

            // alpha blend (SrcAlpha OneMinusSrcAlpha) current final color value with next iteration outline color value
            outlineFinalColor.rgb = outlineFinalColor.rgb * (1 - outlineColor1.a) + outlineColor1.rgb * outlineColor1.a;
            // alpha blend by MAX function
            outlineFinalColor.a = max(outlineFinalColor.a, outlineColor1.a);
        }

        // smoothstep range [0; 0.01] because we can not divide by zero, instead of zero we set 0.01
        // outlineOnly - it is real outline of objects, we multiply outlineFinalColor by (one minus outlineColor alpha);
        float4 outlineOnly = outlineFinalColor * (1 - smoothstep(0, 0.01, outlineColor.a));

        // blend outlineOnly with outlineFinalColor by FillAmount, to get transaprent filling
        outlineFinalColor = lerp(outlineOnly, outlineFinalColor, _FillAmount);
        // sample pattern texture for current pixel
        float4 pattern = tex2D(_Pattern, input.texcoord * _PatternTile);
        // add pattern to outlineFinalColor, also pattern mask multiplied by Pattern fill amount and outlineColor
        // it let us draw pattern only in center of object (not on outline) with transparency and inherit color of outline
        outlineFinalColor = outlineFinalColor + pattern.r * _PatternFillAmount * outlineColor;

        // convert screen space color buffer to SRGB color space
        c.rgb = LinearToSRGB(c.rgb);
        // blend screen space color buffer with calculated outline
        // AlphaBlend SrcAlpha OneMinusSrcAlpha
        c.rgb = c.rgb * (1 - outlineFinalColor.a) + outlineFinalColor.rgb * outlineFinalColor.a;
        // convert screen space color buffer to Linear color space
        c.rgb = SRGBToLinear(c.rgb);

        return c;
    }

    // Second pass (for soft outline)
    // second pass also use some variables that defined in First Pass

    Varyings VertexSoft(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    TEXTURE2D_X(_BlurredOutlineTarget);
    float _Overglow;
    float _Softness;

    float4 FragmentSoft(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // Main idea is:
        // 1. render outline
        // 2. blur outline and save it in other RenderTexture
        // 3. do substraction: BlurredOutline.Alpha - RawOutline.Alpha
        // 4. blend colors with screen color buffer by alpha SrcAlpha OneMinusSrcAlpha
        // this give us soft outline effect

        // screen space pixel position
        uint2 positionSS = input.texcoord * _ScreenSize.xy;

        // sample screen color buffer
        float4 c = LOAD_TEXTURE2D_X(_InputTexture, positionSS);
        // sample blurred outline color buffer
        float4 blurredOutline = LOAD_TEXTURE2D_X(_BlurredOutlineTarget, positionSS);
        // sample outline pattern texture
        float4 pattern = tex2D(_Pattern, input.texcoord * _PatternTile);
        // sample raw outline color buffer
        //float4 outline = tex2D(_OutlineTarget, input.texcoord);
        float4 outline = LOAD_OUTLINE_X(positionSS);

        float4 finalOutline = blurredOutline;
        // do substraction and clamp it (we don't want values above zero) and add filling to result
        finalOutline.a = saturate(finalOutline.a - outline.a) + outline.a * _FillAmount;
        // also we may will want to do outline more lighter, and multiply it by (1 + OverGlow)
        finalOutline *= 1 + _Overglow;

        // add pattern to outline color
        finalOutline = finalOutline + pattern.r * _PatternFillAmount * outline;
        #if SOFTNESS
        // apply softness parameter
        finalOutline.a = smoothstep(0, _Softness, finalOutline.a);
        #endif

        // convert screen space color buffer to SRGB color space
        c.rgb = LinearToSRGB(c.rgb);
        // blend screen space color buffer with calculated outline
        // AlphaBlend SrcAlpha OneMinusSrcAlpha
        c.rgb = c.rgb * (1 - finalOutline.a) + finalOutline.rgb * finalOutline.a;
        // convert screen space color buffer to Linear color space
        c.rgb = SRGBToLinear(c.rgb);

        return c;
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass // First Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile __ ITERATIONS_8 ITERATIONS_16 ITERATIONS_32
            #pragma multi_compile __ STEREO_INSTANCING_ON
            #pragma multi_compile_instancing
            ENDHLSL
        }

        Cull Off ZWrite Off ZTest Always
        Pass // Second Pass
        {
            HLSLPROGRAM
            #pragma vertex VertexSoft
            #pragma fragment FragmentSoft
            #pragma multi_compile __ SOFTNESS
            #pragma multi_compile __ STEREO_INSTANCING_ON
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }

    Fallback Off
}
