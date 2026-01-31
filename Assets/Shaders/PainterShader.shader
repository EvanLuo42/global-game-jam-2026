Shader "Mask/Painter"
{
    Properties
    {
        _MainTex ("Mask (RGBA)", 2D) = "black" {}
        _Center  ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius  ("Radius", Range(0.001, 0.5)) = 0.05
        _Softness("Softness", Range(0.0, 0.2)) = 0.02
        _Channel ("Channel", Int) = 0     // 0=R 1=G 2=B 3=A
        _Value   ("Paint Value", Int) = 1 // 1=paint, 0=erase
        _Aspect  ("Aspect Ratio", Float) = 1.78
    }
    SubShader
    {
        Pass
        {
            Name "BrushPass"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_ST;

            float4 _Center;   // xy = uv
            float  _Radius;
            float  _Softness;
            int    _Channel;  // 0..3
            float  _Value;    // 0 or 1
            float _Aspect;

            Varyings Vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float4 Frag (Varyings IN) : SV_Target
            {
                float4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float2 center = _Center.xy;
                float2 uv = IN.uv;
                
                uv.x *= _Aspect;
                center.x *= _Aspect;

                float d = distance(uv, center);
                
                float edge0 = _Radius;
                float edge1 = _Radius - _Softness;
                float w = smoothstep(edge0, edge1, d);

                float4 channelMask =
                    _Channel == 0 ? float4(1,0,0,0) :
                    _Channel == 1 ? float4(0,1,0,0) :
                    _Channel == 2 ? float4(0,0,1,0) :
                                      float4(0,0,0,1);

                float4 target = channelMask * _Value;
                
                mask = lerp(mask, target, w * channelMask);

                return mask;
            }
            ENDHLSL
        }
    }
}
