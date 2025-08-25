Shader "Custom/DoubleSidedGlass"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.87
        _Metallic("Metallic", Range(0,1)) = 0.0
        _EmissionColor("Emission Color", Color) = (0,0.5,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Cull Off   // <--- disables backface culling, makes it double-sided
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        fixed4 _EmissionColor;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = _EmissionColor.rgb;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
