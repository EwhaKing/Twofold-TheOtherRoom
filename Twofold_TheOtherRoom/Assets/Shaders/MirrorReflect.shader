Shader "Mirror/MirrorReflect"
{
    // 거울 위에 얹는 반사 레이어. 원본 머티리얼을 지우지 않고 그 위에 알파로 섞인다.
    // _Reflectivity 0 = 원본 거울 그대로, 1 = 반사만. 구동은 MirrorReflection 담당.
    //
    // 원본을 갈아끼우지 않는 이유: 거울은 URP Lit + 노멀맵 + 광택이라
    // Unlit 단색으로 대체하면 명암과 반짝임이 사라져 하얀 판으로 보인다.
    Properties
    {
        _ReflectionTex ("Reflection", 2D) = "black" {}
        _ReflectionTint ("Reflection Tint", Color) = (1, 1, 1, 1)
        _Reflectivity ("Reflectivity", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
        }

        Pass
        {
            Name "MirrorReflect"
            Tags { "LightMode" = "UniversalForward" }

            // 원본이 이미 깊이를 썼으므로 여기서는 겹쳐 그리기만 함
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _ReflectionTint;
                float  _Reflectivity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos  = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // 반사 카메라가 소스와 같은 projection 을 써서 스크린 UV 로 그대로 맞음
                float2 uv = input.screenPos.xy / input.screenPos.w;

                half3 reflection = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, uv).rgb
                                 * _ReflectionTint.rgb;

                return half4(reflection, _Reflectivity);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
