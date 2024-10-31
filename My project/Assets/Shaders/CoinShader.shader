Shader "Custom/CoinShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _TopTex ("Top Texture", 2D) = "white" {}
        _BottomTex ("Bottom Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _TopTex;
        sampler2D _BottomTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 normalWorld = normalize(IN.worldNormal);

            fixed4 texColor;

            if (normalWorld.y > 0.5)
            {
                // Cara superior
                texColor = tex2D(_TopTex, IN.uv_MainTex);
            }
            else if (normalWorld.y < -0.5)
            {
                // Cara inferior
                texColor = tex2D(_BottomTex, IN.uv_MainTex);
            }
            else
            {
                // Borde
                texColor = tex2D(_MainTex, IN.uv_MainTex);
            }

            o.Albedo = texColor.rgb;
            o.Alpha = texColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
