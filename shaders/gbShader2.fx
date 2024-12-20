///////////////////////////////////////////////////////////////////////////
//                                                                       //
// Gameboy Classic Shader v0.2.2                                         //
//                                                                       //
// Copyright (C) 2013 Harlequin : unknown92835@gmail.com                 //
//                                                                       //
// This program is free software: you can redistribute it and/or modify  //
// it under the terms of the GNU General Public License as published by  //
// the Free Software Foundation, either version 3 of the License, or     //
// (at your option) any later version.                                   //
//                                                                       //
// This program is distributed in the hope that it will be useful,       //
// but WITHOUT ANY WARRANTY; without even the implied warranty of        //
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         //
// GNU General Public License for more details.                          //
//                                                                       //
// You should have received a copy of the GNU General Public License     //
// along with this program.  If not, see <http://www.gnu.org/licenses/>. //
//                                                                       //
///////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//config                                                                                                                                  //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define baseline_alpha 0.10 //the alpha value of dots in their "off" state, does not affect the border region of the screen - [0, 1]
#define response_time 0.333 //simulate response time, higher values result in longer color transition periods - [0, 1]

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//structs                                                                                                                                 //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct input
{
    float2 video_size;
    float2 texture_size;
    float2 output_size;
    float frame_count;
    sampler2D text   : register(t0);
};

input IN;

struct prev_0
{
    sampler2D text : register(t1);
};

struct prev_1
{
    sampler2D text : register(t2);
};

struct prev_2
{
    sampler2D text : register(t3);
};

struct prev_3
{
    sampler2D text : register(t4);
};

struct prev_4
{
    sampler2D text : register(t5);
};

struct prev_5
{
    sampler2D text : register(t6);
};

struct prev_6
{
    sampler2D text : register(t7);
};

struct matrix_data
{
    fixed2 dot_size : TEXCOORD1;
    fixed2 one_texel : TEXCOORD2;
};*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//vertex definitions                                                                                                                      //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define video_scale         floor(IN.output_size.y / IN.video_size.y)       //largest integer scale of input video that will fit in the current output (y axis would typically be limiting on widescreens)
#define scaled_video_out    (IN.video_size * video_scale)               //size of the scaled video
#define half_pixel      (0.5 / IN.output_size)                  //it's... half a pixel

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//vertex shader                                                                                                                           //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

void main_vertex(float4 position      			: POSITION,
    out float4 oPosition : POSITION,
    float2 texCoord : TEXCOORD0,
    out float2 oTexCoord : TEXCOORD0,
    out matrix_data oMatrixData,
    uniform float4x4 modelViewProj,
    uniform input IN)
{
    oPosition = mul(modelViewProj, position) / float4(float2(IN.output_size / scaled_video_out), 1.0, 1.0);	//remaps position to integer scaled output
    oTexCoord = texCoord + half_pixel;										//half pixel offset seems necessary here

    oMatrixData = matrix_data(
        1.0 / IN.texture_size,				//should always be square, but why the hell not
        1.0 / (IN.texture_size * video_scale)		//one texel on the hypothetical scaled input texture
    );
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//fragment definitions                                                                                                                    //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define foreground_color tex2D(COLOR_PALETTE, fixed2(0.75, 0.5)).rgb                    //hardcoded to look up the foreground color from the right half of the palette image
#define rgb_to_alpha(rgb) ( ((rgb.r + rgb.g + rgb.b) / 3.0) + (is_on_dot * baseline_alpha) )        //averages rgb values (allows it to work with color games), modified for contrast and base alpha


//frame sampling definitions

#define curr_rgb  abs(1 - tex2D(IN.text, texCoord).rgb)
#define prev0_rgb abs(1 - tex2D(PREV.text, texCoord).rgb)
#define prev1_rgb abs(1 - tex2D(PREV1.text, texCoord).rgb)
#define prev2_rgb abs(1 - tex2D(PREV2.text, texCoord).rgb)
#define prev3_rgb abs(1 - tex2D(PREV3.text, texCoord).rgb)
#define prev4_rgb abs(1 - tex2D(PREV4.text, texCoord).rgb)
#define prev5_rgb abs(1 - tex2D(PREV5.text, texCoord).rgb)
#define prev6_rgb abs(1 - tex2D(PREV6.text, texCoord).rgb)

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//fragment shader                                                                                                                         //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 main_fragment( float2 texCoord               : TEXCOORD0, 
    in matrix_data matrixData,
              uniform prev_0 PREV, 
              uniform prev_1 PREV1, 
              uniform prev_2 PREV2, 
              uniform prev_3 PREV3, 
              uniform prev_4 PREV4, 
              uniform prev_5 PREV5, 
              uniform prev_6 PREV6, 
              uniform sampler2D COLOR_PALETTE   : register(t8) ) : COLOR0
{
  //determine if the corrent texel lies on a dot or in the space between dots

    int is_on_dot = all( int2( mod(texCoord.x, matrixData.dot_size.x) > matrixData.one_texel.x,         //returns 1 if fragment lies on a dot, 0 otherwise
                   mod(texCoord.y, matrixData.dot_size.y) > matrixData.one_texel.y ) );


  //sample color from the current and previous frames, apply response time modifier
  //response time effect implmented through an exponential dropoff algorithm

    fixed3 input_rgb = curr_rgb;
    input_rgb += (prev0_rgb - input_rgb) * response_time;
    input_rgb += (prev1_rgb - input_rgb) * pow(response_time, 2.0);
    input_rgb += (prev2_rgb - input_rgb) * pow(response_time, 3.0);
    input_rgb += (prev3_rgb - input_rgb) * pow(response_time, 4.0);
    input_rgb += (prev4_rgb - input_rgb) * pow(response_time, 5.0);
    input_rgb += (prev5_rgb - input_rgb) * pow(response_time, 6.0);
    input_rgb += (prev6_rgb - input_rgb) * pow(response_time, 7.0);


  //apply foreground color and assign alpha value

    fixed4 out_color = fixed4(foreground_color, rgb_to_alpha(input_rgb));   //apply the foreground color to all texels (the color will be modified by alpha later) and assign alpha based on rgb input


  //overlay the matrix

    out_color.a *= is_on_dot;       //if the fragment is not on a dot, set its alpha value to 0


  //return fragment color

    return out_color;
}

technique gameboy
{
	pass P0
	{
		// shaders
		VertexShader = compile vs_5_0 main_vertex();
        PixelShader =  compile ps_5_0 main_fragment();
	}
}