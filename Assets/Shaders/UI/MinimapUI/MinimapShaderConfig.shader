Shader "Hidden/ImageShaderConfifs"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Zoom ("Zoom", Range(0, 5)) = 1
        _Xpos ("X", Range(0, 5)) = 1
        _Ypos ("Y", Range(0, 5)) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        
        Blend SrcAlpha OneMinusSrcAlpha

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
            float _Zoom;
            float _Xpos;
            float _Ypos;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 fixedUvs= fixed2(i.uv.x + _Xpos , i.uv.y + _Ypos);
                fixed4 col = tex2D(_MainTex,fixedUvs* _Zoom);
                // Calculate the distance from the center of the image to the current pixel
                


                return col ;
            }
            ENDCG
        }
    }
}
