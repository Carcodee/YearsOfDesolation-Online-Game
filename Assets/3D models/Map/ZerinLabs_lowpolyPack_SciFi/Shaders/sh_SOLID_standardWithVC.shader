Shader "zerinlabs/sh_SOLID_standardWithVertexColor"
{
	Properties
	{
			_Color("Color", Color) = (1,1,1,1)
			_MetallicFactor("Metallic Factor",Range(0, 1)) = 0
			_GlossMapScale("Smoothness", Range(0, 1)) = 0
			_MainTex("Albedo", 2D) = "white" {}
			[NoScaleOffset]
			_MaskMap("Mask (R/G/B = metallic/smoothness/occlusion)", 2D) = "white" {}
			[NoScaleOffset]
			_BumpMap("Normal Map", 2D) = "bump" {}
			_BumpScale("Bump Scale", Range(0, 1)) = 1.0
			[Space(10)]
			_AOMapScale("AO amount", Range(0, 1)) = 0



			[Header(Vertex color properties)]
			_VCintensity("VC intensity", Range(0, 1)) = 1.0
			_VCtint("VC tint",  Color) = (1,1,1,1)
	}

		SubShader
			{
					Tags
					{
							"RenderType" = "Opaque"
					}

					LOD 200

					CGPROGRAM

				/*
				finalcolor:myFinalColor
								...works only in forward!
				exclude_path:deferred
								...forces forward rendering for the shader.
				*/

				// Physically based Standard lighting model, and enable shadows on all light types
				// #pragma surface surf Standard fullforwardshadows vertex:vert finalcolor:myFinalColor exclude_path:deferred 
				#pragma surface surf Standard fullforwardshadows vertex:vert

				// Use shader model 3.0 target, to get nicer looking lighting
				#pragma target 3.0

				sampler2D _MainTex;
				sampler2D _BumpMap;
				sampler2D _MaskMap;

				float _MetallicFactor;
				float _GlossMapScale;
				float _AOMapScale;

				float _BumpScale;

				fixed4 _Color;

				float _VCintensity;
				fixed4 _VCtint;

				struct Input
				{
						float2 uv_MainTex;
						float4 vcColor;

				};

				/*
				struct appdata_full
				{
						float4 vertex : POSITION;
						float4 tangent : TANGENT;
						float3 normal : NORMAL;
						fixed4 color : COLOR;
						float4 texcoord : TEXCOORD0;
						float4 texcoord1 : TEXCOORD1;
						half4 texcoord2 : TEXCOORD2;
						half4 texcoord3 : TEXCOORD3;
						half4 texcoord4 : TEXCOORD4;
						half4 texcoord5 : TEXCOORD5;
				};
				*/

				void vert(inout appdata_full v, out Input o)
				{
						UNITY_INITIALIZE_OUTPUT(Input, o);
						o.vcColor = v.color;
				}

				/*
				struct SurfaceOutputStandard
				{
						fixed3 Albedo;      // base (diffuse or specular) color
						fixed3 Normal;      // tangent space normal, if written
						half3 Emission;
						half Metallic;      // 0=non-metal, 1=metal
						half Smoothness;    // 0=rough, 1=smooth
						half Occlusion;     // occlusion (default 1)
						fixed Alpha;        // alpha for transparencies
				};
				*/

				void surf(Input IN, inout SurfaceOutputStandard o)
				{

					// Albedo
					fixed4 DF = tex2D(_MainTex, IN.uv_MainTex);

					fixed4 VC = min(IN.vcColor + _VCtint, fixed4(1.0,1.0,1.0,1.0));
					VC = lerp(fixed4(1.0,1.0,1.0,1.0), VC, _VCintensity);
					o.Albedo = DF * _Color * VC;

					// Normal
					o.Normal = lerp(fixed3(0.0,0.0,1.0), UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex)), _BumpScale).rgb;

					// Emissive
					o.Emission = half3(0.0,0.0,0.0);

					// Metallic & smoothness
					fixed4 MK = tex2D(_MaskMap, IN.uv_MainTex);
					o.Metallic = MK.r * _MetallicFactor;
					o.Smoothness = MK.g * _GlossMapScale;

					//Occlusion
					o.Occlusion = lerp(1.0, MK.b, _AOMapScale);

					//Alpha
					o.Alpha = 1.0;
			}

				//works only with forward rendering. you need to force it with the "exclude_path:deferred" option on the #pragma
				/*
				void myFinalColor(Input IN, SurfaceOutputStandard o, inout fixed4 color)
				{
						color *= _OverlayColor;
				}
				*/

		ENDCG

			}

				FallBack "Diffuse"
}
