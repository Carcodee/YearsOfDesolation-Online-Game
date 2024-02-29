Shader "Hidden/ImageShaderConfifs"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OnRepeatTexture ("OnRepeatTexture", 2D) = "blue" {}
        _RepeatOffSet("RepeatOffSet", Range(0, 5)) = 0.01
        _RepeatOffSet2("RepeatOffSet2", Range(0, 5)) = 0.01
        _RepeatOffSet3("RepeatOffSet3", Range(0, 5)) = 0.01
        _MovementSpeed("MovementSpeed", Range(0, 5)) = 0.01
        _Limit("Limit", Range(-5, 5)) = 0.01

        _MinOffset("MinOffset", Vector)= (0.5, 0.5, 0, 0)
        _MaxOffset("MaxOffset", Vector)=(0.5, 0.5, 0, 0)
        
        
    }
    
    SubShader
    {
        // No culling or depth
        Cull off ZWrite On ZTest Always
        
        Blend SrcAlpha OneMinusSrcAlpha

        Tags
	    {
		    "Queue"="Transparent"
		    "IgnoreProjector"="True"
		    "RenderType"="Transparent"
	    }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _OnRepeatTexture;
            float _RepeatOffSet;
            float _RepeatOffSet2;
            float _RepeatOffSet3;
            float _MovementSpeed;
            float _Limit;
            half2 _MinOffset;
            half2 _MaxOffset;
            
            half2 NewCoordFromOffset(half2 fragpos, float offsetFromCenter)
            {
                half2 center= (0.5,0.5);
                half2 offset= ( offsetFromCenter, offsetFromCenter);
                half2 dirFromCenter= (fragpos - center)*offsetFromCenter;
                half2 newUVS= center+dirFromCenter;
                return newUVS;
            }

            bool CheckBounds(half2 min, half2 max, half2 newUVS)
            {
                bool isOutsideCentralArea = newUVS.x < min.x|| newUVS.x > max.x||
                newUVS.y <  min.y || newUVS.y >  max.y;
                return isOutsideCentralArea;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                
                float timeBasedEffect = sin(_Time.y * _MovementSpeed* 3.14159 * 0.5)+_Limit;
                float movement1=timeBasedEffect+_RepeatOffSet;
                float movement2=timeBasedEffect+_RepeatOffSet2;
                float movement3=timeBasedEffect+_RepeatOffSet3;
                
                half2 newUVS1 = NewCoordFromOffset(i.uv, movement1);
                half2 newUVS2 = NewCoordFromOffset(i.uv, movement2);
                half2 newUVS3 = NewCoordFromOffset(i.uv, movement3);
                
                fixed4 text = tex2D(_MainTex, i.uv);

                fixed4 BorderText1 = tex2D(_OnRepeatTexture, newUVS1);
                bool isOutside1 = CheckBounds(_MinOffset, _MaxOffset, newUVS1);

                fixed4 BorderText2 = tex2D(_OnRepeatTexture, newUVS3);
                bool isOutside2 = CheckBounds(_MinOffset, _MaxOffset, newUVS2);

                fixed4 BorderText3 = tex2D(_OnRepeatTexture, newUVS3);
                bool isOutside3 = CheckBounds(_MinOffset, _MaxOffset, newUVS3);

                if (isOutside1)
                {
                    BorderText1 = tex2D(_MainTex, i.uv);
                }
                if (isOutside2)
                {
                    BorderText2 = tex2D(_MainTex, i.uv);
                }
                if (isOutside3)
                {
                    BorderText3 = tex2D(_MainTex, i.uv);
                }

                fixed4 totalBorder= BorderText1+BorderText2+BorderText3;

                fixed4 result= (text * totalBorder);

                return result;
            }
            ENDCG
        }
    }
}
