// Liminal-Passes.hlsl
// The three helper passes every Liminal shader shares. They draw no colour:
//   ShadowCaster  - draws the object into a light's shadow map, so it casts shadows
//   DepthOnly     - writes how far each pixel is from the camera (the depth texture)
//   DepthNormals  - writes depth and the surface direction, used by SSAO and screen space reflections
// A shader can hook into them by defining, before this file is included:
//   LIMINAL_DISPLACE      + void  LiminalDisplace(inout float3 positionOS, inout float3 normalOS, float2 uv)
//                           (moves vertices - the same movement must happen in every pass, or the shadow and the object disagree)
//   LIMINAL_DEPTH_NORMAL  + float3 LiminalDepthNormal(float2 uv, float3 positionWS, float3 normalWS)
//                           (the per-pixel normal written for SSAO and SSR)
//   LIMINAL_SMOOTHNESS    + half  LiminalSmoothness()  (written for screen space reflections)
//   LIMINAL_TWO_SIDED     (the back faces are drawn too: their normal is turned around)

#ifndef LIMINAL_PASSES_INCLUDED
#define LIMINAL_PASSES_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

struct LiminalAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 uv         : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

void LiminalMove(inout LiminalAttributes input)
{
#if defined(LIMINAL_DISPLACE)
    LiminalDisplace(input.positionOS.xyz, input.normalOS, input.uv);
#endif
}

// ---------- ShadowCaster ----------
// Set by URP for each shadow-casting light: its direction (sun) or its position (spot and point lights).
float3 _LightDirection;
float3 _LightPosition;

struct LiminalShadowVaryings
{
    float4 positionCS : SV_POSITION;
};

LiminalShadowVaryings LiminalShadowVertex(LiminalAttributes input)
{
    LiminalShadowVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    LiminalMove(input);
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif
    // the shadow bias pushes the surface slightly away, so it does not shadow itself (shadow acne)
    output.positionCS = ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS)));
    return output;
}

half4 LiminalShadowFragment(LiminalShadowVaryings input) : SV_Target
{
    return 0;
}

// ---------- DepthOnly ----------
struct LiminalDepthVaryings
{
    float4 positionCS : SV_POSITION;
};

LiminalDepthVaryings LiminalDepthOnlyVertex(LiminalAttributes input)
{
    LiminalDepthVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    LiminalMove(input);
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

half LiminalDepthOnlyFragment(LiminalDepthVaryings input) : SV_Target
{
    return input.positionCS.z;
}

// ---------- DepthNormals ----------
struct LiminalDepthNormalsVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv         : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS   : TEXCOORD2;
};

LiminalDepthNormalsVaryings LiminalDepthNormalsVertex(LiminalAttributes input)
{
    LiminalDepthNormalsVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    LiminalMove(input);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.uv = input.uv;
    return output;
}

half4 LiminalDepthNormalsFragment(LiminalDepthNormalsVaryings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
{
    float3 normalWS = normalize(input.normalWS);
#if defined(LIMINAL_TWO_SIDED)
    normalWS *= IS_FRONT_VFACE(face, 1.0, -1.0);
#endif
#if defined(LIMINAL_DEPTH_NORMAL)
    normalWS = LiminalDepthNormal(input.uv, input.positionWS, normalWS);
#endif
    half4 output;
#if defined(_GBUFFER_NORMALS_OCT)
    float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
    output = half4(PackFloat2To888(saturate(octNormalWS * 0.5 + 0.5)), 0.0);
#else
    output = half4(normalWS, 0.0);
#endif
#if defined(_WRITE_SMOOTHNESS) && defined(LIMINAL_SMOOTHNESS)
    output.a = LiminalSmoothness();
#endif
    return output;
}

#endif
