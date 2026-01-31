Shader "Mask/Paint"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        [MainTexture] _MainTex("Main Tex", 2D) = "white" {}

        _ActiveMask ("Active Mask Index", Int) = 0
        _MaskAlpha  ("Mask Alpha", Range(0,1)) = 0.6
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
            Name "PaintPreview"
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
                int _ActiveMask;
                float _MaskAlpha;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float3 ActiveMaskColor(int index)
            {
                if (index == 0) return float3(1,0,0);
                if (index == 1) return float3(0,1,0);
                if (index == 2) return float3(0,0,1);
                if (index == 3) return float3(1,1,0);
                if (index == 4) return float3(1,0,1);
                if (index == 5) return float3(0,1,1);
                if (index == 6) return float3(1,0.5,0);
                if (index == 7) return float3(0.5,0,1);
                return 0;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float4 baseCol =
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv)
                    * _BaseColor;

                float4 m0 = SAMPLE_TEXTURE2D(_MaskTex0, sampler_MaskTex0, IN.uv);
                float4 m1 = SAMPLE_TEXTURE2D(_MaskTex1, sampler_MaskTex1, IN.uv);

                float maskValue = 0;

                if (_ActiveMask < 4)
                {
                    maskValue =
                        _ActiveMask == 0 ? m0.r :
                        _ActiveMask == 1 ? m0.g :
                        _ActiveMask == 2 ? m0.b :
                                           m0.a;
                }
                else
                {
                    int idx = _ActiveMask - 4;
                    maskValue =
                        idx == 0 ? m1.r :
                        idx == 1 ? m1.g :
                        idx == 2 ? m1.b :
                                   m1.a;
                }

                maskValue = step(0.5, maskValue);

                float3 overlay = ActiveMaskColor(_ActiveMask);
                float alpha = maskValue * _MaskAlpha;

                float3 finalRGB = lerp(baseCol.rgb, overlay, alpha);

                return float4(finalRGB, baseCol.a);
            }
            ENDHLSL
        }
    }
}
