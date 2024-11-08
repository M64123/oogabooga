Shader "KOI/StandardPixel"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 1)) = 1.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 200

        Cull Off // Disable backface culling to render both sides

        CGPROGRAM
        #pragma surface surf Standard alpha:clip addshadow

        sampler2D _MainTex;
        sampler2D _NormalMap;
        float _Cutoff;
        float _NormalStrength;
        float _Smoothness;
        float _Metallic;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            clip(c.a - _Cutoff);

            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Smoothness = _Smoothness;
            o.Metallic = _Metallic;

            // Apply normal map with strength adjustment
            half3 normalTex = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            o.Normal = normalize(o.Normal + normalTex * _NormalStrength);
        }
        ENDCG
    }

    FallBack "Diffuse"
}
