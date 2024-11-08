Shader "KOI/Leaf"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _Color ("Main Color", Color) = (1,1,1,1)
        _WindStrength ("Wind Strength", Range(0,1)) = 0.5
        _WindSpeed ("Wind Speed", Range(0,10)) = 1.0
        _WindDirection ("Wind Direction", Vector) = (1,0,0)
        _WindFrequency ("Wind Frequency", Range(0,10)) = 1.0
        _WindGustStrength ("Wind Gust Strength", Range(0,1)) = 0.2
        _WindGustFrequency ("Wind Gust Frequency", Range(0,10)) = 2.0
        _Translucency ("Translucency", Range(0,1)) = 0.5
        _SunDirection ("Sun Direction", Vector) = (0, 1, 0, 0)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
        LOD 200

        Cull Off // Disable backface culling to render both sides

        CGPROGRAM
        #pragma surface surf Standard alpha:clip addshadow vertex:vert

        sampler2D _MainTex;
        float _Cutoff;
        fixed4 _Color;
        float _WindStrength;
        float _WindSpeed;
        float3 _WindDirection;
        float _WindFrequency;
        float _WindGustStrength;
        float _WindGustFrequency;
        float _Translucency;
        float4 _SunDirection;
        sampler2D _NormalMap;
        float _NormalStrength;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos; // Add worldPos to the input struct
        };

        // Function to generate Perlin noise
        float noise(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);

            float n = dot(i, float3(1.0, 57.0, 113.0));
            return lerp(lerp(lerp( frac(sin(n + dot(float3(0.0, 0.0, 0.0), float3(1.0, 57.0, 113.0))) * 43758.5453), 
                                    frac(sin(n + dot(float3(1.0, 0.0, 0.0), float3(1.0, 57.0, 113.0))) * 43758.5453), f.x),
                                lerp( frac(sin(n + dot(float3(0.0, 1.0, 0.0), float3(1.0, 57.0, 113.0))) * 43758.5453), 
                                    frac(sin(n + dot(float3(1.0, 1.0, 0.0), float3(1.0, 57.0, 113.0))) * 43758.5453), f.x), f.y),
                            lerp(lerp( frac(sin(n + dot(float3(0.0, 0.0, 1.0), float3(1.0, 57.0, 113.0))) * 43758.5453), 
                                    frac(sin(n + dot(float3(1.0, 0.0, 1.0), float3(1.0, 57.0, 113.0))) * 43758.5453), f.x),
                                lerp( frac(sin(n + dot(float3(0.0, 1.0, 1.0), float3(1.0, 57.0, 113.0))) * 43758.5453), 
                                    frac(sin(n + dot(float3(1.0, 1.0, 1.0), float3(1.0, 57.0, 113.0))) * 43758.5453), f.x), f.y), f.z);
        }

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldPos = worldPos;

            // Introduce randomness based on position data
            float noiseOffset = noise(worldPos * _WindFrequency) * 2.0 - 1.0;

            // Simplified wind movement with position-based randomness
            float waveFactor = sin(_Time.y * _WindSpeed + dot(worldPos, _WindDirection) + noiseOffset) * _WindStrength;
            float gustFactor = sin(_Time.y * _WindGustFrequency + dot(worldPos, _WindDirection) + noiseOffset) * _WindGustStrength;
            float windFactor = waveFactor + gustFactor;

            // Create a sway effect by modifying the vertex position
            v.vertex.xyz += v.normal * windFactor;
            o.uv_MainTex = v.texcoord;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            clip(c.a - _Cutoff); // Discard transparent pixels

            // Apply normal map
            half3 normalTex = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
            o.Normal = normalize(o.Normal + normalTex * _NormalStrength);

            // Add translucency effect
            half3 lightDir = normalize(_SunDirection.xyz);
            float NdotL = dot(o.Normal, lightDir);
            float3 translucency = _Translucency * _LightColor0.rgb * saturate(NdotL) * c.rgb;

            // Adjust translucency based on view direction relative to the light direction
            float3 viewDir = normalize(IN.worldPos - _WorldSpaceCameraPos);
            float viewDotLight = dot(viewDir, lightDir);
            float translucencyEffect = saturate(viewDotLight);
            translucency *= translucencyEffect;

            o.Emission = translucency;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
