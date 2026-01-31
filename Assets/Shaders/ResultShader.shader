Shader "Mask/Result"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        [MainTexture] _MainTex("Main Tex", 2D) = "white" {}

        _DarkenStrength  ("Darken Strength", Range(0,1)) = 0.5
        _LightenStrength ("Lighten Strength", Range(0,1)) = 0.5
        _BlurStrength    ("Blur Strength", Range(0,5)) = 1.0
        _InvertStrength  ("Invert Strength (Blue Mask)", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "Result"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MaskTex0);
            SAMPLER(sampler_MaskTex0);

            TEXTURE2D(_MaskTex1);
            SAMPLER(sampler_MaskTex1);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;

                float _DarkenStrength;
                float _LightenStrength;
                float _BlurStrength;
                float _InvertStrength;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float3 ApplyDarken(float3 col, float mask)
            {
                return lerp(col, col * (1.0 - _DarkenStrength), mask);
            }

            float3 ApplyLighten(float3 col, float mask)
            {
                float3 lightenCol = col * (1.0 + _LightenStrength * 2.0);

                return lerp(col, lightenCol, mask);
            }

            float3 SampleBlur(float2 uv, float strength)
            {
                float2 texel = strength * _MainTex_TexelSize.xy;
                float3 col = 0;

                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-1, -1) * texel).rgb * 0.09;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0, -1) * texel).rgb * 0.12;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 1, -1) * texel).rgb * 0.09;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-1,  0) * texel).rgb * 0.12;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0,  0) * texel).rgb * 0.16;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 1,  0) * texel).rgb * 0.12;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-1,  1) * texel).rgb * 0.09;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0,  1) * texel).rgb * 0.12;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 1,  1) * texel).rgb * 0.09;

                return col;
            }
            
            float3 ApplyInvert(float3 col, float mask)
            {
                float3 invertedCol = 1.0 - col;
                return lerp(col, invertedCol, mask * _InvertStrength);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float4 m0 = SAMPLE_TEXTURE2D(_MaskTex0, sampler_MaskTex0, IN.uv);

                float darkenMask  = smoothstep(0.45, 0.55, m0.r);
                float lightenMask = smoothstep(0.45, 0.55, m0.g);
                float blurMask    = smoothstep(0.45, 0.55, m0.b);
                float invertMask  = smoothstep(0.45, 0.55, m0.a);

                float3 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;
                
                float3 blurredCol = SampleBlur(IN.uv, _BlurStrength);
                float3 col = lerp(baseCol, blurredCol, blurMask);
                
                col *= _BaseColor.rgb;

                col = ApplyDarken(col, darkenMask);
                col = ApplyLighten(col, lightenMask);
                col = ApplyInvert(col, invertMask);

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
