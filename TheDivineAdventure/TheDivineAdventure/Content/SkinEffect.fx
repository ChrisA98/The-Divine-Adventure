#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

// M A X   B O N E S 
#define MAX_BONES   180
#define MAX_LIGHTS  21

// TEXTURES
sampler texSampler : register(s0)
{
    Texture = <TexDiffuse>;
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};
sampler normalSampler : register(s1)
{
    Texture = <TexNormalMap>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};
sampler specSampler : register(s2)
{
    Texture = <TexSpecular>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4x4 World, WorldViewProj;
float3x3 WorldInverseTranspose;
float3   CamPos;

// MATERIAL & LIGHTING
float4   DiffuseColor;
float3   EmissiveColor; // Ambient factored into emissive already
float3   SpecularColor;
float    SpecularPower;
float3x3 LightDir_0;
float3x3 LightDiffCol_0;
float3x3 LightSpecCol_0;
float3x3 LightPos_1;
float3 LightPow_1;
float3x3 LightDiffCol_1;
float3x3 LightSpecCol_1;
float3x3 LightPos_2;
float3 LightPow_2;
float3x3 LightDiffCol_2;
float3x3 LightSpecCol_2;
float3x3 LightPos_3;
float3 LightPow_3;
float3x3 LightDiffCol_3;
float3x3 LightSpecCol_3;
float3x3 LightPos_4;
float3 LightPow_4;
float3x3 LightDiffCol_4;
float3x3 LightSpecCol_4;
float3x3 LightPos_5;
float3 LightPow_5;
float3x3 LightDiffCol_5;
float3x3 LightSpecCol_5;
float3x3 LightPos_6;
float3 LightPow_6;
float3x3 LightDiffCol_6;
float3x3 LightSpecCol_6;

int LightCount;


float3 FogColor;
float4 FogVector;

matrix Bones[MAX_BONES];

// INPUTS 
struct VS_In //Vertex Shade Input
{
    float4 pos          : POSITION0;
    float2 uv           : TEXCOORD0;
    float3 normal       : NORMAL0;
    float3 tangent      : TANGENT0;
    float3 bitangent    : BINORMAL0;
    uint4  indices      : BLENDINDICES0;
    float4 weights      : BLENDWEIGHT0;
};

// TRANSFER FROM VERTEX-SHADER TO PIXEL-SHADER 
// 
// NORMAL_MAPPED__VS__OUTPUT
struct VS_N_Out                       // normal will be extracted from normalMap during pixel-shading, so no need to pass it
{
    float4 position   : SV_POSITION;
    float2 uv         : TEXCOORD0;
    float4 worldPos   : TEXCOORD1;    // position in world space (we'll make the w component into the fog-factor)    
    float3x3 tanSpace : TEXCOORD2;
};
// REGULAR__VS__OUTPUT
struct VS_Out
{
    float4 position : SV_POSITION;
    float2 uv       : TEXCOORD0;
    float4 worldPos : TEXCOORD1;      // position in world space (we'll make the w component into the fog-factor)    
    float3 normal   : TEXCOORD2;
};




// VERTEX SHADER TECHNIQUES----------------------------------------------------------------------------

// SKIN  -  calculates the position and normal from weighted bone matrices
void Skin(inout VS_In vin) {
    float4x3 skinning = 0;
    [unroll]
    for (int i = 0; i < 4; i++) { skinning += Bones[vin.indices[i]] * vin.weights[i]; }  // looks up bone, uses bone matrix by some percentage (4 bones should add to 100%)
    vin.pos.xyz = mul(vin.pos, skinning);
    vin.normal = mul(vin.normal, (float3x3) skinning);
}

void SkinNorms(inout VS_In vin) {
    float4x3 skinning = 0;
    [unroll]
    for (int i = 0; i < 4; i++) { skinning += Bones[vin.indices[i]] * vin.weights[i]; }  // looks up bone, uses bone matrix by some percentage (4 bones should add to 100%)
    vin.pos.xyz = mul(vin.pos, skinning);
    vin.normal = mul(vin.normal, (float3x3) skinning);
    vin.tangent = mul(vin.tangent, (float3x3) skinning);
    vin.bitangent    = mul(vin.bitangent,(float3x3) skinning);
}



// VERTEX SHADER 3LIGHTS SKIN ( & FOG )
VS_Out VertexShader_3Lights_Skin(VS_In vin)
{
    VS_Out vout;
    Skin(vin);

    // use dot-product of pos with FogVector to determine fog intensity (as done in Monogame's SkinnedEffect)
    float fogFactor = saturate(dot(vin.pos, FogVector)); // get intensity (clamp: 0-1) 

    float3 wpos = mul(vin.pos, World);             // transform model to where it should be in world coordinates
    vout.normal = normalize(mul(vin.normal, WorldInverseTranspose)); // (using World-InverseTrans to prevent scaling & deformation issues for normals for skin animation) 
    vout.position = mul(vin.pos, WorldViewProj);   // used by the shader-pipeline (to get to screen location) 
    vout.worldPos = float4(wpos, fogFactor);       // send/interpolate fog factor in worldPos.w (piggyback) 
    vout.uv = vin.uv;
    return vout;
}



// VERTEX SHADER 3LIGHTS SKIN_NORMALMAPPED ( & FOG )
// Technique made for 3 directional lights, normal-mapping, and skin
// Could interpolate 3 direction intensities based on relative distances to in-game lights (good enough for the look I'm going for - weaken other 2 lights for subtle cues when only 1 light in scene) 
// May want to consider 1 sun source and 1 dominant point-source (interpolated pos) to draw dynamic character shadows from (depthstencil shadows)
// or could also get into deferred lighting(great for many lights); or even get into PBR and/or ray-casting all the lighting(no need to shadow-map) if you're really feeling brave ^-^
VS_N_Out VertexShader_3Lights_Skin_Nmap(VS_In vin)
{
    VS_N_Out vout;
    SkinNorms(vin);

    vout.tanSpace[0] = normalize(mul(vin.tangent, WorldInverseTranspose));
    float3 n = normalize(mul(vin.normal, WorldInverseTranspose)).xyz;
    float3 t = vout.tanSpace[0];
    t = normalize(t - dot(t, n) * n);  //orthogonalizes to improve quality	
    float3 b = normalize(cross(t, n));
    vout.tanSpace[1] = b;
    vout.tanSpace[2] = n;

    // use dot-product of pos with FogVector to determine fog intensity (as done in XNA/Monogame's SkinnedEffect)
    float fogFactor = saturate(dot(vin.pos, FogVector)); // get intensity (clamp: 0-1) 

    float3 wpos = mul(vin.pos, World);                 // transform model to where it should be in world coordinates
    vout.position = mul(vin.pos, WorldViewProj);       // used by the shader-pipeline (to get to screen location) 
    vout.worldPos = float4(wpos, fogFactor);           // send/interpolate fog factor in worldPos.w (piggyback) 
    vout.uv = vin.uv;
    return vout;
}





// PIXEL SHADER TECHNIQUES------------------------------------------------------------------------------------------------------------------------------------------
struct ColorPair
{
    float3 diffuse;
    float3 specular;
};
//COMPUTE LIGHTS
ColorPair ComputeLights0(float3 eyeVector, float3 normal, int count)
{
    float3x3 lightDirections = 0, lightDiffuse = 0, lightSpecular = 0;
    float3x3 halfVectors = 0;
    if (count > 3) count = 3;
    [unroll]
    for (int i = 0; i < 3; i++)
    {
        lightDirections[i] = float3x3(    LightDir_0[0],     LightDir_0[1],     LightDir_0[2])[i];
        lightDiffuse[i]    = float3x3(LightDiffCol_0[0], LightDiffCol_0[1], LightDiffCol_0[2])[i];
        lightSpecular[i]   = float3x3(LightSpecCol_0[0], LightSpecCol_0[1], LightSpecCol_0[2])[i];
        halfVectors[i]     = normalize(eyeVector - lightDirections[i]);
    }
    float3 dotL = mul(-lightDirections, normal); // angle between light and surface (moreless)
    float3 dotH = mul(halfVectors, normal);
    float3 zeroL = step(float3(0, 0, 0), dotL);   // clamp    
    float3 diffuse = zeroL * dotL;
    float3 specular = pow(max(dotH, 0) * zeroL, SpecularPower);
    ColorPair result;
    result.diffuse = mul(diffuse, lightDiffuse) * DiffuseColor.rgb + EmissiveColor; // diffuse-factor * texture color (+emissive color)
    result.specular = mul(specular, lightSpecular) * SpecularColor; // specular intensity * spec color
    return result;
}

ColorPair ComputeLights1(float3 eyeVector, float3 normal, int count, ColorPair base, VS_Out pin)
{
    float3x3 lightDirections = 0, lightDiffuse = 0, lightSpecular = 0;
    float3x3 halfVectors = 0;
    float3   strengths = 0, distances = 0;
    [unroll]
    for (int i = 0; i < count; i++)
    {
        distances[i]       = length(float3x3(LightPos_1[0], LightPos_1[1], LightPos_1[2])[i] - pin.worldPos.xyz);
        strengths[i]       = (1 / (distances[i] * distances[i])) * LightPow_1[i];
        lightDirections[i] = float3x3(    LightPos_1[0],     LightPos_1[1],     LightPos_1[2])[i] - pin.worldPos.xyz;
        lightDiffuse[i]    = float3x3(LightDiffCol_1[0], LightDiffCol_1[1], LightDiffCol_1[2])[i] * strengths[i];
        lightSpecular[i]   = float3x3(LightSpecCol_1[0], LightSpecCol_1[1], LightSpecCol_1[2])[i] * strengths[i];
        halfVectors[i]     = normalize(eyeVector - lightDirections[i]);
    }

    float3 dotL     = mul(lightDirections, normal); // angle between light and surface (moreless)
    float3 dotH     = mul(halfVectors, normal) * LightPow_1[i];
    float3 zeroL    = step(float3(0, 0, 0), dotL);   // clamp    
    float3 specular = pow(max(dotH, 0) * zeroL, SpecularPower);

    float3 diffuse = zeroL * dotL;

    ColorPair result = base;
    result.diffuse  += mul(diffuse, lightDiffuse) * DiffuseColor.rgb + EmissiveColor; // diffuse-factor * texture color (+emissive color)
    result.specular += mul(specular, lightSpecular) * SpecularColor; // specular intensity * spec color
    return result;
}

ColorPair ComputeLights2(float3 eyeVector, float3 normal, int count, ColorPair base, VS_Out pin)
{
    float3x3 lightDirections = 0, lightDiffuse = 0, lightSpecular = 0;
    float3x3 halfVectors = 0;
    float3   strengths = 0, distances = 0;
    [unroll]
    for (int i = 0; i < count; i++)
    {
        distances[i]       = length(float3x3(LightPos_2[0], LightPos_2[1], LightPos_2[2])[i] - pin.worldPos.xyz);
        strengths[i]       = (1 / (distances[i] * distances[i])) * LightPow_2[i];
        lightDirections[i] = float3x3(    LightPos_2[0],     LightPos_2[1],     LightPos_2[2])[i] - pin.worldPos.xyz;
        lightDiffuse[i]    = float3x3(LightDiffCol_2[0], LightDiffCol_2[1], LightDiffCol_2[2])[i] * strengths[i];
        lightSpecular[i]   = float3x3(LightSpecCol_2[0], LightSpecCol_2[1], LightSpecCol_2[2])[i] * strengths[i];
        halfVectors[i]     = normalize(eyeVector - lightDirections[i]);
    }

    float3 dotL     = mul(lightDirections, normal); // angle between light and surface (moreless)
    float3 dotH     = mul(halfVectors, normal) * LightPow_2[i];
    float3 zeroL    = step(float3(0, 0, 0), dotL);   // clamp    
    float3 specular = pow(max(dotH, 0) * zeroL, SpecularPower);

    float3 diffuse = zeroL * dotL;

    ColorPair result = base;
    result.diffuse  += mul(diffuse, lightDiffuse) * DiffuseColor.rgb + EmissiveColor; // diffuse-factor * texture color (+emissive color)
    result.specular += mul(specular, lightSpecular) * SpecularColor; // specular intensity * spec color
    return result;
}

ColorPair ComputeLights3(float3 eyeVector, float3 normal, int count, ColorPair base, VS_Out pin)
{
    float3x3 lightDirections = 0, lightDiffuse = 0, lightSpecular = 0;
    float3x3 halfVectors = 0;
    float3   strengths = 0, distances = 0;
    [unroll]
    for (int i = 0; i < count; i++)
    {
        distances[i]       = length(float3x3(LightPos_3[0], LightPos_3[1], LightPos_3[2])[i] - pin.worldPos.xyz);
        strengths[i]       = (1 / (distances[i] * distances[i])) * LightPow_3[i];
        lightDirections[i]  = float3x3(    LightPos_3[0],     LightPos_3[1],     LightPos_3[2])[i] - pin.worldPos.xyz;
        lightDiffuse[i]    =  float3x3(LightDiffCol_3[0], LightDiffCol_3[1], LightDiffCol_3[2])[i] * strengths[i];
        lightSpecular[i]   =  float3x3(LightSpecCol_3[0], LightSpecCol_3[1], LightSpecCol_3[2])[i] * strengths[i];
        halfVectors[i]     = normalize(eyeVector - lightDirections[i]);
    }

    float3 dotL     = mul(lightDirections, normal); // angle between light and surface (moreless)
    float3 dotH     = mul(halfVectors, normal) * LightPow_3[i];
    float3 zeroL    = step(float3(0, 0, 0), dotL);   // clamp    
    float3 specular = pow(max(dotH, 0) * zeroL, SpecularPower);

    float3 diffuse = zeroL * dotL;

    ColorPair result = base;
    result.diffuse  += mul(diffuse, lightDiffuse) * DiffuseColor.rgb + EmissiveColor; // diffuse-factor * texture color (+emissive color)
    result.specular += mul(specular, lightSpecular) * SpecularColor; // specular intensity * spec color
    return result;
}

ColorPair ComputeLights4(float3 eyeVector, float3 normal, int count, ColorPair base, VS_Out pin)
{
    float3x3 lightDirections = 0, lightDiffuse = 0, lightSpecular = 0;
    float3x3 halfVectors = 0;
    float3   strengths = 0, distances = 0;
    [unroll]
    for (int i = 0; i < count; i++)
    {
        distances[i]       = length(float3x3(LightPos_4[0], LightPos_4[1], LightPos_4[2])[i] - pin.worldPos.xyz);
        strengths[i]       = (1 / (distances[i] * distances[i])) * LightPow_4[i];
        lightDirections[i]  = float3x3(    LightPos_4[0],     LightPos_4[1],     LightPos_4[2])[i] - pin.worldPos.xyz;
        lightDiffuse[i]    =  float3x3(LightDiffCol_4[0], LightDiffCol_4[1], LightDiffCol_4[2])[i] * strengths[i];
        lightSpecular[i]   =  float3x3(LightSpecCol_4[0], LightSpecCol_4[1], LightSpecCol_4[2])[i] * strengths[i];
        halfVectors[i]     = normalize(eyeVector - lightDirections[i]);
    }

    float3 dotL     = mul(lightDirections, normal); // angle between light and surface (moreless)
    float3 dotH     = mul(halfVectors, normal) * LightPow_4[i];
    float3 zeroL    = step(float3(0, 0, 0), dotL);   // clamp    
    float3 specular = pow(max(dotH, 0) * zeroL, SpecularPower);

    float3 diffuse = zeroL * dotL;

    ColorPair result = base;
    result.diffuse  += mul(diffuse, lightDiffuse) * DiffuseColor.rgb + EmissiveColor; // diffuse-factor * texture color (+emissive color)
    result.specular += mul(specular, lightSpecular) * SpecularColor; // specular intensity * spec color
    return result;
}

ColorPair ComputeLights5(float3 eyeVector, float3 normal, int count, ColorPair base, VS_Out pin)
{
    float3x3 lightDirections = 0, lightDiffuse = 0, lightSpecular = 0;
    float3x3 halfVectors = 0;
    float3   strengths = 0, distances = 0;
    [unroll]
    for (int i = 0; i < count; i++)
    {
        distances[i] = length(float3x3(LightPos_5[0], LightPos_5[1], LightPos_5[2])[i] - pin.worldPos.xyz);
        strengths[i] = (1 / (distances[i] * distances[i])) * LightPow_5[i];
        lightDirections[i] = float3x3(LightPos_5[0], LightPos_5[1], LightPos_5[2])[i] - pin.worldPos.xyz;
        lightDiffuse[i] = float3x3(LightDiffCol_5[0], LightDiffCol_5[1], LightDiffCol_5[2])[i] * strengths[i];
        lightSpecular[i] = float3x3(LightSpecCol_5[0], LightSpecCol_5[1], LightSpecCol_5[2])[i] * strengths[i];
        halfVectors[i] = normalize(eyeVector - lightDirections[i]);
    }

    float3 dotL = mul(lightDirections, normal); // angle between light and surface (moreless)
    float3 dotH = mul(halfVectors, normal) * LightPow_5[i];
    float3 zeroL = step(float3(0, 0, 0), dotL);   // clamp    
    float3 specular = pow(max(dotH, 0) * zeroL, SpecularPower);

    float3 diffuse = zeroL * dotL;

    ColorPair result = base;
    result.diffuse += mul(diffuse, lightDiffuse) * DiffuseColor.rgb + EmissiveColor; // diffuse-factor * texture color (+emissive color)
    result.specular += mul(specular, lightSpecular) * SpecularColor; // specular intensity * spec color
    return result;
}


ColorPair ComputeLights6(float3 eyeVector, float3 normal, int count, ColorPair base, VS_Out pin)
{
    float3x3 lightDirections = 0, lightDiffuse = 0, lightSpecular = 0;
    float3x3 halfVectors = 0;
    float3   strengths = 0, distances = 0;
    [unroll]
    for (int i = 0; i < count; i++)
    {
        distances[i] = length(float3x3(LightPos_6[0], LightPos_6[1], LightPos_6[2])[i] - pin.worldPos.xyz);
        strengths[i] = (1 / (distances[i] * distances[i])) * LightPow_6[i];
        lightDirections[i] = float3x3(LightPos_6[0], LightPos_6[1], LightPos_6[2])[i] - pin.worldPos.xyz;
        lightDiffuse[i] = float3x3(LightDiffCol_6[0], LightDiffCol_6[1], LightDiffCol_6[2])[i] * strengths[i];
        lightSpecular[i] = float3x3(LightSpecCol_6[0], LightSpecCol_6[1], LightSpecCol_6[2])[i] * strengths[i];
        halfVectors[i] = normalize(eyeVector - lightDirections[i]);
    }

    float3 dotL = mul(lightDirections, normal); // angle between light and surface (moreless)
    float3 dotH = mul(halfVectors, normal) * LightPow_6[i];
    float3 zeroL = step(float3(0, 0, 0), dotL);   // clamp    
    float3 specular = pow(max(dotH, 0) * zeroL, SpecularPower);

    float3 diffuse = zeroL * dotL;

    ColorPair result = base;
    result.diffuse += mul(diffuse, lightDiffuse) * DiffuseColor.rgb + EmissiveColor; // diffuse-factor * texture color (+emissive color)
    result.specular += mul(specular, lightSpecular) * SpecularColor; // specular intensity * spec color
    return result;
}

// PIXEL SHADER 3 LIGHTS_SKIN ( & FOG - no normal-map )
// NOTE: this has been modified with the assumption that transparent stuff will be like glass or ice - super shiny (see below)
float4 PixelShader_3Lights_Skin(VS_Out pin) : SV_Target0
{
    float4 color = tex2D(texSampler, pin.uv); // sample from the texture (& multiply by material's diffuse color [optional])    
    float3 eyeVector = normalize(CamPos - pin.worldPos.xyz);     // this vector points from surface position toward camera
    float3 normal = normalize(pin.normal);

    ColorPair lit = ComputeLights0(eyeVector, normal, LightCount);

    if (LightCount > 3) {
        lit = ComputeLights1(eyeVector, normal, LightCount - 3, lit, pin);
        lit = ComputeLights1(eyeVector, normal, LightCount - 3, lit, pin);
        lit = ComputeLights1(eyeVector, normal, LightCount - 3, lit, pin);
        lit = ComputeLights1(eyeVector, normal, LightCount - 3, lit, pin);
    }
    if (LightCount > 6) {
        lit = ComputeLights2(eyeVector, normal, LightCount - 6, lit, pin);
        lit = ComputeLights2(eyeVector, normal, LightCount - 6, lit, pin);
        lit = ComputeLights2(eyeVector, normal, LightCount - 6, lit, pin);
        lit = ComputeLights2(eyeVector, normal, LightCount - 6, lit, pin);
    }
    if (LightCount > 9) {
        lit = ComputeLights3(eyeVector, normal, LightCount - 9, lit, pin);
        lit = ComputeLights3(eyeVector, normal, LightCount - 9, lit, pin);
        lit = ComputeLights3(eyeVector, normal, LightCount - 9, lit, pin);
        lit = ComputeLights3(eyeVector, normal, LightCount - 9, lit, pin);
    }
    if (LightCount > 12) {
        lit = ComputeLights4(eyeVector, normal, LightCount - 12, lit, pin);
        lit = ComputeLights4(eyeVector, normal, LightCount - 12, lit, pin);
        lit = ComputeLights4(eyeVector, normal, LightCount - 12, lit, pin);
        lit = ComputeLights4(eyeVector, normal, LightCount - 12, lit, pin);
    }
    if (LightCount > 15) {
        lit = ComputeLights5(eyeVector, normal, LightCount - 15, lit, pin);
        lit = ComputeLights5(eyeVector, normal, LightCount - 15, lit, pin);
        lit = ComputeLights5(eyeVector, normal, LightCount - 15, lit, pin);
        lit = ComputeLights5(eyeVector, normal, LightCount - 15, lit, pin);
    }
    if (LightCount > 18) {
        lit = ComputeLights6(eyeVector, normal, LightCount - 18, lit, pin);
        lit = ComputeLights6(eyeVector, normal, LightCount - 18, lit, pin);
        lit = ComputeLights6(eyeVector, normal, LightCount - 18, lit, pin);
        lit = ComputeLights6(eyeVector, normal, LightCount - 18, lit, pin);
    }
    color.rgb *= lit.diffuse;
    color.rgb += lit.specular * ((1 - color.a) * 100);

    if (FogVector.w > 0) color.rgb = lerp(color.rgb, FogColor * color.a, pin.worldPos.w);
    return color;
}


// PIXEL SHADER  3LIGHTS SKIN NORMALMAPPED ( & FOG )
float4 PixelShader_3Lights_Skin_Nmap(VS_N_Out pin) : SV_Target0
{
    float4 color = tex2D(texSampler, pin.uv) * DiffuseColor;     // sample from the texture (& multiply by material's diffuse color [optional])    
    float3 eyeVector = normalize(CamPos - pin.worldPos.xyz);         // this vector points from surface position toward camera
    float3 normCol = normalize(tex2D(normalSampler, pin.uv).xyz - float3(0.5f, 0.5f, 0.5f)); // get value from normal-map of range -1 to +1    
    float3 normal = normalize(mul(normCol, pin.tanSpace));        // get the normal value in tangent-space and ensure normalized length    

    ColorPair lit = ComputeLights0(eyeVector, normal, 3);

    color.rgb *= lit.diffuse;

    color.rgb += lit.specular * color.a; // <-- original version

    if (FogVector.w > 0) color.rgb = lerp(color.rgb, FogColor * color.a, pin.worldPos.w);
    return color;
}



// T E C H N I Q U E S ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
#define TECHNIQUE(name, vsname, psname ) technique name { pass { VertexShader = compile VS_SHADERMODEL vsname(); PixelShader = compile PS_SHADERMODEL psname(); } }

TECHNIQUE(Skin_NormalMapped_Directional_Fog, VertexShader_3Lights_Skin_Nmap, PixelShader_3Lights_Skin_Nmap);  // NORMAL MAPS (& DIRECTIONAL LIGHTS & FOG)
TECHNIQUE(Skin_Directional_Fog, VertexShader_3Lights_Skin, PixelShader_3Lights_Skin);       // NO normal maps