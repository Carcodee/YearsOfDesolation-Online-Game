Shader "Hidden/Kronnect/Beautify/CopyDepthBiased" {
	Properties{
		_MainTex("", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
	}

		SubShader{
			Tags { "RenderType" = "Transparent" }
			Pass {
				Name "Beauify Copy Depth Biased"
				Cull [_BeautifyDoFExclusionCullMode]
			CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_instancing
		#include "UnityCG.cginc"

		struct v2f {
			float4 pos : SV_POSITION;
			float depth01 : TEXCOORD0;
		};

#ifdef UNITY_INSTANCING_ENABLED
    UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
        UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
    UNITY_INSTANCING_BUFFER_END(PerDrawSprite)
    #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)
#endif

CBUFFER_START(UnityPerDrawSprite)
#ifndef UNITY_INSTANCING_ENABLED
    fixed2 _Flip;
#endif
CBUFFER_END

float4 FlipSprite(in float3 pos, in fixed2 flip) {
    return float4(pos.xy * flip, pos.z, 1.0);
}

		float _BeautifyDepthBias;

		v2f vert(appdata_base v) {
			v2f o;
			if (any(_Flip < 0)) {
				v.vertex = FlipSprite(v.vertex, _Flip);
			}
			o.pos = UnityObjectToClipPos(v.vertex);
			o.depth01 = COMPUTE_DEPTH_01;
			o.depth01 *= _BeautifyDepthBias;
			return o;
		}
		float4 frag(v2f i) : SV_Target{
			return EncodeFloatRGBA(i.depth01);
		}
		ENDCG
		}
	}
}
