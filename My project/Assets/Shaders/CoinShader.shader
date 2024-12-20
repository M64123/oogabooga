Shader "Custom/SimpleCoinShader"
{
    Properties
    {
        _MainTex("Edge Texture", 2D) = "white" {}
        _TopTex("Top Texture", 2D) = "white" {}
        _BottomTex("Bottom Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
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

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.normalOS = v.normal;
            o.uv_MainTex = v.texcoord;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Asignar texturas de manera simple, sin l�gica adicional.
            // Esto es solo una muestra visual, ya que la l�gica se manejar� en los colliders.
            float yNormal = IN.normalOS.y;

            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);

            // Ajustar las texturas para las caras superior, inferior y borde
            if (yNormal > 0.5)
            {
                texColor = tex2D(_TopTex, IN.uv_MainTex);
            }
            else if (yNormal < -0.5)
            {
                texColor = tex2D(_BottomTex, IN.uv_MainTex);
            }
            else
            {
                texColor = tex2D(_MainTex, IN.uv_MainTex);
            }

            o.Albedo = texColor.rgb;
            o.Alpha = texColor.a;
        }

        ENDCG
    }
        FallBack "Diffuse"
}
