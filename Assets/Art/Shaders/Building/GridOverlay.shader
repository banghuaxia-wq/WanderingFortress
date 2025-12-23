Shader "WF/Building/GridOverlay"
{
    Properties
    {
        _LineColor("Line Color", Color) = (1,1,1,1)
        _BaseAlpha("Base Alpha", Range(0,1)) = 0.35
        _CellSize("Cell Size", Float) = 1
        _LineWidth("Line Width", Float) = 0.04

        _FadeStart("Fade Start", Float) = 50
        _FadeEnd("Fade End", Float) = 80
        _GridCenter("Grid Center", Vector) = (0,0,0,0)

        _HighlightEnabled("Highlight Enabled", Float) = 0
        _HighlightColor("Highlight Color", Color) = (0.2,1,0.2,0.45)
        _HighlightWorldMin("Highlight World Min", Vector) = (0,0,0,0)
        _HighlightWorldMax("Highlight World Max", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            Name "GridOverlay"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _LineColor;
                float _BaseAlpha;
                float _CellSize;
                float _LineWidth;
                float _FadeStart;
                float _FadeEnd;
                float4 _GridCenter;
                float _HighlightEnabled;
                float4 _HighlightColor;
                float4 _HighlightWorldMin;
                float4 _HighlightWorldMax;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float cell = max(0.0001, _CellSize);
                float2 p = input.worldPos.xz / cell;
                float2 fracp = frac(p);
                float2 distToEdge = min(fracp, 1.0 - fracp);
                float distWorld = min(distToEdge.x, distToEdge.y) * cell;

                float lineMask = 1.0 - smoothstep(_LineWidth, _LineWidth * 1.5, distWorld);
                float alpha = clamp(lineMask, 0.0, 1.0) * _BaseAlpha;

                float2 centerXZ = _GridCenter.xz;
                float d = distance(input.worldPos.xz, centerXZ);
                float fade = 1.0 - smoothstep(_FadeStart, _FadeEnd, d);
                alpha *= fade;

                float4 col = float4(_LineColor.rgb, alpha);

                if (_HighlightEnabled > 0.5)
                {
                    float2 minXZ = _HighlightWorldMin.xz;
                    float2 maxXZ = _HighlightWorldMax.xz;
                    float inRect = step(minXZ.x, input.worldPos.x) * step(minXZ.y, input.worldPos.z) * step(input.worldPos.x, maxXZ.x) * step(input.worldPos.z, maxXZ.y);

                    float4 hcol = _HighlightColor;
                    hcol.a *= fade;
                    col = lerp(col, float4(hcol.rgb, max(col.a, hcol.a)), inRect);
                }

                return col;
            }
            ENDHLSL
        }
    }
}
