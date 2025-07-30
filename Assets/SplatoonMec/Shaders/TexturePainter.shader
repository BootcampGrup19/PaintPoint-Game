Shader "TNTC/TexturePainter"
{
    Properties
    {
        _PainterColor ("Painter Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 _PainterPosition;
            float _Radius;
            float _Hardness;
            float _Strength;
            float4 _PainterColor;
            float _PrepareUV;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            float organicNoise(float2 uv)
            {
                float r1 = rand(uv * 5.0);
                float r2 = rand((uv + 0.37) * 10.0);
                return (r1 + r2) * 0.5;
            }

            float mask(float3 position, float3 center, float radius, float hardness)
            {
                float m = distance(center, position);
                float noise = organicNoise(position.xz);
                m += (noise - 0.5) * radius * 1.2;
                return 1 - smoothstep(radius * hardness, radius, m);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;

                float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2(2, 2) - float2(1, 1));
                o.vertex = uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_PrepareUV > 0)
                {
                    return float4(0, 0, 1, 1);
                }

                float4 col = tex2D(_MainTex, i.uv);

                // Katmanlı mask
                float f1 = mask(i.worldPos, _PainterPosition, _Radius * 0.9, _Hardness);
                float f2 = mask(i.worldPos, _PainterPosition, _Radius, _Hardness * 0.8);
                float f3 = mask(i.worldPos, _PainterPosition, _Radius * 1.1, _Hardness * 0.6);
                float f = max(f1, max(f2 * 0.7, f3 * 0.4));

                // Ek damlacıklar
                float2 offset1 = float2(0.3, -0.2);
                float2 offset2 = float2(-0.25, 0.1);
                float d1 = distance(i.worldPos.xz, _PainterPosition.xz + offset1 * _Radius);
                float d2 = distance(i.worldPos.xz, _PainterPosition.xz + offset2 * _Radius);
                float drop1 = 1 - smoothstep(0.0, 0.05 * _Radius, d1);
                float drop2 = 1 - smoothstep(0.0, 0.04 * _Radius, d2);

                float combined = max(f, max(drop1, drop2));
                float edge = pow(combined, 0.5) * _Strength;

                return lerp(col, _PainterColor, edge);
            }
            ENDCG
        }
    }
}
