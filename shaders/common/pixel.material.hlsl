#ifndef COMMON_PIXEL_MATERIAL_H
#define COMMON_PIXEL_MATERIAL_H

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
struct Material
{
    float3  Albedo;
    float3  Normal;
    float   Roughness;
    float   Metalness;
    float   AmbientOcclusion;
    float   BlendMask;
    float   TintMask;
    float   Opacity;
};

//-----------------------------------------------------------------------------
//
// Transform texture input to the Material structure
//
//-----------------------------------------------------------------------------
Material ToMaterial( float4 vColor, float4 vNormal, float4 vRMA, float3 vTintColor = float3( 1.0f, 1.0f, 1.0f ) )
{
    Material p;
    p.Albedo = vColor.rgb;
    p.Normal = vNormal.rgb;
    p.Roughness = vRMA.r;
    p.Metalness = vRMA.g;
    p.AmbientOcclusion = vRMA.b;
    p.BlendMask = vRMA.a;
    p.TintMask = ( S_ALPHA_TEST || S_TRANSLUCENT ) ? 0.0f : vColor.a;
    p.Opacity = ( S_ALPHA_TEST || S_TRANSLUCENT ) ? vColor.a : 0.0f;
    
    // Do tint
    p.Albedo = lerp( p.Albedo.rgb, p.Albedo.rgb * vTintColor, p.TintMask );
    
    return p;
}

//-----------------------------------------------------------------------------
//
// Lerp function for Material
//
//-----------------------------------------------------------------------------
Material lerp( Material a, Material b, float amount )
{
    Material o;
    o.Albedo =           lerp( a.Albedo, b.Albedo, amount );
    o.Normal =           lerp( a.Normal, b.Normal, amount );
    o.Roughness =        lerp( a.Roughness, b.Roughness, amount );
    o.Metalness =        lerp( a.Metalness, b.Metalness, amount );
    o.AmbientOcclusion = lerp( a.AmbientOcclusion, b.AmbientOcclusion, amount );
    o.BlendMask =        lerp( a.BlendMask, b.BlendMask, amount );
    o.TintMask  =        lerp( a.TintMask, b.TintMask, amount );
    return o;
}


//-----------------------------------------------------------------------------
//
// Material
//
//-----------------------------------------------------------------------------
CreateInputTexture2D( TextureColor,            Srgb,   8, "",                 "_color",  "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
CreateInputTexture2D( TextureNormal,           Linear, 8, "NormalizeNormals", "_normal", "Material,10/20", Default3( 0.5, 0.5, 1.0 ) );
CreateInputTexture2D( TextureRoughness,        Linear, 8, "",                 "_rough",  "Material,10/30", Default( 0.5 ) );
CreateInputTexture2D( TextureMetalness,        Linear, 8, "",                 "_metal",  "Material,10/40", Default( 1.0 ) );
CreateInputTexture2D( TextureAmbientOcclusion, Linear, 8, "",                 "_ao",     "Material,10/50", Default( 1.0 ) );
CreateInputTexture2D( TextureBlendMask,        Linear, 8, "",                 "_blend",  "Material,10/60", Default( 1.0 ) );

// We internally encode the opacity and blend mask differently
#if ( S_ALPHA_TEST || S_TRANSLUCENT )
    CreateInputTexture2D( TextureTranslucency, Linear, 8, "",                 "_trans",  "Material,10/70", Default3( 1.0, 1.0, 1.0 ) );
    #define COLOR_TEXTURE_CHANNELS Channel( RGB, AlphaWeighted( TextureColor, TextureTranslucency ), Srgb ); Channel( A, Box( TextureTranslucency ), Linear )
#else
    CreateInputTexture2D( TextureTintMask,     Linear, 8, "",                 "_tint",   "Material,10/70", Default( 1.0 ) );
    #define COLOR_TEXTURE_CHANNELS Channel( RGB,  Box( TextureColor ), Srgb ); Channel( A, Box( TextureTintMask ), Linear )
#endif

float3 g_flTintColor < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Material,10/80" ); >;

CreateTexture2DWithoutSampler( g_tColor )  < COLOR_TEXTURE_CHANNELS; OutputFormat( BC7 ); SrgbRead( true ); >;
CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGBA, Box( TextureNormal ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
CreateTexture2DWithoutSampler( g_tRma )    < Channel( R,    Box( TextureRoughness ), Linear ); Channel( G, Box( TextureMetalness ), Linear ); Channel( B, Box( TextureAmbientOcclusion ), Linear );  Channel( A, Box( TextureBlendMask ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

#endif //COMMON_PIXEL_MATERIAL_H