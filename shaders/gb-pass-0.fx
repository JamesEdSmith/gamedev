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

#define baseline_alpha 0.05 //the alpha value of dots in their "off" state, does not affect the border region of the screen - [0, 1]
#define response_time 0.444 //simulate response time, higher values result in longer color transition periods - [0, 1]

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//structs                                                                                                                                 //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


float2 video_size;
float2 texture_size;
float2 output_size;
float frame_count;
sampler2D text : register(s0) ;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//vertex definitions                                                                                                                      //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define video_scale 		floor(output_size.y / video_size.y)		//largest integer scale of input video that will fit in the current output (y axis would typically be limiting on widescreens)
#define scaled_video_out	(video_size * video_scale)				//size of the scaled video
#define half_pixel		(0.5 / output_size)					//it's... half a pixel

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//fragment definitions                                                                                                                    //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define foreground_color tex2D(COLOR_PALETTE, half2(0.75, 0.5)).rgb					//hardcoded to look up the foreground color from the right half of the palette image
#define rgb_to_alpha(rgb) ((rgb.r + rgb.g + rgb.b) / 2.6 + (is_on_dot * baseline_alpha) )		//averages rgb values (allows it to work with color games), modified for contrast and base alpha


//frame sampling definitions

#define curr_rgb  abs(1 - tex2D(text, texCoord).rgb)
#define prev0_rgb abs(1 - tex2D(PREV, texCoord).rgb)
#define prev1_rgb abs(1 - tex2D(PREV1, texCoord).rgb)
#define prev2_rgb abs(1 - tex2D(PREV2, texCoord).rgb)
#define prev3_rgb abs(1 - tex2D(PREV3, texCoord).rgb)
#define prev4_rgb abs(1 - tex2D(PREV4, texCoord).rgb)
#define prev5_rgb abs(1 - tex2D(PREV5, texCoord).rgb)
#define prev6_rgb abs(1 - tex2D(PREV6, texCoord).rgb)

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//fragment shader                                                                                                                         //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 main_fragment(float4 position : SV_Position, float4 col : COLOR0,
    float2 texCoord : TEXCOORD0,
    uniform sampler2D PREV : register(s1),
    uniform sampler2D PREV1 : register(s2),
    uniform sampler2D PREV2 : register(s3),
    uniform sampler2D PREV3 : register(s4),
    uniform sampler2D PREV4 : register(s5),
    uniform sampler2D PREV5 : register(s6),
    uniform sampler2D PREV6 : register(s7),
    uniform sampler2D COLOR_PALETTE : register(s8)) : COLOR0
{
    half2 dot_size = 1.0 / texture_size;
    half2 one_texel = 1.0 / (texture_size * video_scale);
    //determine if the corrent texel lies on a dot or in the space between dots

    int is_on_dot = all(int2(fmod(texCoord.x, dot_size.x) > one_texel.x, 		//returns 1 if fragment lies on a dot, 0 otherwise
                    fmod(texCoord.y, dot_size.y) > one_texel.y));
    

//sample color from the current and previous frames, apply response time modifier
//response time effect implmented through an exponential dropoff algorithm

  half3 input_rgb = curr_rgb;
  input_rgb += (prev0_rgb - input_rgb) * response_time;
  input_rgb += (prev1_rgb - input_rgb) * pow(response_time, 2.0);
  input_rgb += (prev2_rgb - input_rgb) * pow(response_time, 3.0);
  input_rgb += (prev3_rgb - input_rgb) * pow(response_time, 4.0);
  input_rgb += (prev4_rgb - input_rgb) * pow(response_time, 5.0);
  input_rgb += (prev5_rgb - input_rgb) * pow(response_time, 6.0);
  input_rgb += (prev6_rgb - input_rgb) * pow(response_time, 7.0);


  //apply foreground color and assign alpha value

    //half4 out_color = half4(input_rgb, rgb_to_alpha(input_rgb));	//apply the foreground color to all texels (the color will be modified by alpha later) and assign alpha based on rgb input
  half alpha =((input_rgb.r + input_rgb.g + input_rgb.b) / 2.6) + (is_on_dot * baseline_alpha) ;
  half4 out_color = half4(foreground_color, alpha);

  //overlay the matrix

    out_color.a *= is_on_dot;		//if the fragment is not on a dot, set its alpha value to 0

  //return fragment color
   return out_color;
   
}

