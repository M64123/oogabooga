Shader "KOI/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _Color ("Main Color", Color) = (1,1,1,1)
        _WindStrength ("Wind Strength", Range(0,1)) = 0.5
        _WindSpeed ("Wind Speed", Range(0,10)) = 1.0
        _Translucency ("Translucency", Range(0,1)) = 0.5
        _SwayColorStrength ("Sway Color Strength", Range(0,1)) = 0.5
        _SwaySpeed ("Sway Speed", Range(0,10)) = 1.0
        _SwayWidth ("Sway Width", Float) = 1.0
        _SwayColor ("Sway Color", Color) = (1, 0.8, 0.6, 1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest" "RenderType"="TransparentCutout"
        }
        LOD 200

        Cull Off // Disable backface culling to render both sides

        CGPROGRAM
        #pragma surface surf Standard alpha:clip addshadow vertex:vert

        sampler2D _MainTex;
        float _Cutoff;
        fixed4 _Color;
        float _WindStrength;
        float _WindSpeed;
        float _Translucency;
        float _SwayColorStrength;
        float _SwaySpeed;
        float _SwayWidth;
        fixed4 _SwayColor;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos; // Add worldPos to the input struct
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldPos = worldPos;

            // Calculate the gradient factor based on the height of the vertex
            float heightFactor = saturate(worldPos.y); // Adjust this to control the gradient effect

            // Calculate the wind effect
            float windFactor = sin(_Time.y * _WindSpeed + dot(worldPos.xy, float2(1, 0))) * _WindStrength *
                heightFactor;
            v.vertex.xyz += v.normal * windFactor;

            o.uv_MainTex = v.texcoord;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            clip(c.a - _Cutoff); // Discard transparent pixels

            // Calculate the translucency effect
            half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
            float NdotL = dot(o.Normal, lightDir);
            float3 translucency = _Translucency * _LightColor0.rgb * saturate(NdotL) * c.rgb;

            // Adjust translucency based on view direction relative to the light direction
            float3 viewDir = normalize(IN.worldPos - _WorldSpaceCameraPos);
            float viewDotLight = dot(viewDir, lightDir);
            float translucencyEffect = saturate(viewDotLight);
            translucency *= translucencyEffect;

            // Add color sway effect based on wind offset
            float3 swayDir = float3(1, 0, 0); // Fixed sway direction
            float swayOffset = sin(_Time.y * _SwaySpeed + dot(IN.worldPos.xy, swayDir.xy) * _SwayWidth);
            float3 colorSway = lerp(c.rgb, _SwayColor.rgb, abs(swayOffset) * _SwayColorStrength);
            o.Albedo = colorSway;

            o.Emission = translucency;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
