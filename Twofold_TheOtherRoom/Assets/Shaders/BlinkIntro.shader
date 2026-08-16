// "Hidden/" 으로 시작하면 머티리얼 인스펙터의 셰이더 드롭다운에 뜨지 않음.
Shader "FullScreen/BlinkIntro"
{
    Properties
    {
        // 0~1 은 보통 깜빡임, 1~2 는 눈꺼풀이 화면 밖까지 열려 전체 화면이 되는 구간.
        _Blink        ("Blink (0 감김 / 1 뜸 / 2 전체 화면)", Range(0, 2))       = 0

        _EyeWidth     ("Eye Half Width",                      Range(0.05, 2))    = 0.6
        _EyeHeight    ("Upper Lid Peak",                      Range(0.01, 0.5))  = 0.42
        _EyeHeightDown("Lower Lid Peak",                      Range(0.01, 0.5))  = 0.4
        _LidBias      ("Upper Lid Bias",                      Range(0, 1))       = 0.1
        _Softness     ("Edge Softness",                       Range(0.001, 0.3)) = 0.12

        _Exposure     ("Exposure When Closed",                Range(1, 8))       = 2.0
        _Bleed        ("Light Bleed",                         Range(0, 1))       = 0.6
        _BlurRadius   ("Blur Radius When Closed",             Range(0, 0.05))    = 0.016
        _BlurFalloff  ("Focus Recovery",                      Range(0.25, 3))    = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "BlinkIntro"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Vert / Varyings / _BlitTexture 는 Blit.hlsl 제공. URP 17 부터 universal → core 로 이동.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // SRP Batcher 호환을 위해 CBUFFER 로 묶음.
            CBUFFER_START(UnityPerMaterial)
                float _Blink;
                float _EyeWidth;
                float _EyeHeight;
                float _EyeHeightDown;
                float _LidBias;
                float _Softness;
                float _Exposure;
                float _Bleed;
                float _BlurRadius;
                float _BlurFalloff;
            CBUFFER_END

            // 링 2겹 13탭 블러. 도입부에 한 번 재생되고 끝나므로 탭 수를 넉넉히 사용.
            half3 SampleBlurred(float2 uv, float radius)
            {
                half3 sum   = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float total = 1.0;

                if (radius <= 0.0001) return sum;

                // 화면 비율 보정. 없으면 블러가 가로로 늘어남.
                float2 aspect = float2(_ScreenParams.y / _ScreenParams.x, 1.0);

                [unroll]
                for (int i = 0; i < 12; i++)
                {
                    float  angle = (i / 12.0) * 6.2831853;
                    float  ring  = (i < 6) ? 0.5 : 1.0;   // 안쪽 6개 + 바깥 6개
                    float2 off   = float2(cos(angle), sin(angle)) * radius * ring * aspect;

                    sum   += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + off).rgb;
                    total += 1.0;
                }

                return sum / total;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                float blinkN = saturate(_Blink);          // 0~1 : 보통 깜빡임
                float over   = saturate(_Blink - 1.0);    // 1~2 : 화면 전체로 벌어지는 구간

                // _BlurFalloff 가 초점 회복 속도를 눈꺼풀과 분리. 1 미만이면 눈은 떠져도 뿌옇게 보임.
                float blur = _BlurRadius * pow(1.0 - blinkN, _BlurFalloff);
                half3 col  = SampleBlurred(uv, blur);

                float2 p    = uv - 0.5;
                float  soft = max(_Softness, 0.0001);

                // 마지막에는 좌우 끝점도 화면 밖으로 밀어낸다.
                // 세로만 벌리면 양옆이 뾰족하게 좁혀진 채로 남아 부자연스럽고,
                // 끝점이 화면 안(w < 0.5)이면 모서리를 아예 못 덮는다.
                float w = lerp(max(_EyeWidth, 0.01), max(_EyeWidth, 0.9), over);

                // arc 는 양 끝 0, 가운데 1 인 포물선.
                // 어떤 높이를 곱해도 (±w, 0) 을 지나므로 꼭짓점만 움직이고 좌우 끝점은 고정.
                float nx  = p.x / w;
                float arc = saturate(1.0 - nx * nx);

                // 화면을 덮는 데 필요한 꼭짓점 배율. arc 가 가장 작은 화면 좌우 끝이 기준.
                // 직접 값을 찾으면 _EyeWidth 를 바꿀 때마다 다시 찾아야 하므로 여기서 계산한다.
                float arcEdge = max(1.0 - 0.25 / (w * w), 1e-4);
                float needU   = (0.5 + soft) / max(_EyeHeight     * arcEdge, 1e-4);
                float needL   = (0.5 + soft) / max(_EyeHeightDown * arcEdge, 1e-4);

                // _LidBias 가 클수록 아랫눈꺼풀이 먼저 열려 움직임이 윗눈꺼풀에 몰림.
                float openU = blinkN                                                * lerp(1.0, needU, over);
                float openL = pow(blinkN, lerp(1.0, 0.15, saturate(_LidBias))) * lerp(1.0, needL, over);

                float yUp =  _EyeHeight     * openU * arc;
                float yDn = -_EyeHeightDown * openL * arc;

                float d = max(abs(p.x) - w,        // 좌우 끝점 바깥
                          max(p.y - yUp,           // 윗눈꺼풀 위
                              yDn - p.y));         // 아랫눈꺼풀 아래

                float mask = 1.0 - smoothstep(-soft, soft, d);

                // 다 감기면 두 곡선이 한 선으로 겹쳐 실선이 남음. 마지막에 눌러서 제거.
                mask *= smoothstep(0.0, 0.04, blinkN);

                col *= lerp(_Exposure, 1.0, blinkN);   // 감았을수록 과노출

                // 실눈일 때 하얗게 날아가는 느낌
                float bleed = saturate(mask * (1.0 - blinkN)) * _Bleed;
                col = lerp(col, half3(1.0, 1.0, 1.0), bleed);

                return half4(col * mask, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
