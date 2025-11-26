Shader "WF/OutlineNormalExpand"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0.2,0.8,1,1)
        _OutlineThickness ("Outline Thickness (View Space)", Float) = 0.015
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _OutlineThickness; fixed4 _OutlineColor;
            struct v2f { float4 pos:SV_POSITION; };
            v2f vert(appdata_full v)
            {
                float3 viewPos = mul(UNITY_MATRIX_MV, v.vertex).xyz;
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float3 extruded = viewPos + viewNormal * _OutlineThickness;
                v2f o; o.pos = mul(UNITY_MATRIX_P, float4(extruded, 1.0));
                return o;
            }
            fixed4 frag(v2f i):SV_Target { return _OutlineColor; }
            ENDCG
        }
    }
}
