// Cel shading de Sailor Moon para URP, pensado para una GPU de móvil.
//
// POR QUÉ ES PROPIO: en Godot se midió que en la Adreno 619 lo caro no eran
// los triángulos sino el trabajo por píxel, y que cruzar cierto número de
// muestras y registros duplicaba el coste de golpe (docs/aprendido/RENDIMIENTO.md).
// Los toon de terceros (lilToon, UTS3, MToon) son über-shaders con decenas de
// opciones. Este hace solo lo que el estilo necesita:
//
//   · Luz principal partida en dos tonos (luz / sombra) con un borde ajustable
//   · Sombra recibida, que entra en el mismo escalón
//   · Ambiente por sonda esférica, calculado por VÉRTICE
//   · Lightmap para lo estático: la luz y las sombras del decorado vienen
//     horneadas y por píxel cuestan una sola lectura
//   · Luz de borde opcional que pasa POR la iluminación, nunca como emisión
//     (el «aura blanca» de Godot era un rim sumado a la emisión, trampa 110)
//   · Contorno por casco invertido en una segunda pasada
//
// Una sola textura por material. Sin mapas normales, sin especular, sin
// reflejos: el cel shading no los usa.
Shader "SailorMoon/Toon"
{
    Properties
    {
        [MainTexture] _BaseMap ("Textura", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)

        [Header(Sombreado)]
        _ShadeColor ("Color de la sombra (multiplica)", Color) = (0.72, 0.66, 0.82, 1)
        _ShadeThreshold ("Dónde cae el borde de sombra", Range(-1, 1)) = 0.05
        _ShadeSoftness ("Suavidad del borde", Range(0.001, 0.5)) = 0.04
        _AmbientStrength ("Cuánto aporta el ambiente", Range(0, 1)) = 0.35

        [Header(Luz de borde)]
        _RimColor ("Color del borde (negro = apagado)", Color) = (0, 0, 0, 1)
        _RimPower ("Estrechez del borde", Range(1, 16)) = 5

        [Header(Contorno)]
        _OutlineWidth ("Grosor del contorno (m)", Range(0, 0.02)) = 0.004
        _OutlineColor ("Color del contorno", Color) = (0.12, 0.08, 0.14, 1)

        [Header(Opciones)]
        [Toggle(_ALPHATEST_ON)] _AlphaClip ("Recortar por alfa", Float) = 0
        _Cutoff ("Umbral de recorte", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Caras", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // Un único CBUFFER con TODAS las propiedades, en todas las pasadas:
        // es la condición para que el SRP Batcher agrupe los materiales.
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _ShadeColor;
            half _ShadeThreshold;
            half _ShadeSoftness;
            half _AmbientStrength;
            half4 _RimColor;
            half _RimPower;
            float _OutlineWidth;
            half4 _OutlineColor;
            half _Cutoff;
        CBUFFER_END

        // Declara _BaseMap y lo que esperan las pasadas de sombra y
        // profundidad de URP. Va DESPUÉS del CBUFFER, que es lo que ellas leen.
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
        ENDHLSL

        // ------------------------------------------------------------------
        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                // Lightmap si lo hay; si no, el ambiente ya resuelto en el vértice.
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, ambient, 3);
                // x = niebla, yzw = luces puntuales resueltas por vértice
                half4 fogAndVertexLight : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(input.normalOS);

                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = nrm.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                #if defined(LIGHTMAP_ON)
                    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                #else
                    output.ambient = SampleSH(nrm.normalWS);
                #endif

                half3 vertexLight = 0;
                #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                    vertexLight = VertexLighting(pos.positionWS, nrm.normalWS);
                #endif
                output.fogAndVertexLight = half4(ComputeFogFactor(pos.positionCS.z), vertexLight);
                return output;
            }

            // El escalón del cel shading: 0 en sombra, 1 en luz, con un borde
            // de _ShadeSoftness de ancho.
            half Band(half x)
            {
                return smoothstep(_ShadeThreshold - _ShadeSoftness, _ShadeThreshold + _ShadeSoftness, x);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                #if defined(_ALPHATEST_ON)
                    clip(albedo.a - _Cutoff);
                #endif

                half3 normalWS = normalize(input.normalWS);
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                #if defined(LIGHTMAP_ON)
                    half4 shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
                    Light mainLight = GetMainLight(shadowCoord, input.positionWS, shadowMask);
                    // Lo estático: la luz ya viene horneada con sus sombras. Solo
                    // se resta la sombra en tiempo real de lo que se mueve
                    // (la jugadora sobre el camino), que es el modo Subtractive.
                    half3 baked = SampleLightmap(input.lightmapUV, normalWS);
                    MixRealtimeAndBakedGI(mainLight, normalWS, baked);
                    half3 color = albedo.rgb * baked;
                #else
                    Light mainLight = GetMainLight(shadowCoord);
                    half ndotl = dot(normalWS, mainLight.direction);
                    // La sombra recibida entra en el mismo escalón: así la
                    // sombra de un árbol sobre Serena tiene el mismo borde que
                    // la de su propio cuerpo.
                    half lit = Band(ndotl) * mainLight.shadowAttenuation;
                    half3 tone = lerp(_ShadeColor.rgb, half3(1, 1, 1), lit);
                    half3 color = albedo.rgb * (tone * mainLight.color
                                               + input.ambient * _AmbientStrength);

                    // Borde iluminado: se multiplica por la luz, así que en la
                    // cara de sombra y de noche se apaga solo.
                    half3 viewWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                    half rim = pow(saturate(1.0h - dot(normalWS, viewWS)), _RimPower);
                    color += _RimColor.rgb * rim * lit * mainLight.color;
                #endif

                color += albedo.rgb * input.fogAndVertexLight.yzw;
                color = MixFog(color, input.fogAndVertexLight.x);
                return half4(color, 1);
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------
        // Contorno: la malla otra vez, inflada por su normal y dibujando solo
        // las caras traseras. En Godot se midió en +1 % del fotograma.
        Pass
        {
            Name "ToonOutline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half fog : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                // Se infla en espacio de MUNDO para que el grosor sea en metros
                // aunque el FBX venga con escala 0,01.
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                positionWS += normalWS * _OutlineWidth;

                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                #if defined(_ALPHATEST_ON)
                    half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(a - _Cutoff);
                #endif
                // Sin contorno: grosor 0 deja el casco pegado a la malla y el
                // test de profundidad lo descarta.
                clip(_OutlineWidth - 1e-5);
                return half4(MixFog(_OutlineColor.rgb, input.fog), 1);
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // ------------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // ------------------------------------------------------------------
        // Lo que el horneado de luz lee del material: su color. Sin esto, los
        // lightmaps salen como si todo fuera blanco.
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex UniversalVertexMeta
            #pragma fragment FragMeta
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

            half4 FragMeta(Varyings input) : SV_Target
            {
                MetaInput meta = (MetaInput)0;
                meta.Albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                meta.Emission = 0;
                return UniversalFragmentMeta(input, meta);
            }
            ENDHLSL
        }
    }
}
