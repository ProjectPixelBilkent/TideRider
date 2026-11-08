// Cloud that blurs and greys out whatever is underneath it instead of painting over it.
// Requires the 2D Renderer's "Camera Sorting Layer Texture" to be enabled, with its foremost layer set
// to the last layer the clouds should blur (GameObjects), and the clouds on a later sorting layer.
Shader "TideRider/CloudBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Tint ("Cloud Tint", Color) = (0.62, 0.66, 0.72, 1)
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.35
        _BlurRadius ("Blur Radius (pixels at 1080p)", Range(0, 48)) = 18
        _EdgeSoftness ("Edge Softness (screen pixels)", Range(0, 40)) = 14

        // Kept so SpriteRenderer colour works the same as the default sprite shaders.
        [HideInInspector] _Color ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _Tint;
                half _TintStrength;
                float _BlurRadius;
                float _EdgeSoftness;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
                float4 screenPos  : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                SetUpSpriteInstanceProperties();
                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                o.color = v.color * _Color * unity_SpriteColor;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            #define BLUR_TAPS 16
            #define GOLDEN_ANGLE 2.39996323

            half SampleMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Feathered cloud mask: average the sprite alpha over a small ring (sized in screen pixels)
                // so the blurred area fades in at the cloud's edge instead of ending on a hard outline.
                float2 e = fwidth(i.uv) * _EdgeSoftness;
                half mask = SampleMask(i.uv) * 2.0;
                mask += SampleMask(i.uv + float2( e.x, 0));
                mask += SampleMask(i.uv + float2(-e.x, 0));
                mask += SampleMask(i.uv + float2(0,  e.y));
                mask += SampleMask(i.uv + float2(0, -e.y));
                mask += SampleMask(i.uv + float2( e.x,  e.y) * 0.7071);
                mask += SampleMask(i.uv + float2(-e.x,  e.y) * 0.7071);
                mask += SampleMask(i.uv + float2( e.x, -e.y) * 0.7071);
                mask += SampleMask(i.uv + float2(-e.x, -e.y) * 0.7071);
                mask = (mask / 10.0) * i.color.a;
                clip(mask - 0.002);

                // Disc blur of everything rendered beneath the clouds (golden-angle spiral sampling).
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 radius = (_BlurRadius * (_ScreenParams.y / 1080.0)) / _ScreenParams.xy;

                half3 sum = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, screenUV).rgb;
                [unroll]
                for (int k = 0; k < BLUR_TAPS; k++)
                {
                    float r = sqrt((k + 0.5) / BLUR_TAPS);
                    float a = k * GOLDEN_ANGLE;
                    float2 offset = float2(cos(a), sin(a)) * r * radius;
                    sum += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, screenUV + offset).rgb;
                }
                half3 blurred = sum / (BLUR_TAPS + 1);

                // Grey it out like mist; renderer colour can tint it further.
                half3 color = lerp(blurred, _Tint.rgb, _TintStrength) * i.color.rgb;
                return half4(color, mask);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
