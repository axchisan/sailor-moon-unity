// La iluminación toon, compartida por Toon.shader (personajes y props) y
// TerrainToon.shader (el suelo). Un solo sitio donde se decide cómo se ve la
// luz, para que el suelo y lo que hay encima no parezcan de dos juegos.
//
// MODELO DE LUZ (Shadowmask, docs/RENDIMIENTO.md):
//   · El SOL se calcula aquí, en dos tonos (luz / sombra). Es barato: un
//     producto escalar y un smoothstep.
//   · Las SOMBRAS de lo estático (árboles, rocas, muros) vienen horneadas en
//     la shadowmask; las de lo que se mueve, del mapa de sombras en tiempo
//     real. URP las mezcla en mainLight.shadowAttenuation.
//   · El AMBIENTE viene del lightmap (luz rebotada horneada) en lo estático, o
//     de las sondas de luz (SH por vértice) en lo que se mueve.
//
// Así lo estático conserva el escalón del cel shading (con Subtractive salía
// suave porque el sol iba horneado) y el pase de sombras solo dibuja lo que
// se mueve.

#ifndef SAILORMOON_TOON_LIGHTING_INCLUDED
#define SAILORMOON_TOON_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct ToonSurface
{
    half3 albedo;
    half3 normalWS;
    float3 positionWS;
    half3 ambient;      // lightmap (ya ponderado) o SH × fuerza
    half4 shadowMask;   // shadowmask horneada o oclusión de sondas
};

struct ToonStyle
{
    half3 shadeColor;   // multiplica en la cara de sombra
    half threshold;     // dónde cae el borde
    half softness;      // ancho del borde
};

half ToonBand(half x, ToonStyle style)
{
    return smoothstep(style.threshold - style.softness, style.threshold + style.softness, x);
}

// Devuelve el color iluminado. `lit` (0 sombra … 1 luz) y `mainLight` salen
// para quien quiera añadir algo encima (la luz de borde de Toon.shader).
half3 ToonShade(ToonSurface s, ToonStyle style, out half lit, out Light mainLight)
{
    float4 shadowCoord = TransformWorldToShadowCoord(s.positionWS);
    mainLight = GetMainLight(shadowCoord, s.positionWS, s.shadowMask);
    half ndotl = dot(s.normalWS, mainLight.direction);
    // La sombra recibida entra en el mismo escalón que la forma: la sombra de
    // un árbol sobre el suelo tiene el mismo borde que la cara oscura de Serena.
    lit = ToonBand(ndotl, style) * mainLight.shadowAttenuation;
    half3 tone = lerp(style.shadeColor, half3(1, 1, 1), lit);
    return s.albedo * (tone * mainLight.color + s.ambient);
}

#endif
