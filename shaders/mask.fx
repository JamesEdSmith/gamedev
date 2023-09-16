#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

sampler2D text : register(s0) ;

struct PixelInput {
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float4 TexCoord : TEXCOORD0;
};

float4 SpritePixelShader(PixelInput p, uniform sampler2D Mask): COLOR0 {
    
    float4 t = tex2D(text, p.TexCoord.xy);
    float4 m = tex2D(Mask, p.TexCoord.xy);

    
    return float4(t.r, t.g, t.b, t.a * m.a);
    
}

technique SpriteBatch {
    pass P0{
        PixelShader = compile PS_SHADERMODEL SpritePixelShader();
    }
}