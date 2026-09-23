Shader "MarginRun/PencilOutline"
{
    Properties
    {
        _PencilStroke("Pencil Stroke", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0.08, 0.07, 0.06, 1)
        _Thickness("Thickness", Range(0.001, 0.15)) = 0.025
        _TextureScale("Texture Scale", Float) = 80
        _TextureStrength("Texture Strength", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            TEXTURE2D(_PencilStroke);
            SAMPLER(sampler_PencilStroke);
            float4 _OutlineColor;
            float _Thickness;
            float _TextureScale;
            float _TextureStrength;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                positionWS += normalWS * _Thickness;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 pencilUV = screenUV * _ScreenParams.xy / max(_TextureScale, 1.0);
                half4 stroke = SAMPLE_TEXTURE2D(_PencilStroke, sampler_PencilStroke, pencilUV);
                half textureAlpha = lerp(1.0, stroke.a, _TextureStrength);
                half4 result = _OutlineColor;
                result.a *= textureAlpha;
                return result;
            }
            ENDHLSL
        }
    }
}
