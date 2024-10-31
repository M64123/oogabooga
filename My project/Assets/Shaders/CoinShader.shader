Shader "Custom/CoinShader"
{
    Properties
    {
        _MainTex ("Edge Texture", 2D) = "white" {}
        _TopTex ("Top Texture", 2D) = "white" {}
        _BottomTex ("Bottom Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _TopTex;
        sampler2D _BottomTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 normalOS; // Normal en espacio de objeto
        };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.normalOS = v.normal;
            o.uv_MainTex = v.texcoord;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float yNormal = IN.normalOS.y;

            fixed4 texColor;

            if (yNormal > 0.5)
            {
                // Cara superior
                texColor = tex2D(_TopTex, IN.uv_MainTex);
            }
            else if (yNormal < -0.5)
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
