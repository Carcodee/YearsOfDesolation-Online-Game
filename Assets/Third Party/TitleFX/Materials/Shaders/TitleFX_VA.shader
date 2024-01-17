// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TitleFX_VA"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_XTiling("XTiling", Float) = 1.2
		_YTiling("YTiling", Float) = 1
		[HDR]_MainColor("MainColor", Color) = (1.308841,1.308841,1.308841,1)
		_Main("Main", 2D) = "white" {}
		[HDR]_OutlineColor("OutlineColor", Color) = (6.198622,1.092255,0.4385816,1)
		_Outline("Outline", 2D) = "white" {}
		_MainMASK("MainMASK", 2D) = "white" {}
		_TransitionFactor("TransitionFactor", Float) = 1
		_DetailsMASK("DetailsMASK", 2D) = "white" {}
		_DetailsMaskDistortionMult("DetailsMask Distortion Mult", Float) = 1
		[Toggle(_INVERSEDIRECTION_ON)] _InverseDirection("InverseDirection", Float) = 0
		[Toggle(_UPDOWNDIRECTION_ON)] _UpDownDirection("Up/Down Direction", Float) = 0
		[Toggle]_AutoManualAnimation("Auto/Manual Animation", Float) = 0
		_TransitionSpeed("Transition Speed", Range( 2 , 5)) = 5
		_VignetteMaskFallof("Vignette Mask Fallof", Range( 0 , 0.5)) = 0.25
		_Animation_Factor("Animation_Factor", Range( 0 , 2)) = 0
		[ASEEnd]_VignetteMaskSize("VignetteMaskSize", Range( 0 , 1)) = 0.4

	}

	SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
		
		Stencil
		{
			Ref [_Stencil]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
			CompFront [_StencilComp]
			PassFront [_StencilOp]
			FailFront Keep
			ZFailFront Keep
			CompBack Always
			PassBack Keep
			FailBack Keep
			ZFailBack Keep
		}


		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		
		Pass
		{
			Name "Default"
		CGPROGRAM
			
			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			#include "UnityShaderVariables.cginc"
			#pragma shader_feature_local _INVERSEDIRECTION_ON
			#pragma shader_feature_local _UPDOWNDIRECTION_ON

			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};
			
			uniform fixed4 _Color;
			uniform fixed4 _TextureSampleAdd;
			uniform float4 _ClipRect;
			uniform sampler2D _MainTex;
			uniform sampler2D _Main;
			uniform sampler2D _MainMASK;
			uniform float _XTiling;
			uniform float _YTiling;
			uniform float _TransitionFactor;
			uniform float _AutoManualAnimation;
			uniform float _Animation_Factor;
			uniform float _TransitionSpeed;
			uniform sampler2D _DetailsMASK;
			uniform float _DetailsMaskDistortionMult;
			uniform float4 _MainColor;
			uniform sampler2D _Outline;
			uniform float4 _OutlineColor;
			uniform float _VignetteMaskSize;
			uniform float _VignetteMaskFallof;
			float4 MyCustomExpression160( float3 c, float a )
			{
				float4 colors = float4(c.x,c.y,c.z,a);
				return colors;
			}
			

			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID( IN );
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				OUT.worldPosition = IN.vertex;
				
				
				OUT.worldPosition.xyz +=  float3( 0, 0, 0 ) ;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 texCoord2 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult181 = (float2(_XTiling , _YTiling));
				float2 temp_output_33_0 = ( texCoord2 * appendResult181 );
				float Main_Mask168 = ( tex2D( _MainMASK, temp_output_33_0 ).r * _TransitionFactor );
				float Animation162 = (( _AutoManualAnimation )?( _Animation_Factor ):( _SinTime.w ));
				float ifLocalVar117 = 0;
				if( Animation162 <= 0.0 )
				ifLocalVar117 = Animation162;
				else
				ifLocalVar117 = ( Animation162 * -1.0 );
				float temp_output_39_0 = ( ( ifLocalVar117 + 0.5 ) * _TransitionSpeed );
				float clampResult45 = clamp( ( Main_Mask168 - temp_output_39_0 ) , 0.0 , 1.0 );
				float Details_Mask167 = ( tex2D( _DetailsMASK, temp_output_33_0 ).r * _DetailsMaskDistortionMult );
				float clampResult41 = clamp( ( Details_Mask167 - temp_output_39_0 ) , 0.0 , 1.0 );
				float AllMasks171 = ( clampResult45 + clampResult41 );
				#ifdef _UPDOWNDIRECTION_ON
				float2 staticSwitch190 = float2( 0,1 );
				#else
				float2 staticSwitch190 = float2( 1,0 );
				#endif
				#ifdef _INVERSEDIRECTION_ON
				float2 staticSwitch188 = ( staticSwitch190 * float2( -1,-1 ) );
				#else
				float2 staticSwitch188 = staticSwitch190;
				#endif
				float2 temp_output_53_0 = ( staticSwitch188 * 0.25 );
				float2 _Vector2 = float2(1,1);
				float2 _Vector1 = float2(0,0);
				float4 tex2DNode1 = tex2D( _Main, ( ( ( ( texCoord2 + ( 0.15 * AllMasks171 * staticSwitch188 ) ) - temp_output_53_0 ) * _Vector2 ) + _Vector1 ) );
				float ifLocalVar134 = 0;
				if( Animation162 <= 0.0 )
				ifLocalVar134 = Animation162;
				else
				ifLocalVar134 = ( Animation162 * -1.0 );
				float temp_output_136_0 = ( ( ifLocalVar134 + 0.5 ) * 3.5 );
				float clampResult140 = clamp( ( Main_Mask168 - temp_output_136_0 ) , 0.0 , 1.0 );
				float clampResult139 = clamp( ( Details_Mask167 - temp_output_136_0 ) , 0.0 , 1.0 );
				float temp_output_141_0 = ( clampResult140 + clampResult139 );
				float2 temp_output_156_0 = ( ( ( ( texCoord2 + ( 0.15 * temp_output_141_0 * staticSwitch188 ) ) - temp_output_53_0 ) * _Vector2 ) + _Vector1 );
				float clampResult150 = clamp( temp_output_141_0 , 0.0 , 1.0 );
				float3 c160 = ( ( tex2DNode1.r * _MainColor ) + ( ( tex2D( _Outline, temp_output_156_0 ).r * _OutlineColor ) * ( 1.0 - clampResult150 ) ) ).rgb;
				float clampResult119 = clamp( ( tex2DNode1.r * pow( AllMasks171 , 1.5 ) ) , 0.0 , 1.0 );
				float2 texCoord196 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_cast_1 = (( _VignetteMaskSize * 0.5 )).xx;
				float clampResult208 = clamp( ( distance( max( ( abs( ( texCoord196 - float2( 0.5,0.5 ) ) ) - temp_cast_1 ) , float2( 0,0 ) ) , float2( 0,0 ) ) / _VignetteMaskFallof ) , 0.0 , 1.0 );
				float a160 = ( clampResult119 * ( 1.0 - clampResult208 ) );
				float4 localMyCustomExpression160 = MyCustomExpression160( c160 , a160 );
				
				half4 color = localMyCustomExpression160;
				
				#ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
	CustomEditor "Title_fx_GUI"
	
	
}
/*ASEBEGIN
Version=18935
0;0;2560;1379;2759.205;3502.182;1.301673;True;False
Node;AmplifyShaderEditor.CommentaryNode;151;-1141.172,-2850.875;Inherit;False;3710.135;2778.623;Comment;69;132;34;2;133;118;33;134;117;136;37;43;39;40;44;138;137;140;41;45;139;46;10;11;141;9;54;128;129;53;8;121;131;52;77;150;1;149;143;120;142;147;146;148;124;154;152;156;153;155;157;162;163;167;168;169;170;171;173;175;176;177;178;185;188;189;190;192;193;195;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;180;-1456.292,-1730.016;Inherit;False;Property;_YTiling;YTiling;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-1462.792,-1843.116;Inherit;False;Property;_XTiling;XTiling;0;0;Create;True;0;0;0;False;0;False;1.2;1.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;38;-1876.412,-2758.222;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;209;-1882.161,-2545.982;Inherit;False;Property;_Animation_Factor;Animation_Factor;15;0;Create;True;0;0;0;False;0;False;0;1.109;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;210;-1524.936,-2613.91;Inherit;False;Property;_AutoManualAnimation;Auto/Manual Animation;12;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;181;-1306.791,-1822.316;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-999.0427,-1622.452;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;162;-1122.045,-2691.061;Inherit;False;Animation;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-759.3134,-1837.652;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;178;-287.0214,-1994.142;Inherit;False;Property;_DetailsMaskDistortionMult;DetailsMask Distortion Mult;9;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;37;-537.0887,-1869.645;Inherit;True;Property;_DetailsMASK;DetailsMASK;8;0;Create;True;0;0;0;False;0;False;-1;None;3fe2e7186ee6f82428cfc815f1b96796;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-858.9975,-2684.746;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;43;-590.9604,-2777.3;Inherit;True;Property;_MainMASK;MainMASK;6;0;Create;True;0;0;0;False;0;False;-1;None;3a3a1483cc479a34f8a018b14cda1694;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;177;-450.0214,-2582.142;Inherit;False;Property;_TransitionFactor;TransitionFactor;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;163;-1092.877,-203.004;Inherit;False;162;Animation;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;195;-363.5236,-2398.898;Inherit;False;Property;_TransitionSpeed;Transition Speed;13;0;Create;True;0;0;0;False;0;False;5;5;2;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;-251.0214,-2744.142;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;117;-657.2704,-2550.184;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-130.0214,-1835.142;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;133;-1091.172,-439.4522;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;167;54.51428,-1847.227;Inherit;False;Details_Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;168;-43.48572,-2778.227;Inherit;False;Main_Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;39;-86.1568,-2547.365;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;134;-912.8975,-473.5171;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;44;185.3876,-2771.05;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;40;183.8994,-2327.158;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;41;446.53,-2327.763;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;45;423.9894,-2769.559;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;170;-796.3436,-970.7168;Inherit;False;168;Main_Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;169;-762.0274,-257.7558;Inherit;False;167;Details_Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;11;-380.4928,-1344.917;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;136;-821.4336,-725.911;Inherit;False;ConstantBiasScale;-1;;4;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;3.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;192;-369.3208,-1170.04;Inherit;False;Constant;_Vector4;Vector 4;3;0;Create;True;0;0;0;False;0;False;0,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;137;-467.8958,-249.7125;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;138;-560.8911,-969.1431;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;838.0199,-2594.903;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;190;-142.3208,-1234.04;Inherit;False;Property;_UpDownDirection;Up/Down Direction;11;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;171;1022.871,-2599.084;Inherit;False;AllMasks;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;193;108.6792,-1092.04;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-1,-1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;140;-322.2892,-967.6523;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;139;-215.5566,-249.8936;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-68.93052,-1414.321;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;0.15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;188;220.3859,-1225.614;Inherit;False;Property;_InverseDirection;InverseDirection;10;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;141;39.31998,-278.0063;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;173;278.1086,-1483.003;Inherit;False;171;AllMasks;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;196;2180.713,-3453.658;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;128;484.3092,-709.7624;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;54;529.3167,-1157.478;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;0;False;0;False;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;189;-393.7108,-864.21;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;198;2423.787,-3446.03;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;197;2262.713,-3202.658;Inherit;False;Property;_VignetteMaskSize;VignetteMaskSize;16;0;Create;True;0;0;0;False;0;False;0.4;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;476.385,-1410.688;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.AbsOpNode;199;2657.833,-3421.818;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;200;2594.883,-3200.687;Inherit;False;2;2;0;FLOAT;0.7;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;621.6794,-1631.24;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;712.4386,-1227.202;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;129;656.9659,-733.1437;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;131;1032.116,-734.2673;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;152;967.5908,-1243.218;Inherit;False;Constant;_Vector2;Vector 2;4;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;52;928.1084,-1630.728;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;201;2835.383,-3378.238;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;153;1246.815,-1630.295;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;202;3040.253,-3376.238;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;157;1212.559,-1229.347;Inherit;False;Constant;_Vector1;Vector 1;6;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;154;1248.585,-730.5544;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;121;1335.111,-573.2615;Inherit;True;Property;_Outline;Outline;5;0;Create;True;0;0;0;False;0;False;None;39196b062061f794da6befceceb3b6a9;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;155;1459.595,-1634.747;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;204;2854.713,-3212.658;Inherit;False;Property;_VignetteMaskFallof;Vignette Mask Fallof;14;0;Create;True;0;0;0;False;0;False;0.25;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;172;668.8711,-3107.084;Inherit;False;171;AllMasks;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;77;1323.792,-2701.588;Inherit;True;Property;_Main;Main;3;0;Create;True;0;0;0;False;0;False;None;60efe16f69643404b98c844fb66f1a48;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;156;1393.441,-927.9559;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DistanceOpNode;203;3212.838,-3374.238;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;60;856.116,-3105.11;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;150;1438.126,-231.2522;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;120;1599.428,-955.588;Inherit;True;Property;_TextureSample1;Texture Sample 1;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;205;3386.271,-3375.238;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;143;1687.882,-489.6244;Inherit;False;Property;_OutlineColor;OutlineColor;4;1;[HDR];Create;True;0;0;0;False;0;False;6.198622,1.092255,0.4385816,1;73.92464,292.0157,51.98185,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;1596.582,-2405.709;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;142;1945.343,-641.9182;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;149;1979.981,-239.9813;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;1895.048,-3131.944;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;147;1662.906,-2749.031;Inherit;False;Property;_MainColor;MainColor;2;1;[HDR];Create;True;0;0;0;False;0;False;1.308841,1.308841,1.308841,1;1.124667,0.05299473,0.1824394,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;208;3508.35,-3208.004;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;119;2656.238,-3103.198;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;1936.926,-2592.909;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;2129.593,-643.1022;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;206;3631.814,-3374.753;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;124;2382.355,-1325.137;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;3553.908,-2134.124;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;160;2908.588,-1323.874;Inherit;False;float4 colors = float4(c.x,c.y,c.z,a)@$return colors@;4;Create;2;True;c;FLOAT3;0,0,0;In;;Inherit;False;True;a;FLOAT;0;In;;Inherit;False;My Custom Expression;True;False;0;;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SinTimeNode;132;-1057.722,-800.4966;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;34;-990.5675,-2022.324;Inherit;False;Constant;_Vector3;Vector 3;3;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;185;1538.63,-1082.637;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;158;3993.512,-1323.909;Float;False;True;-1;2;Title_fx_GUI;0;4;TitleFX_VA;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;False;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;True;True;True;True;True;0;True;-9;False;False;False;False;False;False;False;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;0;True;-11;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;210;0;38;4
WireConnection;210;1;209;0
WireConnection;181;0;179;0
WireConnection;181;1;180;0
WireConnection;162;0;210;0
WireConnection;33;0;2;0
WireConnection;33;1;181;0
WireConnection;37;1;33;0
WireConnection;118;0;162;0
WireConnection;43;1;33;0
WireConnection;175;0;43;1
WireConnection;175;1;177;0
WireConnection;117;0;162;0
WireConnection;117;2;118;0
WireConnection;117;3;162;0
WireConnection;117;4;162;0
WireConnection;176;0;37;1
WireConnection;176;1;178;0
WireConnection;133;0;163;0
WireConnection;167;0;176;0
WireConnection;168;0;175;0
WireConnection;39;3;117;0
WireConnection;39;2;195;0
WireConnection;134;0;163;0
WireConnection;134;2;133;0
WireConnection;134;3;163;0
WireConnection;134;4;163;0
WireConnection;44;0;168;0
WireConnection;44;1;39;0
WireConnection;40;0;167;0
WireConnection;40;1;39;0
WireConnection;41;0;40;0
WireConnection;45;0;44;0
WireConnection;136;3;134;0
WireConnection;137;0;169;0
WireConnection;137;1;136;0
WireConnection;138;0;170;0
WireConnection;138;1;136;0
WireConnection;46;0;45;0
WireConnection;46;1;41;0
WireConnection;190;1;11;0
WireConnection;190;0;192;0
WireConnection;171;0;46;0
WireConnection;193;0;190;0
WireConnection;140;0;138;0
WireConnection;139;0;137;0
WireConnection;188;1;190;0
WireConnection;188;0;193;0
WireConnection;141;0;140;0
WireConnection;141;1;139;0
WireConnection;128;0;10;0
WireConnection;128;1;141;0
WireConnection;128;2;188;0
WireConnection;189;0;2;0
WireConnection;198;0;196;0
WireConnection;9;0;10;0
WireConnection;9;1;173;0
WireConnection;9;2;188;0
WireConnection;199;0;198;0
WireConnection;200;0;197;0
WireConnection;8;0;2;0
WireConnection;8;1;9;0
WireConnection;53;0;188;0
WireConnection;53;1;54;0
WireConnection;129;0;189;0
WireConnection;129;1;128;0
WireConnection;131;0;129;0
WireConnection;131;1;53;0
WireConnection;52;0;8;0
WireConnection;52;1;53;0
WireConnection;201;0;199;0
WireConnection;201;1;200;0
WireConnection;153;0;52;0
WireConnection;153;1;152;0
WireConnection;202;0;201;0
WireConnection;154;0;131;0
WireConnection;154;1;152;0
WireConnection;155;0;153;0
WireConnection;155;1;157;0
WireConnection;156;0;154;0
WireConnection;156;1;157;0
WireConnection;203;0;202;0
WireConnection;60;0;172;0
WireConnection;150;0;141;0
WireConnection;120;0;121;0
WireConnection;120;1;156;0
WireConnection;205;0;203;0
WireConnection;205;1;204;0
WireConnection;1;0;77;0
WireConnection;1;1;155;0
WireConnection;142;0;120;1
WireConnection;142;1;143;0
WireConnection;149;0;150;0
WireConnection;51;0;1;1
WireConnection;51;1;60;0
WireConnection;208;0;205;0
WireConnection;119;0;51;0
WireConnection;146;0;1;1
WireConnection;146;1;147;0
WireConnection;148;0;142;0
WireConnection;148;1;149;0
WireConnection;206;0;208;0
WireConnection;124;0;146;0
WireConnection;124;1;148;0
WireConnection;207;0;119;0
WireConnection;207;1;206;0
WireConnection;160;0;124;0
WireConnection;160;1;207;0
WireConnection;185;0;156;0
WireConnection;158;0;160;0
ASEEND*/
//CHKSM=63893373C5721896570957C04426739E4E152C20