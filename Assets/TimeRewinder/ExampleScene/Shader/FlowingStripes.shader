Shader "Custom/ScrollingStripes"
{
    Properties
    {
        _MainColor("Color", Color) = (1,1,1,1)
        _Speed("Speed", float) = 1.0
        _OwnTime("Time",float) =1
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainColor;
            float _Speed;
            float _OwnTime;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv + float2(0, _OwnTime * _Speed);
                float stripeValue = frac(uv.y * 20.0); // Generate a value that oscillates between 0 and 1

                // Adjust the value so that the black stripe is half as thick as the colored stripe
                float adjustedStripeValue = stripeValue < 0.5 ? 1.0 : 0.0;

                half4 color = lerp(half4(0,0,0,1), _MainColor, adjustedStripeValue);
                return color;
            }


            ENDCG
        }
    }
}