#include "UnityInstancing.cginc"

float4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
float _Threshold;
float _FresnelScale;
float _FresnelPower;

struct appdata
{
    float4 vertex : POSITION;
    
    #if _FRESNEL
    float4 normal : NORMAL;
    #endif
    #if UV0
    float2 uv : TEXCOORD0;
    #elif UV1
    float2 uv : TEXCOORD1;
    #elif UV2
    float2 uv : TEXCOORD2;
    #elif UV3
    float2 uv : TEXCOORD3;
    #elif UV4
    float2 uv : TEXCOORD4;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    
    #if _FRESNEL
    float4 worldNormal : TEXCOORD1;
    float4 worldPosition : TEXCOORD2;
    #endif
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f DefaultVert (appdata v)
{
    v2f o;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.vertex = UnityObjectToClipPos(v.vertex);
    #if _FRESNEL
    o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
    o.worldNormal.xyz = UnityObjectToWorldNormal(v.normal);
    o.worldNormal.w = 0;
    #endif
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

float4 DefaultFrag (v2f i)
{
    float4 color = _Color;
    #if _BASECOLOR_ALPHA
    color.a *= tex2D(_MainTex, i.uv).a;
    clip(color.a - (1 - _Threshold));
    #elif _BASECOLOR_COLOR
    color *= tex2D(_MainTex, i.uv);
    clip(color.a - (1 - _Threshold));
    #endif
    
    #if _FRESNEL
    float3 wPos = i.worldPosition.xyz;
    float3 wDir = UnityWorldSpaceViewDir(wPos);
    wDir = normalize(wDir);
    
    float3 wNormal = i.worldNormal.xyz;
    float fresnelDot = dot( wNormal, wDir );
    float fresnel = _FresnelScale * pow(1.0 - fresnelDot, _FresnelPower);
    color.a *= fresnel;
    #endif

    color.a = clamp(color.a, 0, 1);
    // color.rgb = i.stereoTargetEyeIndex;
    // color.a = 1;
    return color;
}