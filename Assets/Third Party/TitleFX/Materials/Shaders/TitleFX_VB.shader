// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TitleFX_VB"
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
		_Main("Main", 2D) = "white" {}
		[HDR]_MainColor("MainColor", Color) = (1.308841,1.308841,1.308841,1)
		_MainMASK("MainMASK", 2D) = "white" {}
		_TransitionFactor("TransitionFactor", Range( -1 , 1)) = 1
		_DetailsMASK("DetailsMASK", 2D) = "white" {}
		_DetailsMaskDistortionMult("DetailsMask Distortion Mult", Range( 0 , 1)) = 1
		[Toggle(_INVERSEDIRECTION_ON)] _InverseDirection("InverseDirection", Float) = 0
		[Toggle(_FORMAT_ON)] _Format("1:1 | 1:2 Format", Float) = 0
		[Toggle]_AutoManualAnimation("Auto/Manual Animation", Float) = 0
		[Toggle(_UPDOWNDIRECTION_ON)] _UpDownDirection("Up/Down Direction", Float) = 1
		_Animation_Factor("Animation_Factor", Range( 0 , 2)) = 0
		_TransitionSpeed("Transition Speed", Range( 2 , 5)) = 2
		_VignetteMaskFallof("Vignette Mask Fallof", Range( 0 , 0.5)) = 0.25
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
			#pragma shader_feature_local _FORMAT_ON
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
			uniform float _AutoManualAnimation;
			uniform float _Animation_Factor;
			uniform float _TransitionFactor;
			uniform float _TransitionSpeed;
			uniform sampler2D _DetailsMASK;
			uniform float _DetailsMaskDistortionMult;
			uniform float4 _MainColor;
			uniform float _VignetteMaskSize;
			uniform float _VignetteMaskFallof;
			float4 MyCustomExpression215( float3 c, float a )
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
				#ifdef _FORMAT_ON
				float staticSwitch263 = 2.0;
				#else
				float staticSwitch263 = 1.0;
				#endif
				#ifdef _FORMAT_ON
				float2 staticSwitch265 = float2( 0.5,1 );
				#else
				float2 staticSwitch265 = float2( 1,1 );
				#endif
				float2 temp_output_259_0 = ( staticSwitch263 * staticSwitch265 );
				float2 UV209 = ( ( ( texCoord2 - float2( 0.5,0 ) ) * temp_output_259_0 ) + float2( 0.5,0 ) );
				float2 appendResult186 = (float2(_XTiling , _YTiling));
				float2 temp_output_33_0 = ( UV209 * appendResult186 );
				float Animation221 = (( _AutoManualAnimation )?( _Animation_Factor ):( _SinTime.w ));
				float temp_output_180_0 = ( 1.0 * Animation221 );
				#ifdef _UPDOWNDIRECTION_ON
				float2 staticSwitch256 = float2( 0,1 );
				#else
				float2 staticSwitch256 = float2( 1,0 );
				#endif
				#ifdef _INVERSEDIRECTION_ON
				float2 staticSwitch258 = ( staticSwitch256 * float2( -1,-1 ) );
				#else
				float2 staticSwitch258 = staticSwitch256;
				#endif
				float2 temp_output_157_0 = pow( ( temp_output_180_0 * staticSwitch258 ) , 2.0 );
				float ifLocalVar117 = 0;
				if( temp_output_180_0 <= 0.0 )
				ifLocalVar117 = temp_output_180_0;
				else
				ifLocalVar117 = ( temp_output_180_0 * -1.0 );
				float temp_output_39_0 = ( ( ifLocalVar117 + 0.5 ) * _TransitionSpeed );
				float clampResult45 = clamp( ( ( tex2D( _MainMASK, ( temp_output_33_0 + temp_output_157_0 ) ).r * _TransitionFactor ) - temp_output_39_0 ) , 0.0 , 1.0 );
				float clampResult41 = clamp( ( ( tex2D( _DetailsMASK, ( temp_output_33_0 + temp_output_157_0 ) ).r * _DetailsMaskDistortionMult ) - temp_output_39_0 ) , 0.0 , 1.0 );
				float temp_output_46_0 = ( clampResult45 + clampResult41 );
				float MaskData187 = temp_output_46_0;
				#ifdef _FORMAT_ON
				float staticSwitch261 = 0.75;
				#else
				float staticSwitch261 = 0.5;
				#endif
				float2 temp_output_52_0 = ( ( UV209 + ( MaskData187 * staticSwitch261 * staticSwitch258 ) ) - ( staticSwitch258 * temp_output_259_0 ) );
				float4 tex2DNode1 = tex2D( _Main, temp_output_52_0 );
				float temp_output_51_0 = ( tex2DNode1.r * pow( ( temp_output_46_0 * 0.5 ) , 2.0 ) );
				float4 clampResult253 = clamp( ( ( ( 1.0 - ( temp_output_51_0 - 1.5 ) ) * ( temp_output_51_0 - 1.6 ) ) * _MainColor * 8.0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float3 c215 = ( ( tex2DNode1 * _MainColor ) + clampResult253 ).rgb;
				float2 texCoord279 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_cast_1 = (( _VignetteMaskSize * 0.5 )).xx;
				float clampResult301 = clamp( ( distance( max( ( abs( ( texCoord279 - float2( 0.5,0.5 ) ) ) - temp_cast_1 ) , float2( 0,0 ) ) , float2( 0,0 ) ) / _VignetteMaskFallof ) , 0.0 , 1.0 );
				float a215 = ( temp_output_51_0 * ( 1.0 - clampResult301 ) );
				float4 localMyCustomExpression215 = MyCustomExpression215( c215 , a215 );
				float4 clampResult224 = clamp( localMyCustomExpression215 , float4( 0,0,0,0 ) , float4( 10000,10000,10000,10000 ) );
				
				half4 color = clampResult224;
				
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
	CustomEditor "Title_fx_GUI_V2"
	
	
}
/*ASEBEGIN
Version=18935
9;39;2560;1361;-1281.24;425.8186;1;True;False
Node;AmplifyShaderEditor.Vector2Node;266;-2957.588,709.009;Inherit;False;Constant;_Vector5;Vector 5;16;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;201;-2956.365,427.5073;Inherit;False;Constant;_UV_Scale;UV_Scale;0;0;Create;True;0;0;0;False;0;False;2;2;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;264;-2748.588,539.009;Inherit;False;Constant;_Float5;Float 5;16;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;260;-2970.476,583.3177;Inherit;False;Constant;_Vector3;Vector 3;16;0;Create;True;0;0;0;False;0;False;0.5,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2846.314,893.6484;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;263;-2523.588,398.009;Inherit;False;Property;_Format;1:1 | 1:2 Format;10;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;265;-2541.588,572.009;Inherit;False;Property;_Format;1:1 | 1:2 Format;9;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;202;-2404.331,894.9604;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;259;-2288.476,552.3177;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;203;-2176.345,755.7054;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;222;-2616.391,-1159.516;Inherit;False;Property;_Animation_Factor;Animation_Factor;13;0;Create;True;0;0;0;False;0;False;0;1.109;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;255;-1686.998,1536.517;Inherit;False;Constant;_Vector4;Vector 4;3;0;Create;True;0;0;0;False;0;False;0,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;254;-1698.17,1361.64;Inherit;False;Constant;_Vector2;Vector 2;3;0;Create;True;0;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SinTimeNode;38;-2679.088,-967.9436;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;204;-2161.331,1003.96;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;184;-2964.322,-562.7751;Inherit;False;Property;_XTiling;XTiling;0;0;Create;True;0;0;0;False;0;False;1.2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;-2957.822,-449.6752;Inherit;False;Property;_YTiling;YTiling;1;0;Create;True;0;0;0;False;0;False;1;1.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;223;-2251.357,-898.1202;Inherit;False;Property;_AutoManualAnimation;Auto/Manual Animation;11;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;256;-1459.998,1472.517;Inherit;False;Property;_UpDownDirection;Up/Down Direction;12;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;209;-1967.898,1006.822;Inherit;False;UV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;186;-2808.321,-541.9752;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;210;-2802.265,-385.6785;Inherit;False;209;UV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;257;-1165.998,1585.517;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-1,-1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;221;-1978.391,-897.5161;Inherit;False;Animation;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;258;-1056.386,1306.308;Inherit;False;Property;_InverseDirection;InverseDirection;9;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-2578.769,-573.9227;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;180;-1769.783,-779.3179;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;274;-2422.76,-302.8186;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;153;-1874.106,-548.8521;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;157;-1675.469,-547.9989;Inherit;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;275;-2321.76,-126.8186;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-1455.821,-782.3053;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;151;-1437.898,-107.0731;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;152;-1568.95,-1107.182;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;43;-1163.112,-1134.168;Inherit;True;Property;_MainMASK;MainMASK;5;0;Create;True;0;0;0;False;0;False;-1;None;1497a3038200a9f4eb6b85614958dc03;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;217;-1997.954,-1417.33;Inherit;False;Property;_TransitionFactor;TransitionFactor;6;0;Create;True;0;0;0;False;0;False;1;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;37;-1169.111,-135.2785;Inherit;True;Property;_DetailsMASK;DetailsMASK;7;0;Create;True;0;0;0;False;0;False;-1;None;1497a3038200a9f4eb6b85614958dc03;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;219;-807.3392,-280.6351;Inherit;False;Property;_DetailsMaskDistortionMult;DetailsMask Distortion Mult;8;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;234;-1205.851,-463.8667;Inherit;False;Property;_TransitionSpeed;Transition Speed;14;0;Create;True;0;0;0;False;0;False;2;2;2;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;117;-1114.657,-692.3558;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;-809.9537,-1436.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;220;-608.3392,-121.6351;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;39;-894.4338,-835.7504;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;44;-629.1749,-1099.421;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;40;-632.1315,-658.9529;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;41;-368.0323,-656.1331;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;45;-390.5731,-1097.93;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-156.5423,-921.2731;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;262;-144.542,1582.859;Inherit;False;Constant;_Float4;Float 4;15;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;187;68.60535,-788.6466;Inherit;False;MaskData;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-193.7419,1428.883;Inherit;False;Constant;_Offset;Offset;0;0;Create;True;0;0;0;False;0;False;0.75;0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;261;5.458008,1501.859;Inherit;False;Property;_Format;1:1 | 1:2 Format;12;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;188;-127.7988,1162.21;Inherit;False;187;MaskData;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;91.86524,1286.297;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;211;68.99426,850.323;Inherit;False;209;UV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;279;1789.24,84.1814;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;141.5214,1701.406;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;387.0764,857.957;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;300;1871.24,335.1814;Inherit;False;Property;_VignetteMaskSize;VignetteMaskSize;16;0;Create;True;0;0;0;False;0;False;0.4;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;268;699.5391,-924.224;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;77;1146.678,822.3198;Inherit;True;Property;_Main;Main;3;0;Create;True;0;0;0;False;0;False;None;b0c70b723e8976d41abc5e0af255229a;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleSubtractOpNode;52;724.5054,1118.465;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;289;2032.314,91.80945;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;1;1507.917,1095.293;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;60;1088.417,-916.6091;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;295;2203.41,337.1523;Inherit;False;2;2;0;FLOAT;0.7;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;291;2266.36,116.0209;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;2066.668,1158.027;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;290;2443.91,159.6016;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;293;2648.78,161.6019;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;246;1863.041,1489.841;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;297;2463.24,325.1814;Inherit;False;Property;_VignetteMaskFallof;Vignette Mask Fallof;15;0;Create;True;0;0;0;False;0;False;0.25;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;249;1863.041,1600.154;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;294;2821.365,163.6015;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;247;2063.512,1502.569;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;248;2263.985,1500.448;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;252;2402.938,1660.614;Inherit;False;Constant;_Float0;Float 0;14;0;Create;True;0;0;0;False;0;False;8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;292;2994.797,162.6016;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;147;1951.744,471.702;Inherit;False;Property;_MainColor;MainColor;4;1;[HDR];Create;True;0;0;0;False;0;False;1.308841,1.308841,1.308841,1;1,0.2375007,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;251;2547.192,1502.569;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;10,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;301;3146.24,256.1814;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;287;3282.24,159.1814;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;253;2741.302,1493.023;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;2366.316,708.6823;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;232;2765.285,708.1685;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;272;2615.24,1158.181;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;215;2981.961,1133.771;Inherit;False;float4 colors = float4(c.x,c.y,c.z,a)@$return colors@;4;Create;2;True;c;FLOAT3;0,0,0;In;;Inherit;False;True;a;FLOAT;0;In;;Inherit;False;My Custom Expression;True;False;0;;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-2170.783,-516.3179;Inherit;False;Property;_Transition;Transition;2;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;181;-2290.783,-336.3179;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureTransformNode;212;-2559.19,1278.812;Inherit;False;-1;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.SimpleSubtractOpNode;189;955.1721,1508.399;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;224;3282.977,717.2006;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;10000,10000,10000,10000;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;1087.158,1398.144;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-52.58681,1768.103;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;190;1224.172,1508.399;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;155;-851.477,1816.487;Inherit;False;Constant;_Vector1;Vector 1;6;0;Create;True;0;0;0;False;0;False;0,-1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;11;-844.4369,1961.557;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FractNode;182;-2095.783,-241.3179;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-2397.783,-222.3179;Inherit;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;299;2191.24,-282.8186;Inherit;False;BoxMask;-1;;2;9dce4093ad5a42b4aa255f0153c4f209;0;4;1;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;10;FLOAT3;0,0,0;False;17;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;191;935.1382,1297.946;Inherit;False;Constant;_Float3;Float 3;7;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;214;3493.082,979.55;Float;False;True;-1;2;Title_fx_GUI_V2;0;4;TitleFX_VB;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;False;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;True;True;True;True;True;0;True;-9;False;False;False;False;False;False;False;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;0;True;-11;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;263;1;264;0
WireConnection;263;0;201;0
WireConnection;265;1;266;0
WireConnection;265;0;260;0
WireConnection;202;0;2;0
WireConnection;259;0;263;0
WireConnection;259;1;265;0
WireConnection;203;0;202;0
WireConnection;203;1;259;0
WireConnection;204;0;203;0
WireConnection;223;0;38;4
WireConnection;223;1;222;0
WireConnection;256;1;254;0
WireConnection;256;0;255;0
WireConnection;209;0;204;0
WireConnection;186;0;184;0
WireConnection;186;1;185;0
WireConnection;257;0;256;0
WireConnection;221;0;223;0
WireConnection;258;1;256;0
WireConnection;258;0;257;0
WireConnection;33;0;210;0
WireConnection;33;1;186;0
WireConnection;180;1;221;0
WireConnection;274;0;33;0
WireConnection;153;0;180;0
WireConnection;153;1;258;0
WireConnection;157;0;153;0
WireConnection;275;0;274;0
WireConnection;118;0;180;0
WireConnection;151;0;275;0
WireConnection;151;1;157;0
WireConnection;152;0;33;0
WireConnection;152;1;157;0
WireConnection;43;1;152;0
WireConnection;37;1;151;0
WireConnection;117;0;180;0
WireConnection;117;2;118;0
WireConnection;117;3;180;0
WireConnection;117;4;180;0
WireConnection;218;0;43;1
WireConnection;218;1;217;0
WireConnection;220;0;37;1
WireConnection;220;1;219;0
WireConnection;39;3;117;0
WireConnection;39;2;234;0
WireConnection;44;0;218;0
WireConnection;44;1;39;0
WireConnection;40;0;220;0
WireConnection;40;1;39;0
WireConnection;41;0;40;0
WireConnection;45;0;44;0
WireConnection;46;0;45;0
WireConnection;46;1;41;0
WireConnection;187;0;46;0
WireConnection;261;1;262;0
WireConnection;261;0;10;0
WireConnection;9;0;188;0
WireConnection;9;1;261;0
WireConnection;9;2;258;0
WireConnection;53;0;258;0
WireConnection;53;1;259;0
WireConnection;8;0;211;0
WireConnection;8;1;9;0
WireConnection;268;0;46;0
WireConnection;52;0;8;0
WireConnection;52;1;53;0
WireConnection;289;0;279;0
WireConnection;1;0;77;0
WireConnection;1;1;52;0
WireConnection;60;0;268;0
WireConnection;295;0;300;0
WireConnection;291;0;289;0
WireConnection;51;0;1;1
WireConnection;51;1;60;0
WireConnection;290;0;291;0
WireConnection;290;1;295;0
WireConnection;293;0;290;0
WireConnection;246;0;51;0
WireConnection;249;0;51;0
WireConnection;294;0;293;0
WireConnection;247;0;246;0
WireConnection;248;0;247;0
WireConnection;248;1;249;0
WireConnection;292;0;294;0
WireConnection;292;1;297;0
WireConnection;251;0;248;0
WireConnection;251;1;147;0
WireConnection;251;2;252;0
WireConnection;301;0;292;0
WireConnection;287;0;301;0
WireConnection;253;0;251;0
WireConnection;146;0;1;0
WireConnection;146;1;147;0
WireConnection;232;0;146;0
WireConnection;232;1;253;0
WireConnection;272;0;51;0
WireConnection;272;1;287;0
WireConnection;215;0;232;0
WireConnection;215;1;272;0
WireConnection;181;0;183;0
WireConnection;189;0;52;0
WireConnection;224;0;215;0
WireConnection;178;0;189;0
WireConnection;178;1;191;0
WireConnection;190;0;178;0
WireConnection;182;0;181;0
WireConnection;214;0;224;0
ASEEND*/
//CHKSM=38411B3D41AC22D52E5FF59797ACABF91FE8B40F