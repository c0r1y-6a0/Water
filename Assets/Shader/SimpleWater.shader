Shader "Water/SimpleWater"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeWidth("EdgeWidth", Float) = 1.0
        _DepthRampTex("Depth Ramp Texture", 2D) = "white"{}

        [KeywordEnum(None, Simple, Ramp)] _Foam("Foam Mode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

#pragma multi_compile _FOAM_NONE _FOAM_SIMPLE _FOAM_RAMP

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
            sampler2D _DepthRampTex;
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

            fixed4 frag(v2f i) : SV_Target
            {
#if _FOAM_NONE
                return _Color;
#else
                float4 depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screen_pos);
                float viewZ = LinearEyeDepth(depth);
                float foamLine =  saturate(_EdgeWidth * (viewZ - i.screen_pos.w));
#if _FOAM_SIMPLE
                float4 col = _Color + (1 - foamLine) * _EdgeColor;;
#elif _FOAM_RAMP
                float4 foamRamp = tex2D(_DepthRampTex, float2(foamLine, 0.1));
                float4 col = _Color * foamRamp;
#endif
                return col;
#endif
                /*
#if ENABLE_FOAM
                float4 depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screen_pos);
                float viewZ = LinearEyeDepth(depth);
                float foamLine =  saturate(_EdgeWidth * (viewZ - i.screen_pos.w));
                //float4 col = _Color + (1 - foamLine )* _EdgeColor;
                float4 foamRamp = tex2D(_DepthRampTex, float2(foamLine, 0.1));
                float4 col = _Color + (1 - foamLine) * _EdgeColor;;

                return col;
#else
#endif
*/
            }
            ENDHLSL
        }
    }
}
