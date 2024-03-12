Shader "Hidden/ConvoShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha("alpha", Range(0, 1)) = 0.5

        
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

            float _Alpha;


            static const int WIDTH = 3;
            static const int HEIGHT = 3;
            float kernel[WIDTH * HEIGHT];
            float2 offsets[WIDTH * HEIGHT];

            
            
            fixed4 frag (v2f i) : SV_Target
            {

                //Screen size
                float stepSizeX = 1.0 / 1920.0;
                float stepSizeY = 1.0 / 1080.0;

                //Offsets
                offsets[0] = float2(-stepSizeX, stepSizeY);
                offsets[1] = float2(0, stepSizeY);
                offsets[2] = float2(stepSizeX, stepSizeY);
                offsets[3] = float2(-stepSizeX, 0);
                offsets[4] = float2(0, 0);
                offsets[5] = float2(stepSizeX, 0);
                offsets[6] = float2(-stepSizeX, -stepSizeY);
                offsets[7] = float2(0, -stepSizeY);
                offsets[8] = float2(stepSizeX, -stepSizeY);

                //Gaussian blur
                kernel[0] = 1.0f/16.0f;
                kernel[1] = 2.0f/16.0f;
                kernel[2] = 1.0f/16.0f;
                kernel[3] = 2.0f/16.0f;
                kernel[4] = 4.0f/16.0f;
                kernel[5] = 2.0f/16.0f;
                kernel[6] = 1.0f/16.0f;
                kernel[7] = 2.0f/16.0f;
                kernel[8] = 1.0f/16.0f;

                
                // //Gaussian blur
                // kernel[0] = -1;
                // kernel[1] = -1;
                // kernel[2] = -1;
                // kernel[3] = -1;
                // kernel[4] = -1;
                // kernel[5] = 8;
                // kernel[6] = -1;
                // kernel[7] = -1;
                // kernel[8] = -1;
                //
                //Normalized blur
                // kernel[0] = 1.0f/16.0f;
                // kernel[1] = 1.0f/16.0f;
                // kernel[2] = 1.0f/16.0f;
                // kernel[3] = 1.0f/16.0f;
                // kernel[4] = 1.0f/16.0f;
                // kernel[5] = 1.0f/16.0f;
                // kernel[6] = 1.0f/16.0f;
                // kernel[7] = 1.0f/16.0f;
                // kernel[8] = 1.0f/16.0f;


                kernel[0] = 0.0f;
                kernel[1] = 0.0f;
                kernel[2] = 0.0f;
                kernel[3] = 0.0f;
                kernel[4] = 1.0f;
                kernel[5] = 0.0f;
                kernel[6] = 0.0f;
                kernel[7] = 0.0f;
                kernel[8] = 0.0f;
                //All pixels in a 3x3 grid
                fixed4 fixedUVS[WIDTH * HEIGHT];

                for (int j = 0; j < WIDTH*HEIGHT; j++)
                {
                    fixedUVS[j] =tex2D(_MainTex, i.uv + offsets[j]);
                }
                fixed4 col= fixed4(0.0f,0.0f,0.0f,_Alpha);

                
                for (int l = 0; l < WIDTH*HEIGHT; l++)
                {
                    col += fixedUVS[l] * kernel[l];
                }

                col = fixed4(col);
                // col = tex2D(_MainTex, i.uv);

                return col ;
            }
            ENDCG
        }
    }
}
