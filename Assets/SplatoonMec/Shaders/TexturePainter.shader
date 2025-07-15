Shader "TNTC/TexturePainter"
{   
    Properties
    {
        _PainterColor ("Painter Color", Color) = (0, 0, 0, 0)
        _PainterBrush ("Painter Brush (Alpha)", 2D) = "white" {} // Yeni eklendi
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

            sampler2D _PainterBrush; // Yeni: Brush texture

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

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2(2, 2) - float2(1, 1));
                o.vertex = uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {   
                if (_PrepareUV > 0)
                {
                    return float4(0, 0, 1, 1);
                }

                float4 col = tex2D(_MainTex, i.uv);

                // XZ düzleminde worldPos ile brushUV hesapla
                float2 brushUV = (i.worldPos.xz - _PainterPosition.xz) / (_Radius * 4) + 0.5;

                // brushUV dışına çıkarsa boyama (görüntüyü kırmamak için)
                if (brushUV.x < 0 || brushUV.x > 1 || brushUV.y < 0 || brushUV.y > 1)
                    discard;

                // Brush'ın alpha kanalıyla etki miktarı
                float brushAlpha = tex2D(_PainterBrush, brushUV).a;

                // Hardness kullanılmak istenirse alpha * hardness * strength yapılabilir
                float edge = brushAlpha * _Strength;

                return lerp(col, _PainterColor, edge);
            }
            ENDCG
        }
    }
}
