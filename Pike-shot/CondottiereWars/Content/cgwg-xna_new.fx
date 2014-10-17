/*
Copyright (C) 2003 Ryan A. Nunn

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

Modified by David St-Louis in a simpler form that also accept a 
custom resolution.
*/

//
// Source Texture Definition for Scaler Effects
//

// Size of one Texel
float2 TexelSize;

// Spritebatch stuff
float4x4 MatrixTransform;
float2 Viewport;

//
// Source Texture
//
texture SourceTexture				: SOURCETEXTURE;

sampler	SourceSampler = sampler_state {
	Texture	  = (SourceTexture);
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = NONE;
	AddressU  = Clamp;
	AddressV  = Clamp;
};

sampler	BilinearSourceSampler = sampler_state {
	Texture	  = (SourceTexture);
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = NONE;
	AddressU  = Clamp;
	AddressV  = Clamp;
};

sampler	SRGBSourceSampler = sampler_state {
	Texture	  = (SourceTexture);
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = NONE;
	AddressU  = Clamp;
	AddressV  = Clamp;
};

sampler	SRGBBilinearSourceSampler = sampler_state {
	Texture	  = (SourceTexture);
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = NONE;
	AddressU  = Clamp;
	AddressV  = Clamp;
};

sampler SourceBorderSampler = sampler_state {
	Texture	  = (SourceTexture);
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = NONE;
	AddressU  = Clamp;
	AddressV  = Clamp;
};


struct VS_OUTPUT_PRODUCT
{
	float4 Position		: POSITION;
	float2 TexCoord		: TEXCOORD0;
	float2 abspos		: TEXCOORD1;
};



// vertex shader
VS_OUTPUT_PRODUCT VS_Product(
				in float4 color    : COLOR0,
                in float2 texCoord : TEXCOORD0,
                in float4 position : POSITION0)
{
	VS_OUTPUT_PRODUCT Out;

	// Half pixel offset for correct texel centering.
	Out.Position.xy = position.xy - 0.5;

	// Viewport adjustment.
	Out.Position.xy = position.xy / Viewport;
	Out.Position.xy *= float2(2, -2);
	Out.Position.xy -= float2(1, -1);
	Out.Position.z = 0;
	Out.Position.w = 1;

	Out.TexCoord = texCoord;

	// Resulting X pixel-coordinate of the pixel we're drawing.
	// Assumes (-0.5, 0.5) quad and output size in World matrix
	// as currently done in DOSBox D3D patch
	Out.abspos = position;//float2((position.x + 0.5) * Viewport.x, (position.y - 0.5) * (-Viewport.y));

	return Out;
}

#define CURVATURE
//#define LINEAR_PROCESSING // [dsl] Not sure what this does

// Macros.
#define distortion 0.1 // [dsl] Default was .05, but curvature was too low
#define FIX(c) max(abs(c), 1e-5);
#define PI 3.141592653589

#ifdef REF_LEVELS
#       define LEVELS(c) max((c - 16.0 / 255.0) * 255.0 / (235.0 - 16.0), 0.0)
#else
#       define LEVELS(c) c
#endif

#ifdef LINEAR_PROCESSING
#       define TEX2D(c) pow(LEVELS(tex2D(SourceBorderSampler, (c))), inputGamma)
#else
#       define TEX2D(c) LEVELS(tex2D(SourceBorderSampler, (c)))
#endif

// Simulate a CRT gamma of 2.4.
#define inputGamma  2.4

// Compensate for the standard sRGB gamma of 2.2.
#define outputGamma 2.2

// Apply radial distortion to the given coordinate.
float2 radialDistortion(float2 coord, float2 pos)
{
	pos /= float2(Viewport.x, Viewport.y);
	float2 cc = pos - 0.5;
	float dist = dot(cc, cc) * distortion;
	return coord * (pos + cc * (1.0 + dist) * dist) / pos;
}

