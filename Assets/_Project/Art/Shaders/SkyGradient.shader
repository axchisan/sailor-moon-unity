// Cielo de anime: tres colores en vertical y un halo suave hacia el sol.
//
// El cielo se pinta en todos los píxeles que no tapa nada — en un parque con
// el horizonte despejado, un tercio de la pantalla —, así que lo que cueste
// aquí se paga en grande. En Godot el cielo llegó a hacer 32 senos por píxel
// (trampa 67). Este hace un degradado y un pow: unas pocas instrucciones.
Shader "SailorMoon/SkyGradient"
{
    Properties
    {
        _TopColor ("Cénit", Color) = (0.33, 0.58, 0.93, 1)
        _HorizonColor ("Horizonte", Color) = (0.82, 0.88, 1.0, 1)
        _GroundColor ("Bajo el horizonte", Color) = (0.72, 0.76, 0.88, 1)
        _Exponent ("Altura del degradado", Range(0.1, 4)) = 0.6
        _SunColor ("Halo del sol", Color) = (1.0, 0.93, 0.8, 1)
        _SunSize ("Tamaño del halo", Range(1, 256)) = 24
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor, _HorizonColor, _GroundColor, _SunColor;
                half _Exponent, _SunSize;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.dir = input.positionOS.xyz;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                half3 d = normalize(i.dir);
                half up = pow(saturate(d.y), _Exponent);
                half down = saturate(-d.y * 4);
                half3 c = lerp(_HorizonColor.rgb, _TopColor.rgb, up);
                c = lerp(c, _GroundColor.rgb, down);
                half sun = pow(saturate(dot(d, _MainLightPosition.xyz)), _SunSize);
                c += _SunColor.rgb * sun * 0.35;
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