//a simple blur technique that softens harsh color transitions
//specialized to only blur alpha values and limited to only blurring texels lying in the spaces between two or more texels
#define blending_modifier(color) saturate(half(color.a == 0) + blending_mode)
#define blending_mode 1.0				//0 - only the space between dots is blending, 1 - all texels are blended [DEFAULT: 0]
#define adjacent_texel_alpha_blending 0.38	//the amount of alpha swapped between neighboring texels [DEFAULT: 0.38]

half simple_blur( half4 COLOR, half2 tex_coord_1, half2 tex_coord_2, half2 tex_coord_3, half2 tex_coord_4, half2 lower_bound, half2 upper_bound)
{
    //clamp the blur coords to the input texture size so it doesn't attempt to sample off the texture (it'll retrieve float4(0.0) and darken the edges otherwise)

    tex_coord_1 = clamp(tex_coord_1, lower_bound, upper_bound);
    tex_coord_2 = clamp(tex_coord_2, lower_bound, upper_bound);
    tex_coord_3 = clamp(tex_coord_3, lower_bound, upper_bound);
    tex_coord_4 = clamp(tex_coord_4, lower_bound, upper_bound);


    //sample adjacent texels based on the coordinates above

    half4 adjacent_texel_1 = tex2D(text, tex_coord_1);
    half4 adjacent_texel_2 = tex2D(text, tex_coord_2);
    half4 adjacent_texel_3 = tex2D(text, tex_coord_3);
    half4 adjacent_texel_4 = tex2D(text, tex_coord_4);


    //sum the alpha differences between neighboring texels, apply modifiers, then subtract the result from the current fragment alpha value

    COLOR.a -= ((COLOR.a - adjacent_texel_1.a) +
        (COLOR.a - adjacent_texel_2.a) +
        (COLOR.a - adjacent_texel_3.a) +
        (COLOR.a - adjacent_texel_4.a)) * adjacent_texel_alpha_blending * blending_modifier(COLOR);


    //return new alpha value

    return COLOR.a;
}

float4 main_fragment1(float4 position : SV_Position, float4 col : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    //sample the input textures

      half4 out_color = tex2D(text, texCoord);

      half2 texel = 1.0 / texture_size;

      half2 tex_coord_1 = texCoord + half2(0.0, texel.y);	//down
      half2 tex_coord_2 = texCoord + half2(0.0, -texel.y);	//up
      half2 tex_coord_3 = texCoord + half2(texel.x, 0.0);	//right
      half2 tex_coord_4 = texCoord + half2(-texel.x, 0.0);	//left
      half2 lower_bound = half2(0.0, 0.0);				//lower texture bounds
      half2 upper_bound = texel * (output_size - 2.0);		//upper texture bounds


//apply the blur effect

  out_color.a = simple_blur(out_color, tex_coord_1, tex_coord_2, tex_coord_3, tex_coord_4, lower_bound, upper_bound);


  //return

    return out_color;
}

#define shadow_blur1 5.0	//blurriness of the shadow [0, 5]  [DEFAULT: 2.0]