// Calculate the influence of a scanline on the current pixel.
//
// 'distance' is the distance in texture coordinates from the current
// pixel to the scanline in question.
// 'color' is the colour of the scanline at the horizontal location of
// the current pixel.
float4 scanlineWeights(float distance, float4 color)
{
	// The "width" of the scanline beam is set as 2*(1 + x^4) for
	// each RGB channel.
	float4 wid = 2.0 + 2.0 * pow(color, 4.0);

	// The "weights" lines basically specify the formula that gives
	// you the profile of the beam, i.e. the intensity as
	// a function of distance from the vertical center of the
	// scanline. In this case, it is gaussian if width=2, and
	// becomes nongaussian for larger widths. Ideally this should
	// be normalized so that the integral across the beam is
	// independent of its width. That is, for a narrower beam
	// "weights" should have a higher peak at the center of the
	// scanline than for a wider beam.
	float4 weights = distance / 0.3;
	return 1.4 * exp(-pow(weights * rsqrt(0.5 * wid), wid)) / (0.6 + 0.2 * wid);
}

float4 PS_Product ( in VS_OUTPUT_PRODUCT input ) : COLOR
{
#ifdef CURVATURE
	float2 xy = radialDistortion(input.TexCoord, input.abspos);
#else
	float2 xy = input.TexCoord;
#endif

	if (xy.x < 0 || xy.y < 0 ||
		xy.x > 1 || xy.y > 1) return float4(0, 0, 0, 1);

	// Of all the pixels that are mapped onto the texel we are
	// currently rendering, which pixel are we currently rendering?
	float2 ratio_scale = xy * Viewport - 0.5;
	float2 uv_ratio = frac(ratio_scale);

	// Snap to the center of the underlying texel.
	xy = (floor(ratio_scale) + 0.5) / Viewport;

	// Calculate Lanczos scaling coefficients describing the effect
	// of various neighbour texels in a scanline on the current
	// pixel.
	float4 coeffs = PI * float4(1.0 + uv_ratio.x, uv_ratio.x, 1.0 - uv_ratio.x, 2.0 - uv_ratio.x);

	// Prevent division by zero.
	coeffs = FIX(coeffs);

	// Lanczos2 kernel.
	coeffs = 2.0 * sin(coeffs) * sin(coeffs / 2.0) / (coeffs * coeffs);

	// Normalize.
	coeffs /= dot(coeffs, 1.0);

	// Calculate the effective colour of the current and next
	// scanlines at the horizontal location of the current pixel,
	// using the Lanczos coefficients above.
	float4 col  = clamp(
			mul(coeffs, float4x4(
					TEX2D(xy + float2(-TexelSize.r, 0.0)),
					TEX2D(xy),
					TEX2D(xy + float2(TexelSize.x, 0.0)),
					TEX2D(xy + float2(2.0 * TexelSize.x, 0.0))
			)), 0.0, 1.0);
	float4 col2 = clamp(
			mul(coeffs, float4x4(
					TEX2D(xy + float2(-TexelSize.x, TexelSize.y)),
					TEX2D(xy + float2(0.0, TexelSize.y)),
					TEX2D(xy + TexelSize),
					TEX2D(xy + float2(2.0 * TexelSize.x, TexelSize.y))
			)), 0.0, 1.0);

#ifndef LINEAR_PROCESSING
	col  = pow(col , inputGamma);
	col2 = pow(col2, inputGamma);
#endif

	// Calculate the influence of the current and next scanlines on
	// the current pixel.

	// [dsl] This thing didn't work, so I tried something on my own...
	//float4 weights  = scanlineWeights(uv_ratio.y, col);
	//float4 weights2 = scanlineWeights(1.0 - uv_ratio.y, col2);
	//float3 mul_res  = (col * weights + col2 * weights2).rgb;
		
	float mix = sin(xy.y * Viewport.y * .6 * PI); // [dsl] That .6 is a magic number from me that makes it look nice
	mix = (mix + 1) / 2;
	pow(mix, 2);
	float3 mul_res = lerp(col2, col2 * .25, mix).rgb;


	// dot-mask emulation:
	// Output pixels are alternately tinted green and magenta.
	float3 dotMaskWeights = lerp(
			float3(1.0, 0.7, 1.0),
			float3(0.7, 1.0, 0.7),
			floor(input.abspos.x % 2.0)
		);

	mul_res *= dotMaskWeights;

	// Convert the image gamma for display on our output device.
	mul_res = pow(abs(mul_res), 1.0 / outputGamma);

	// Color the texel.
	return float4(mul_res, 1.0);
}



technique CRTFX
{
	pass P0
	{
		// shaders
		VertexShader = compile vs_3_0 VS_Product();
		PixelShader  = compile ps_3_0 PS_Product();
		AlphaBlendEnable = FALSE;
		ColorWriteEnable = RED|GREEN|BLUE|ALPHA;
	}
}