
sampler ColorMapSampler : register(s0); 

float4 PixelShaderFunction(float4 color: COLOR0, float2 texCoord : TEXCOORD0) : COLOR0 
{ 
    // fake chromatic aberration in the stupidest way possible 
    float2 texCoordOffset = float2(0.0014, 0); 
    float r = tex2D(ColorMapSampler, texCoord - texCoordOffset).r; 
    float g = tex2D(ColorMapSampler, texCoord).g; 
    float b = tex2D(ColorMapSampler, texCoord + texCoordOffset).b; 
    float4 imageColor = float4(r,g,b,1); 
 
    // combine everything 
    return color * imageColor;
} 

technique CRTFX
{
	pass P0
	{
		// shaders
		PixelShader  = compile ps_2_0 PixelShaderFunction();
		//AlphaBlendEnable = FALSE;
		//ColorWriteEnable = RED|GREEN|BLUE|ALPHA;
	}
}