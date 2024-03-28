Shader "Hidden/ImageShaderConfifs"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MinDifunationOffset ("MinDiff", Range(0, 1)) = 0.5
        _MaxDifunationOffset ("MaxDiff", Range(0, 1)) = 0.5
        _RoundedBordersRadius("RoundedBorders", Range(0, 1)) = 0
        
    }
    
    SubShader
    {
        Pass
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
            float _MinDifunationOffset;
            float _MaxDifunationOffset;
            float _RoundedBordersRadius;
            const uint k = 1103515245U;


            fixed4 frag (v2f i) : SV_Target
            {
                  // float d = length(max(abs(i.uv - (0.5, 0.5)),(0.8f,0.8f))- (0.8f,0.8f)) - _RoundedBordersRadius;
                  // float alpharounded=smoothstep(0.55, 0.45, abs(d / 0.2) * 5.0);
               
                float distanceForRoundedBorders =length((0.5,0.5) - i.uv);
                if (distanceForRoundedBorders > _RoundedBordersRadius)
                {
                    discard;
                }
                
                fixed4 col = tex2D(_MainTex, i.uv);
                // Calculate the distance from the center of the image to the current pixel
                float distance = length((i.uv) - float2(0.5, 0.5));

                // Normalize the distance
                float normalizedDistance = (distance / sqrt(0.3 * i.uv.x)) ;

                // Calculate the alpha value by lerping between the min and max alpha values
                float alpha = lerp(_MinDifunationOffset, _MaxDifunationOffset, normalizedDistance);
                float step= smoothstep( _MinDifunationOffset, sin(_Time.y*2)-1, _MaxDifunationOffset) ;
                
                // alpha= clamp(alpha,alpha,step);
                // Apply the alpha value to the color
                col = fixed4(col.rgb,alpha);
                return col ;
            }
            ENDCG
        }
    }
}