half4 gaussian_blur(float2 tex_coord, float2 texel, float2 lower_bound, float2 upper_bound)
{
    //define offsets and weights - change this for both the X and Y passes if you change the sigma value or number of texels sampled

    //float offsets[5] = float[](0.0, 1.0, 2.0, 3.0, 4.0);
    //half weights[5] = fixed[](0.13465834124289953661305802732548,  		//precalculated using the Gaussian function:
    //    0.13051534237555914090930704141833,  		//  G(x) = (1 / sqrt(2 * pi * sigma^2)) * e^(-x^2 / (2 * sigma^2))
    //    0.11883557904592230273554609080014,  		//where sigma = 4.0 and x = offset in range [0, 5]
    //    0.10164546793794160274995705611009,  		//normalized to 1 to prevent image darkening by multiplying each weight by:
    //    0.08167444001912718529866079800870); 		//  1 / sum(all weights)


//sample the current fragment and apply its weight

    half4 out_color = tex2D(text, clamp(tex_coord, lower_bound, upper_bound)) * 0.13;


    //iterate across the offsets in both directions sampling texels and adding their weighted alpha values to the total

 
        out_color.a += tex2D(text, clamp(tex_coord + half2(0.0 * texel.x, 0.0), lower_bound, upper_bound)).a * 0.13;
        out_color.a += tex2D(text, clamp(tex_coord - half2(0.0 * texel.x, 0.0), lower_bound, upper_bound)).a * 0.13;

        out_color.a += tex2D(text, clamp(tex_coord + half2(1.0 * texel.x, 0.0), lower_bound, upper_bound)).a * 0.13 - (shadow_blur1 / 100);
        out_color.a += tex2D(text, clamp(tex_coord - half2(1.0 * texel.x, 0.0), lower_bound, upper_bound)).a * 0.13 - (shadow_blur1 / 100);

        out_color.a += tex2D(text, clamp(tex_coord + half2(2.0 * texel.x, 0.0), lower_bound, upper_bound)).a * 0.13 - (3 * shadow_blur1 / 100);
        out_color.a += tex2D(text, clamp(tex_coord - half2(2.0 * texel.x, 0.0), lower_bound, upper_bound)).a * 0.13 - (3 * shadow_blur1 / 100);

        out_color.a += tex2D(text, clamp(tex_coord + half2(3.0 * texel.x, 0.0), lower_bound, upper_bound)).a * 0.13 - (5 * shadow_blur1 / 100) ;
        out_color.a += tex2D(text, clamp(tex_coord - half2(3.0 * texel.x, 0.0), lower_bound, upper_bound)).a * 0.13 - (5 * shadow_blur1 / 100) ;

    //return the new value

    return out_color;
}


float4 main_fragment2(float4 position : SV_Position, float4 col : COLOR0,
    float2 texCoord : TEXCOORD0) : COLOR0
{
    //apply the Gaussian blur along the x axis and return the result

    float2 texel = 1.0 / texture_size;

      return gaussian_blur(texCoord,
               texel,
               float2(0.0,0.0),
               float2(texel * (output_size - 1.0)));
}

half4 gaussian_blur2(float2 tex_coord, float2 texel, float2 lower_bound, float2 upper_bound)
{
    //define offsets and weights - change this for both the X and Y passes if you change the sigma value or number of texels sampled

    //float offsets[5] = float[](0.0, 1.0, 2.0, 3.0, 4.0);
    //half weights[5] = fixed[](0.13465834124289953661305802732548,     //precalculated using the Gaussian function:
    //    0.13051534237555914090930704141833,     //  G(x) = (1 / sqrt(2 * pi * sigma^2)) * e^(-x^2 / (2 * sigma^2))
    //    0.11883557904592230273554609080014,     //where sigma = 4.0 and x = offset in range [0, 5]
    //    0.10164546793794160274995705611009,     //normalized to 1 to prevent image darkening by multiplying each weight by:
    //    0.08167444001912718529866079800870);    //  1 / sum(all weights)


//sample the current fragment and apply its weight

    half4 out_color = tex2D(text, clamp(tex_coord, lower_bound, upper_bound)) * 0.13;


    //iterate across the offsets in both directions sampling texels and adding their weighted alpha values to the total

 
        out_color.a += tex2D(text, clamp(tex_coord + half2( 0.0, 0.0 * texel.x), lower_bound, upper_bound)).a * 0.13;
        out_color.a += tex2D(text, clamp(tex_coord - half2( 0.0, 0.0 * texel.x), lower_bound, upper_bound)).a * 0.13;

        out_color.a += tex2D(text, clamp(tex_coord + half2(0.0, 1.0 * texel.x), lower_bound, upper_bound)).a * 0.13 - (shadow_blur1 / 100);
        out_color.a += tex2D(text, clamp(tex_coord - half2(0.0, 1.0 * texel.x), lower_bound, upper_bound)).a * 0.13 - (shadow_blur1 / 100);

        out_color.a += tex2D(text, clamp(tex_coord + half2(0.0, 2.0 * texel.x), lower_bound, upper_bound)).a * 0.13 - (3 * shadow_blur1 / 100);
        out_color.a += tex2D(text, clamp(tex_coord - half2(0.0, 2.0 * texel.x), lower_bound, upper_bound)).a * 0.13 - (3 * shadow_blur1 / 100);

        out_color.a += tex2D(text, clamp(tex_coord + half2(0.0, 3.0 * texel.x), lower_bound, upper_bound)).a * 0.13 - (5 * shadow_blur1 / 100) ;
        out_color.a += tex2D(text, clamp(tex_coord - half2(0.0, 3.0 * texel.x), lower_bound, upper_bound)).a * 0.13 - (5 * shadow_blur1 / 100) ;

    //return the new value

    return out_color;
}

