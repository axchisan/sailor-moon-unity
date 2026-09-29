// El suelo: cuatro capas de Unity Terrain con la iluminación toon compartida.
//
// POR QUÉ ES PROPIO: el terreno era el 37 % del fotograma en el HONOR (medido)
// y el Terrain de URP, con lightmap, salía en blanco y negro (TRAMPAS U5). Este
// hace lo mínimo: cinco lecturas (mezcla + 4 capas), el lightmap, la
// shadowmask y la luz del sol en dos tonos. Sin mapas normales, sin
// especular, sin reflejos.
//
// Límites (docs/ESCENARIOS.md):
//   · Máximo 4 capas por terreno (no hay pasada extra).
//   · El terreno se dibuja sin instanciar (Draw Instanced desactivado).
Shader "SailorMoon/TerrainToon"
{
    Properties
    {
        [Header(Sombreado)]
        _ShadeColor ("Color de la sombra (multiplica)", Color) = (0.66, 0.62, 0.80, 1)
        _ShadeThreshold ("Dónde cae el borde de sombra", Range(-1, 1)) = 0.0
        _ShadeSoftness ("Suavidad del borde", Range(0.001, 0.5)) = 0.06
        _AmbientStrength ("Fuerza del ambiente", Range(0, 1)) = 0.35
        [Enum(Normal,0,Shadowmask,1,Sombra final,2,Lightmap,3)] _Debug ("Depuración", Float) = 0

        // Lo rellena el Terrain.
        [HideInInspector] _Control ("Mezcla", 2D) = "red" {}
        [HideInInspector] _Splat0 ("Capa 0", 2D) = "grey" {}
        [HideInInspector] _Splat1 ("Capa 1", 2D) = "grey" {}
        [HideInInspector] _Splat2 ("Capa 2", 2D) = "grey" {}
        [HideInInspector] _Splat3 ("Capa 3", 2D) = "grey" {}
        [HideInInspector] _MainTex ("Mapa base", 2D) = "grey" {}
        [HideInInspector] _BaseColor ("Color base", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry-100"
            "TerrainCompatible" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_Control); SAMPLER(sampler_Control);
        TEXTURE2D(_Splat0);  SAMPLER(sampler_Splat0);
        TEXTURE2D(_Splat1);
        TEXTURE2D(_Splat2);
        TEXTURE2D(_Splat3);

        CBUFFER_START(UnityPerMaterial)
            float4 _Control_ST;
            float4 _Control_TexelSize;
            float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
            half4 _ShadeColor;
            half _ShadeThreshold;
            half _ShadeSoftness;
            half _AmbientStrength;
            half _Debug;
        CBUFFER_END

        // Mezcla de las cuatro capas en la coordenada del terreno (0–1).
        half3 TerrainAlbedo(float2 uv)
        {
            // Centro del texel de mezcla, como hace el Terrain de URP: sin esto
            // el borde entre capas se desplaza medio texel.
            float2 cuv = (uv * (_Control_TexelSize.zw - 1.0) + 0.5) * _Control_TexelSize.xy;
            half4 w = SAMPLE_TEXTURE2D(_Control, sampler_Control, cuv);
            half3 c = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uv * _Splat0_ST.xy + _Splat0_ST.zw).rgb * w.r;
            c += SAMPLE_TEXTURE2D(_Splat1, sampler_Splat0, uv * _Splat1_ST.xy + _Splat1_ST.zw).rgb * w.g;
            c += SAMPLE_TEXTURE2D(_Splat2, sampler_Splat0, uv * _Splat2_ST.xy + _Splat2_ST.zw).rgb * w.b;
            c += SAMPLE_TEXTURE2D(_Splat3, sampler_Splat0, uv * _Splat3_ST.xy + _Splat3_ST.zw).rgb * w.a;
            return c;
        }
        ENDHLSL

        Pass
        {
            Name "TerrainToonForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #pragma multi_compile_fog

            #include "ToonLighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, ambient, 3);
                half fog : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.uv = input.uv;
                #if defined(LIGHTMAP_ON)
                    // El terreno no tiene UV2: el lightmap usa su coordenada 0–1.
                    OUTPUT_LIGHTMAP_UV(input.uv, unity_LightmapST, o.lightmapUV);
                #else
                    o.ambient = SampleSH(o.normalWS);
                #endif
                o.fog = ComputeFogFactor(pos.positionCS.z);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                ToonSurface s;
                s.albedo = TerrainAlbedo(input.uv);
                s.normalWS = normalize(input.normalWS);
                s.positionWS = input.positionWS;
                #if defined(LIGHTMAP_ON)
                    // La luz rebotada horneada, con la misma fuerza que el ambiente
                    // de las sondas: entera, se sumaba al sol y dejaba el suelo al
                    // ~190 % de su color (lavado, y la sombra casi no se leía).
                    s.ambient = SampleLightmap(input.lightmapUV, s.normalWS) * _AmbientStrength;
                    s.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
                #else
                    s.ambient = input.ambient * _AmbientStrength;
                    s.shadowMask = SAMPLE_SHADOWMASK(0);
                #endif

                ToonStyle style;
                style.shadeColor = _ShadeColor.rgb;
                style.threshold = _ShadeThreshold;
                style.softness = _ShadeSoftness;

                half lit;
                Light mainLight;
                half3 color = ToonShade(s, style, lit, mainLight);
                // Vistas de depuración (material, campo «Depuración»).
                if (_Debug > 2.5) return half4(s.ambient, 1);
                if (_Debug > 1.5) return half4(mainLight.shadowAttenuation.xxx, 1);
                if (_Debug > 0.5) return half4(s.shadowMask.rgb, 1);
                return half4(MixFog(color, input.fog), 1);
            }
            ENDHLSL
        }

        // Lo que el horneado lee del suelo: su color, para que la luz rebotada
        // salga verde bajo la hierba y ocre junto al camino.
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex VertMeta
            #pragma fragment FragMeta
            #pragma shader_feature EDITOR_VISUALIZATION
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            struct AttributesMeta
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct VaryingsMeta
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VaryingsMeta VertMeta(AttributesMeta input)
            {
                VaryingsMeta o;
                // El terreno hornea con su coordenada 0–1 (no tiene UV2).
                o.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv0, input.uv2);
                o.uv = input.uv0;
                return o;
            }

            half4 FragMeta(VaryingsMeta input) : SV_Target
            {
                MetaInput meta = (MetaInput)0;
                meta.Albedo = TerrainAlbedo(input.uv);
                return UnityMetaFragment(meta);
            }
            ENDHLSL
        }
    }

    // El horneado del Terrain genera primero un mapa base con este shader; sin
    // la dependencia, el lightmapper descarta el terreno («Instance with no
    // materials»). Es el de URP: mezcla las mismas cuatro capas.
    Dependency "BaseMapGenShader" = "Hidden/Universal Render Pipeline/Terrain/Lit (Basemap Gen)"
}
