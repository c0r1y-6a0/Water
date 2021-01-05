Shader "Water/SimpleWater"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeWidth("EdgeWidth", Float) = 1.0
        _DepthRampTex("Depth Ramp Texture", 2D) = "white"{}
        _NoiseTex("Noise Texture", 2D) = "white"{}
        _WaveSpeed("Wave Speed", Float) = 1.0
        _WaveAmp("Wave Amplifier", Float) = 1.0
        _RelectionIntensity("Relection Intensity", Range(0, 1)) = 0.3

        [KeywordEnum(None, Simple, Ramp)] _Foam("Foam Mode", Float) = 0
        [Toggle(ENABLE_ANIMATION)] _Anim("Vertex Animation?", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "LightMode"="ForwardBase"}
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

#pragma shader_feature ENABLE_ANIMATION
#pragma multi_compile _FOAM_NONE _FOAM_SIMPLE _FOAM_RAMP

            struct appdata
            {
                float4 vertex : POSITION;
                float4 texCoord: TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screen_pos : TEXCOORD1;
                float4 world_pos : TEXCOORD2;
            };

            sampler2D _CameraDepthTexture;
            sampler2D _DepthRampTex;
            sampler2D _NoiseTex;

            float4 _Color;

            float4 _EdgeColor;
            float _EdgeWidth;

            float _WaveSpeed;
            float _WaveAmp;

            float _RelectionIntensity;

            v2f vert (appdata v)
            {
                v2f o;

#if ENABLE_ANIMATION
                float4 noise = tex2Dlod(_NoiseTex, float4(v.texCoord.xy, 0, 0));
                float co = _Time * _WaveSpeed * noise;
                v.vertex.x += sin(co) * _WaveAmp;
                v.vertex.y += cos(co) * _WaveAmp;
#endif

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screen_pos = ComputeScreenPos(o.vertex);
                o.world_pos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 x_dif = ddx(i.world_pos).xyz;
                float3 y_dif = ddy(i.world_pos).xyz;
                float3 normal = -normalize(cross(x_dif, y_dif));

                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.world_pos.xyz));
                float3 refL = reflect(-worldViewDir, normal);

                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, refL);
                half3 _skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
                half4 skyColor = half4(_skyColor, 1);

                float nl = saturate(dot(normal, _WorldSpaceLightPos0.xyz));
                float3 diffuse = nl * _LightColor0;

#if _FOAM_NONE
                return _Color * nl + float4(ShadeSH9(half4(normal, 1)), 0) + skyColor * _RelectionIntensity;
#else
                float depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screen_pos).r;
                float viewZ = LinearEyeDepth(depth);
                float foamLine =  saturate(_EdgeWidth * (viewZ - i.screen_pos.w));
#if _FOAM_SIMPLE
                float4 col = _Color + (1 - foamLine) * _EdgeColor;;
#elif _FOAM_RAMP
                float4 foamRamp = tex2D(_DepthRampTex, float2(foamLine, 0.1));
                float4 col = _Color * foamRamp;
#endif
                return col * nl + float4(ShadeSH9(half4(normal, 1)), 0) + skyColor * _RelectionIntensity;
#endif
            }
            ENDHLSL
        }
    }
}
