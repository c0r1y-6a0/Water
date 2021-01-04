Shader "Water/SimpleWater"
{
    Properties
    {
          // color of the water
        _Color("Color", Color) = (1, 1, 1, 1)
        // color of the edge effect
        _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
        // width of the edge effect
        _EdgeWidth("EdgeWidth", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screen_pos : TEXCOORD1;
            };

            sampler2D _CameraDepthTexture;
            float4 _Color;
            float4 _EdgeColor;
            float _EdgeWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screen_pos = ComputeScreenPos(o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screen_pos);
                float viewZ = LinearEyeDepth(depth);
                float foamLine = 1 - saturate(_EdgeWidth * (viewZ - i.screen_pos.w));
                float4 col = _Color + foamLine * _EdgeColor;

                return col;
            }
            ENDHLSL
        }
    }
}
