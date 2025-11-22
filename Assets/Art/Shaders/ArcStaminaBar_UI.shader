Shader "UI/ArcStaminaBar"
{
    Properties
    {
        _Stamina ("Stamina (0-1)", Range(0,1)) = 1
        _CriticalThreshold ("Critical Threshold", Range(0,1)) = 0.2
        _HighColor ("High Color", Color) = (0,1,0,1)
        _LowColor ("Low Color", Color)  = (1,0,0,1)
        _BackgroundColor ("Background", Color) = (0,0,0,0)

        _StartAngle ("Start Angle (deg)", Range(0,360)) = 0
        _Fill ("Fill (0-1)", Range(0,1)) = 1
        _Clockwise ("Clockwise (1 or -1)", Float) = 1
        _Center ("Center UV", Vector) = (0.5, 0.5, 0, 0)

        _InnerRadius ("Inner Radius", Range(0,1)) = 0.35
        _OuterRadius ("Outer Radius", Range(0,1)) = 0.45
        _Softness ("Edge Softness", Range(0,0.2)) = 0.01
        _AngleSoftnessDeg ("Angle Softness (deg)", Range(0,45)) = 2

        _GlowColor ("Glow Color", Color) = (1,1,0,1)
        _GlowStrength ("Glow Strength", Range(0,5)) = 0
        _GlowWidth ("Glow Width", Range(0,0.5)) = 0.05
        _GlowAlpha ("Glow Alpha Contribution", Range(0,1)) = 0.5
        _UseFillForColor ("Use Fill As Stamina (0/1)", Float) = 0
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        // Premultiplied alpha for clean edges
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 color : COLOR;
            };

            float  _Stamina;
            float  _CriticalThreshold;
            float4 _HighColor;
            float4 _LowColor;
            float4 _BackgroundColor;

            float  _StartAngle; // degrees
            float  _Fill;       // 0..1 fraction of full circle
            float  _Clockwise;  // 1 or -1
            float2 _Center;     // 0..1

            float  _InnerRadius;
            float  _OuterRadius;
            float  _Softness;
            float  _AngleSoftnessDeg; // degrees

            float4 _GlowColor;
            float  _GlowStrength;
            float  _GlowWidth;
            float  _GlowAlpha;
            float  _UseFillForColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.texcoord;
                o.color = v.color;
                return o;
            }

            // constants
            static const float PI    = 3.14159265358979323846;
            static const float TWOPI = 6.28318530717958647692;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                // Position relative to center in UV space
                float2 p = uv - _Center;
                float r = length(p);

                // Ring mask with soft inner/outer edges
                float ringMask = saturate( smoothstep(_InnerRadius, _InnerRadius + _Softness, r)
                                        -  smoothstep(_OuterRadius, _OuterRadius + _Softness, r) );

                // Angular mask (arc segment)
                float ang = atan2(p.y, p.x);               // -PI..PI
                float start = _StartAngle * (PI/180.0);
                ang = ang - start;                          // shift start
                if (_Clockwise < 0.0) ang = -ang;           // direction
                // Wrap to [0, TWOPI)
                ang = fmod(ang + TWOPI, TWOPI);
                float end = saturate(_Fill) * TWOPI;
                float angleSoft = _AngleSoftnessDeg * (PI/180.0);
                float angleMask = saturate( smoothstep(0.0, angleSoft, ang) * (1.0 - smoothstep(end, end + angleSoft, ang)) );

                float mask = ringMask * angleMask;

                // Glow around ring center radius
                float midR = 0.5 * (_InnerRadius + _OuterRadius);
                float d = abs(r - midR);
                float glow = exp(-pow(d / max(_GlowWidth, 1e-5), 2.0)) * angleMask * _GlowStrength;

                // Color selection driven by stamina or fill (switchable)
                float3 high = _HighColor.rgb;
                float3 low  = _LowColor.rgb;
                float thr = max(_CriticalThreshold, 1e-5);
                float driver = (_UseFillForColor > 0.5) ? saturate(_Fill) : saturate(_Stamina);
                float3 baseColor = (driver >= thr) ? high : lerp(low, high, saturate(driver / thr));

                // Premultiplied composition
                float alphaBase = mask;
                float alphaGlow = saturate(glow * _GlowAlpha);
                float outA = saturate(alphaBase + alphaGlow);
                float3 outRGB = baseColor * alphaBase + _GlowColor.rgb * alphaGlow;

                return float4(outRGB, outA);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
