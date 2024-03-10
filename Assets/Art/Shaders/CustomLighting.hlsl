#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

struct CustomLightingData{
	float3 normalWS;
	float3 viewDirectionWS;

	float3 positionWS;

	float3 albedo;
	float smoothness;

	float ambientOcclusion;
	float3 bakedGI;
};

float GetSmoothnessPower(float rawSmoothness){
	return exp2(10*rawSmoothness + 1);
}

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData d, Light light){
	float3 radiance = light.color;

	float3 diffuse = saturate(dot(d.normalWS, light.direction));
	diffuse *= light.shadowAttenuation * light.distanceAttenuation;

	float3 h = SafeNormalize(light.direction + d.viewDirectionWS);
	float3 specularDot = saturate(dot(d.normalWS, h));
	float3 specular = pow(specularDot, GetSmoothnessPower(d.smoothness));
	specular *=  diffuse * d.smoothness;

	specular = specular > 0.2 ? 1 : 0;

	float3 indirectDiffuse = d.albedo * d.bakedGI * d.ambientOcclusion;

	float3 reflectionVector = reflect(-d.viewDirectionWS, d.normalWS);
	float fresnel = Pow4(1 - saturate(dot(d.viewDirectionWS, d.normalWS)));
	float3 indirectSpecular = GlossyEnvironmentReflection(reflectionVector, RoughnessToPerceptualRoughness(1 - d.smoothness), d.ambientOcclusion)*fresnel;

	//indirectSpecular = indirectSpecular > 0.2 ? 1 : 0;

	float3 color = d.albedo * radiance * (diffuse + indirectDiffuse + specular + indirectSpecular);

	return color;
}
#endif

float3 CalculateCustomLighting(CustomLightingData d){
#ifdef SHADERGRAPH_PREVIEW
	float3 lightDir = float3(0.5, 0.5, 0);
	float3 intensity = saturate(dot(d.normalWS, lightDir));
	return d.albedo * intensity;
#else
	
#if SHADOWS_SCREEN
	float4 clipPos = TransformWorldToShadowCoord(d.positionWS);
	float4 shadowCoord = ComputeScreenPosition(clipPos);
#else
	float4 shadowCoord = TransformWorldToShadowCoord(d.positionWS);
#endif

	Light light = GetMainLight();
	light.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);

	float3 color = 0;
	color += CustomLightHandling(d, light);

	int pixelLightCount = GetAdditionalLightsCount();
	for(int i = 0; i < pixelLightCount; i++){
		light = GetAdditionalLight(i, d.positionWS, 1);
		color += CustomLightHandling(d, light);
	}

	return color;
#endif
}

void CalculateCustomLighting_float(float3 Normal, float3 ViewDirection, float Smoothness, float3 Albedo, float AmbientOcclusion, float3 GloablGI, float3 PositionWS, out float3 Color){
	CustomLightingData d;
	d.normalWS = Normal;
	d.viewDirectionWS = ViewDirection;
	d.smoothness = Smoothness;
	d.albedo = Albedo;
	d.ambientOcclusion = AmbientOcclusion;
	d.bakedGI = GloablGI;
	d.positionWS = PositionWS;

	Color = CalculateCustomLighting(d);
}

#endif