float4 main_fragment3(float4 position : SV_Position, float4 col : COLOR0,
    float2 texCoord : TEXCOORD0) : COLOR0
{
    //apply the Gaussian blur along the x axis and return the result

    float2 texel = 1.0 / texture_size;

      return gaussian_blur2(texCoord,
               texel,
               float2(0.0,0.0),
               float2(texel * (output_size - 1.0)));
}

#define contrast 1.00   	//useful to fine-tune the colors. higher values make the "black" color closer to black - [0, 1] [DEFAULT: 0.95]
#define screen_light 1.00   //controls the ambient light of the screen. lower values darken the screen - [0, 2] [DEFAULT: 1.00]
#define pixel_opacity 0.80	//controls the opacity of the dot-matrix pixels. lower values make pixels more transparent - [0, 1] [DEFAULT: 1.00]
#define bg_smoothing 1.0	//higher values suppress changes in background color directly beneath the foreground to improve image clarity - [0, 1] [DEFAULT: 0.75]
#define shadow_opacity 1.0	//how strongly shadows affect the background, higher values darken the shadows - [0, 1] [DEFAULT: 0.55]
#define shadow_offset_x -2.0	//how far the shadow should be shifted to the right in pixels - [-infinity, infinity] [DEFAULT: 1.0]
#define shadow_offset_y -2.0	//how far the shadow should be shifted to down in pixels - [-infinity, infinity] [DEFAULT: 1.5]
#define screen_offset_x 0	//screen offset - [-infinity, infinity] [DEFAULT: 0]
#define screen_offset_y 0	//screen offset - [-infinity, infinity] [DEFAULT: 0]

#define bg_color tex2D(COLOR_PALETTE, half2(0.25, 0.5))				//sample the background color from the palette
#define shadow_alpha (contrast * shadow_opacity)					//blending factor used when overlaying shadows on the background
#define shadow_offset float2(shadow_offset_x * texel.x, shadow_offset_y * texel.y)	//offset for the shadow
#define screen_offset float2(screen_offset_x * texel.x, screen_offset_y * texel.y)	//offset for the entire screen

float4 main_fragment4(float4 position : SV_Position, float4 col : COLOR0,
    float2 texCoord : TEXCOORD0,
    uniform sampler2D PASS2,
    uniform sampler2D COLOR_PALETTE : register(s2),
    uniform sampler2D BACKGROUND : register(s3)) : COLOR0
{
    //sample all the relevant textures	

    float2 texel = 1.0 / texture_size;

      half4 foreground = tex2D(PASS2, texCoord - screen_offset);
      half4 background = tex2D(BACKGROUND, texCoord);
      half4 shadows = tex2D(text, texCoord - (shadow_offset + screen_offset));
      half4 background_color = bg_color;


      //foreground and background are blended with the background color

        foreground *= (1);
        background -= (background -0.5) * bg_smoothing * half(foreground.a > 0.0);	//suppress drastic background color changes under the foreground to improve clarity

        background.rgb = saturate(half3( 				//allows for highlights, background = bg_color when the background color is 0.5 gray
        bg_color.r + lerp(-1.0, 1.0, background.r),
        bg_color.g + lerp(-1.0, 1.0, background.g),
        bg_color.b + lerp(-1.0, 1.0, background.b)));

        //shadows are alpha blended with the background

          half4 out_color = (shadows * shadows.a * shadow_alpha) + (background * (1 - shadows.a * shadow_alpha));


          //foreground is alpha blended with the shadowed background

            out_color = (foreground * foreground.a * (1.0 - foreground.a * foreground.a * contrast)) + (out_color * (screen_light - foreground.a * contrast * pixel_opacity));


            //return fragment

              return out_color;
}

technique gameboy
{
	pass P0
	{
        PixelShader =  compile ps_5_0 main_fragment();
	}
}

technique gameboy1
{
    pass P0
    {
        PixelShader = compile ps_5_0 main_fragment1();
    }
}

technique gameboy2
{
    pass P0
    {
        PixelShader = compile ps_5_0 main_fragment2();
    }
}

technique gameboy3
{
    pass P0
    {
        PixelShader = compile ps_5_0 main_fragment3();
    }
}

technique gameboy4
{
    pass P0
    {
        PixelShader = compile ps_5_0 main_fragment4();
    }
}