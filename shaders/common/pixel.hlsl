#ifndef COMMON_PIXEL_H
#define COMMON_PIXEL_H

#include "common/pixel.config.hlsl"
#include "common/pixel.material.hlsl"

#include "sbox_pixel.fxc"

//-----------------------------------------------------------------------------
//
// Easily extract the Material structure from the textures set by the user
//
//-----------------------------------------------------------------------------
Material GatherMaterial( const PS_INPUT i )
{
    float2 vUV = i.vTextureCoords.xy;
    Material material = ToMaterial( Tex2DS( g_tColor, TextureFiltering, vUV  ), 
                                    Tex2DS( g_tNormal, TextureFiltering, vUV  ), 
                                    Tex2DS( g_tRma, TextureFiltering, vUV  ), 
                                    g_flTintColor  );
    return material;
}


//-----------------------------------------------------------------------------
//
// Compose the final color with lighting from material parameters
//
//-----------------------------------------------------------------------------

PixelOutput FinalizePixelMaterial( PixelInput i, Material m )
{
    CombinerInput o = MaterialToCombinerInput( i, m );
    return FinalizePixel( o );
}


#endif // COMMON_PIXEL_H