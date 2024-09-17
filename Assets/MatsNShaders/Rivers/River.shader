Shader "Custom/River"
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
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1"}

        CGPROGRAM
        #pragma surface surf Standard alpha
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            uv.x = uv.x * 0.06f + _Time.y * 0.005f;
            uv.y -= _Time.y * 0.25f;
            float4 noise = tex2D(_MainTex, uv);

            float2 uv2 = IN.uv_MainTex;
            uv2.x = uv2.x * 0.06f - _Time.y * 0.0052f;
            uv.y -= _Time.y * 0.27f;
            float4 noise2 = tex2D(_MainTex, uv);
            
            fixed4 c = saturate(_Color + (noise.r * noise2.a));
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
