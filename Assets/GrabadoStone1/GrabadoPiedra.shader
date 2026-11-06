Shader "Nia/GrabadoPiedra"
{
    Properties
    {
        [NoScaleOffset] _Imagen ("Imagen", 2D) = "white" {}
        _Intensidad ("Intensidad", Range(0, 1)) = 0.8
        _Bisel ("Ancho del bisel", Range(0.001, 0.03)) = 0.008
        [HideInInspector] _EjeU ("Eje U", Vector) = (1, 0, 0, 0)
        [HideInInspector] _EjeV ("Eje V", Vector) = (0, 1, 0, 0)
        [HideInInspector] _Vista ("Vista de diagnóstico", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+1" "RenderType" = "Transparent" }
        Cull Back
        ZWrite Off
        ZTest LEqual
        Offset 0, -1
        Blend One OneMinusSrcAlpha, Zero One

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_Imagen);
        SAMPLER(sampler_Imagen);

        CBUFFER_START(UnityPerMaterial)
            float4 _EjeU;
            float4 _EjeV;
            float _Intensidad;
            float _Bisel;
            float _Vista;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        Varyings Vert(Attributes input)
        {
            Varyings output;
            float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
            output.positionCS = TransformWorldToHClip(positionWS);
            float4 posicion = float4(positionWS, 1);
            output.uv = float2(dot(posicion, _EjeU), dot(posicion, _EjeV));
            return output;
        }

        float Mascara(float2 uv)
        {
            float4 pixel = SAMPLE_TEXTURE2D(_Imagen, sampler_Imagen, saturate(uv));
            if (any(uv < 0) || any(uv > 1))
                return 0;

            float canalMinimo = min(pixel.r, min(pixel.g, pixel.b));
            return pixel.a * smoothstep(0.045, 1.0, 1.0 - canalMinimo);
        }

        half4 Frag(Varyings input) : SV_Target
        {
            // Malla muestra si el renderizador está dibujando la capa.
            if (_Vista > 1.5)
                return half4(1, 0, 1, 1);

            float2 uv = input.uv;
            clip(min(uv, 1.0 - uv));

            // Área muestra la ubicación de la proyección sobre la malla.
            if (_Vista > 0.5)
                return half4(0.7, 0, 0.7, 0.7);

            float2 paso = _Bisel * float2(-0.6, 0.8);
            float centro = Mascara(uv);
            float pendiente = Mascara(uv - paso) - Mascara(uv + paso);

            float sombra = saturate(centro * 0.40 + saturate(pendiente) * 0.55);
            float luz = saturate(-pendiente) * 0.42;
            float alpha = saturate((sombra + luz) * _Intensidad);
            float3 color = (sombra * float3(0.10, 0.09, 0.08)
                + luz * float3(0.82, 0.79, 0.72)) * _Intensidad;
            return half4(color, alpha);
        }
        ENDHLSL

        Pass
        {
            Name "Grabado2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }

        Pass
        {
            Name "GrabadoForward"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
