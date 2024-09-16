Shader "Custom/Road"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
            "Queue" = "Geometry+1"}
        Offset -1, -1
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows decal:blend
        #pragma target 3.0
        

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 noise = tex2D(_MainTex, IN.worldPos.xz * 0.01f);
            fixed4 c = _Color * (noise.y * 0.75f + 0.4f);
            
            float blend = IN.uv_MainTex.x;
            blend *= noise.x + 0.7f;
            blend = smoothstep(0.3f, 0.7f, blend);
            
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = blend;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
