///Header file containing the logic functions that calculate different lighting modes

//lightDir = direction of the light source
//normal = pixel normal
float Lambertian(float3 lightDir, float3 normal) 
{
	return dot(-lightDir.xyz, normal);
}


//lightDir = direction of the light source
//normal = pixel normal
//litMultiplier = How much brighter to make the "lit" side of the shape. 1 for default
//unlitMultiplier = How much darker to make the "unlit" side of the shape. 0 for pitch black
//flip angle = at what angle from the light direction is the pixel considered unshaded?
float CelShade(float3 lightDir, float3 normal, float litMultiplier, float unlitMultiplier)
{
	float dotprod = dot(-lightDir.xyz, normal);

	if (dotprod >= 0)
		return litMultiplier;
	else
		return unlitMultiplier;
}

//This overload allows for the angle between the normal of the shape surface and the light source
//To change 
//**Note this is significantly more expensive due to an acos call
//flipAngle = if the angle between the two vectors exceeds this, change the brightness based on unlitMultiplier
float CelShade(float3 lightDir, float3 normal, float litMultiplier, float unlitMultiplier, float flipAngle)
{
	float dotprod = dot(-lightDir.xyz, normal);
	float angle = degrees(acos(dotprod));

	if (angle < flipAngle)
		return litMultiplier;
	else
		return unlitMultiplier;
}