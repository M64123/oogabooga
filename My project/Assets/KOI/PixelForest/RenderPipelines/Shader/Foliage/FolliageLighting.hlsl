#ifndef FOLIAGE_LIGHTING_INCLUDED
#define FOLIAGE_LIGHTING_INCLUDED

struct FoliageLightingData {

	//Position and orientation
	float3 positionWS;
	float3 meshNormalWS;
	float3 shapeNormalWS;
	float3 viewDirectionWS;
	float4 shadowCoord;

	//Surface
	float3 albedo;
	float  smoothness;
	float  specularStrength;
	float  ambientOcclusion;
	float3 subsurfaceColor;
	float  thinness;
	float  scatteringStrength;

	//Baked lighting
	float3 bakedGI;
	float  subsurfaceAmbientStrength;
	float  enviroReflectStrength;
	float4 shadowMask;
	float  fogFactor;
};

//Translate a [0, 1] Smoothness value to an exponent
float GetSmoothnessPower(float rawSmoothness)
{
	return exp2(10 * rawSmoothness + 1);
}

#ifndef SHADERGRAPH_PREVIEW
float3 FoliageGlobalIllumination(FoliageLightingData d)
{
	float3 indirectDiffuse = d.albedo * d.bakedGI * d.ambientOcclusion + d.albedo * d.subsurfaceColor * (d.thinness * d.subsurfaceAmbientStrength);

	float3 reflectVector = reflect(-d.viewDirectionWS, d.meshNormalWS);

	float  frensel = Pow4(1 - saturate(dot(d.viewDirectionWS, d.meshNormalWS)));
	float3 indirectSpecular = GlossyEnvironmentReflection(
		reflectVector,
		RoughnessToPerceptualRoughness(1 - d.smoothness),
		d.ambientOcclusion) * (frensel * d.enviroReflectStrength * d.specularStrength);

	return indirectDiffuse + indirectSpecular;
}

float3 FoliageLightHandling(FoliageLightingData d, Light light)
{
	float3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);
	float3 tlucencyRadiance = light.color * d.subsurfaceColor * light.distanceAttenuation;

	float diffuse = saturate(dot(d.shapeNormalWS, light.direction));
	float specularDot = saturate(dot(d.meshNormalWS, normalize(light.direction + d.viewDirectionWS)));
	float specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse * d.specularStrength;

	float3 scatteringDirection = normalize(-light.direction + d.meshNormalWS * d.scatteringStrength);
	float  tlucencyDot = saturate(dot(d.viewDirectionWS, scatteringDirection));
	float  tlucency = pow(tlucencyDot, GetSmoothnessPower(d.smoothness)) * d.thinness;

	float3 color = d.albedo * radiance * (diffuse + specular) +
	d.albedo * tlucencyRadiance * tlucency;
	return color;
}
#endif


float3 CalculateFoliageLighting(FoliageLightingData d)
{
	#ifdef SHADERGRAPH_PREVIEW
	float3 lightDir = float3(0.5, 0.5, 0);
	float intensity = saturate(dot(d.shapeNormalWS, lightDir)) +
		pow(saturate(dot(d.meshNormalWS, normalize(d.viewDirectionWS + lightDir))), GetSmoothnessPower(d.smoothness));
	return d.albedo * intensity;
	#else

	//Get main light. Located in URP/ShaderLibrary/Lighting.hlsl
	Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);


	MixRealtimeAndBakedGI(mainLight, d.shapeNormalWS, d.bakedGI);

	float3 color = FoliageGlobalIllumination(d);

	//Shade the main light.
	color += FoliageLightHandling(d, mainLight);

	#ifdef _ADDITIONAL_LIGHTS
		uint numAdditionalLights = GetAdditionalLightsCount();
		for (uint lightIndex = 0; lightIndex < numAdditionalLights; lightIndex++)
		{
			Light light = GetAdditionalLight(lightIndex, d.positionWS, 1);
			color += FoliageLightHandling(d, light);
		}
	#endif

	color = MixFog(color, d.fogFactor);

	return color;
	#endif
}

void CalculateFoliageLighting_float(float3     Position, float3         MeshNormal, float3               ShapeNormal,
                                    float3     ViewDirection, float3    Albedo, float                    Smoothness, float SpecularStrength,
                                    float      AmbientOcclusion, float3 SubsurfaceColor, float           Thinness, float   ScatteringStrength,
                                    float2     LightmapUV, float        SubsurfaceAmbientStrength, float EnviroReflectStrength,
                                    out float3 Color)
{
	FoliageLightingData d;
	d.positionWS = Position;
	d.meshNormalWS = MeshNormal;
	d.shapeNormalWS = ShapeNormal;
	d.viewDirectionWS = ViewDirection;
	d.albedo = Albedo;
	d.smoothness = Smoothness;
	d.specularStrength = SpecularStrength;
	d.ambientOcclusion = AmbientOcclusion;
	d.subsurfaceColor = SubsurfaceColor;
	d.thinness = Thinness;
	d.scatteringStrength = ScatteringStrength;
	d.subsurfaceAmbientStrength = SubsurfaceAmbientStrength;
	d.enviroReflectStrength = SubsurfaceAmbientStrength;

	#ifdef SHADERGRAPH_PREVIEW
	d.shadowCoord = 0;
	d.bakedGI = 0;
	d.shadowMask = 0;
	d.fogFactor = 0;
	#else
	float4 clipPos = TransformWorldToHClip(Position);
	#if SHADOW_SCREEN
		d.shadowCoord = ComputeScreenPos(clipPos);
	#else
	d.shadowCoord = TransformWorldToShadowCoord(Position);
	#endif

	// The lightmap UV is usually in TEXCOORD1
	// If lightmaps are disabled, OUTPUT_LIGHTMAP_UV does nothing.
	float3 lightmapUV;
	OUTPUT_LIGHTMAP_UV(LightmapUV, unity_LightmapST, lightmapUV);
	// Samples spherical harmonics, which encode light probe data.
	float3 vertexSH;
	OUTPUT_SH(ShapeNormal, vertexSH);
	// This function calculates the final baked lighting from the light maps or probes
	d.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, ShapeNormal);
	d.shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
	//Fog
	d.fogFactor = ComputeFogFactor(clipPos.z);

	#endif

	Color = CalculateFoliageLighting(d);
}

#endif
