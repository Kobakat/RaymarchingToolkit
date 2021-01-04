float4 opAdd(float4 d1, float4 d2) 
{
	float3 col = d1.rgb;

	if (d2.w < d1.w)
		col = d2.rgb;

	float dst = min(d1.w, d2.w);

	return float4(col, dst);
}

float4 opSubtract(float4 d1, float4 d2) 
{
	float3 col = d1.rgb;

	if (d2.w < d1.w)
		col = d2.rgb;

	float dst = max(d1.w, -d2.w);

	return float4(col, dst);
}

float4 opIntersect(float4 d1, float4 d2) 
{
	float3 col = lerp(d1.rgb, d2.rgb, 0.5f);
	float dst = max(d1.w, d2.w);

	return float4(col, dst);
}

float4 opBlend(float4 d1, float4 d2, float k) 
{
	float h = clamp(0.5 + 0.5*(d2.w - d1.w) / k, 0.0, 1.0);

	float3 col = lerp(d2.rgb, d1.rgb, h);
	float dst = lerp(d2.w, d1.w, h) - k * h * (1.0 - h);

	return float4(col, dst);
}