Shader "Mask/MaskPreview"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _MainTex("Main Tex", 2D) = "white" {}

        _MaskAlpha ("Mask Alpha", Range(0,1)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "MaskPreview"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
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
                float _MaskAlpha;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float3 MaskColor(int index)
            {
                if (index == 0) return float3(1, 0, 0); // R
                if (index == 1) return float3(0, 1, 0); // G
                if (index == 2) return float3(0, 0, 1); // B
                if (index == 3) return float3(1, 1, 0); // A

                if (index == 4) return float3(1, 0, 1);
                if (index == 5) return float3(0, 1, 1);
                if (index == 6) return float3(1, 0.5, 0);
                if (index == 7) return float3(0.5, 0, 1);

                return 0;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float4 baseCol =
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv)
                    * _BaseColor;

                float4 m0 = SAMPLE_TEXTURE2D(_MaskTex0, sampler_MaskTex0, IN.uv);
                float4 m1 = SAMPLE_TEXTURE2D(_MaskTex1, sampler_MaskTex1, IN.uv);

                float3 overlay = 0;
                float alpha = 0;

                // Mask 0~3
                overlay += MaskColor(0) * m0.r;
                overlay += MaskColor(1) * m0.g;
                overlay += MaskColor(2) * m0.b;
                overlay += MaskColor(3) * m0.a;

                // Mask 4~7
                overlay += MaskColor(4) * m1.r;
                overlay += MaskColor(5) * m1.g;
                overlay += MaskColor(6) * m1.b;
                overlay += MaskColor(7) * m1.a;

                alpha = saturate(
                    max(max(m0.r, m0.g), max(m0.b, m0.a)) +
                    max(max(m1.r, m1.g), max(m1.b, m1.a))
                ) * _MaskAlpha;

                float3 finalRGB = lerp(baseCol.rgb, overlay, alpha);

                return float4(finalRGB, baseCol.a);
            }
            ENDHLSL
        }
    }
}
