///Header file containing the logic for filter functions

//Simply multiply the light by the scene color
//light = the value to multiply the color by based on the normal
//col = the color of the pixel

fixed4 NoFilter(float light, float3 col)
{
    return fixed4(light * col, 1);
}

//light = the value to multiply the color by based on the normal
//col = the color of the pixel
//mode = the enumeration for highlight filter type
//gradient = How many steps must a ray travel before the shape becomes "illuminated"
//nonHighlightColorStrength = A multiplier used to dull portions of the shape that have a low number of steps to reach
//highlightStrength = How brightly to color the highlight
//emissiveCol = the color for all shapes use take on and emmit 

fixed4 Highlight(float light, float3 col, int step, int mode, int gradient, float nonHighlightColorStrength, float highlightStrength, float3 emissiveCol) 
{
    fixed4 highlight = float4(0, 0, 0, 0);
    
    float mult = nonHighlightColorStrength;    
    if (step > gradient)
        mult = highlightStrength;

    switch (mode) 
    {        
        //Emissive color matches shape color
        case 0:                
            highlight = fixed4(col * light * mult, 1);
            break;
                    
        //Singular Color
        case 1:
            highlight = fixed4(emissiveCol * light * mult, 1);
            break;
    }

    return highlight;
}