Shader "Knife/Knife-HDRPOutline_Unlit"
{
    // this shader is very basic, it is not depend from HDRP
    // simple unlit shader with some modifications
    // you can easily create your own shader for outline with some Visual Shader Editor
    Properties
    {
        // per renderer data provided by script (OutlineObject) to MaterialPropertyBlock
        [PerRendererData] _Color ("Color", Color) = (1, 1, 1, 1)
        [PerRendererData] _MainTex("Base Color", 2D) = "white" {}
        [PerRendererData] _Threshold("Threshold", Range(0, 1)) = 0.5
        [PerRendererData] _FresnelScale("Fresnel Scale", Float) = 2
        [PerRendererData] _FresnelPower("Fresnel Power", Float) = 2
        // multi compile toggles for small optimizations
        // like when we don't want use mask texture so we don't need sample this
        [KeywordEnum(None, Alpha, Color)] _BaseColor("Base Color", Float) = 0
        [KeywordEnum(UV0, UV1, UV2, UV3, UV4)] _UVSet("UV Channel", Float) = 0
        //[KeywordEnum(NoFresnel, Fresnel)] _Fresnel("Fresnel Type", Float) = 0
        [Toggle(_FRESNEL)] _Fresnel("Fresnel", Float) = 0
        // ZTest compare function, with it we can render outline that not visible (GEqual), visible (LEqual) or always (Always).
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
        // ZWrite parameter
        [Toggle] _ZWrite("ZWrite", Float) = 0
    }
    SubShader
    {
        ZWrite [_ZWrite]
        ZTest [_ZTest]
		Offset -1, -1 // we need that offset to prevent Z-Fighting of polygons
        Blend SrcAlpha OneMinusSrcAlpha
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile UV0 UV1 UV2 UV3 UV4
            #pragma multi_compile _BASECOLOR_NONE _BASECOLOR_ALPHA _BASECOLOR_COLOR
            #pragma multi_compile __ _FRESNEL
            #pragma multi_compile_instancing
            #pragma multi_compile __ STEREO_INSTANCING_ON

            #include "UnityCG.cginc"
            #include "HDRPOutline.hlsl"

            v2f vert (appdata v)
            {
                return DefaultVert(v);
            }

            float4 frag (v2f i) : SV_TARGET
            {
                return DefaultFrag(i);
            }

            ENDCG
        }

        Pass // Mask pass (only for GEqual and Greater)
        {
            ZTest LEqual
            Blend One Zero
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile __ STEREO_INSTANCING_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_TARGET1
            {
                return 1;
            }

            ENDCG
        }
    }
}
