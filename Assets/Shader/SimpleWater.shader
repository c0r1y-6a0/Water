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

        _NormalScale("Normal Cal Scale", Float) = 0.1

        [KeywordEnum(None, Simple, Ramp)] _Foam("Foam Mode", Float) = 0
        [Toggle(ENABLE_ANIMATION)] _Anim("Vertex Animation?", Float) = 0
        [Toggle(FLAT_SHADING)] _Flat("Flat Shading?", Float) = 0
        [Toggle(DEBUG_NORMAL)] _DebugNormal("Debug Normal?", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "LightMode"="ForwardBase"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Stencil{
            Ref 0
            Comp Equal
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

#pragma shader_feature ENABLE_ANIMATION
#pragma shader_feature FLAT_SHADING
#pragma shader_feature DEBUG_NORMAL
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
                float3 normal : VAR_NORMAL;
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
            float _MyTime;
            float _NormalScale;

            float _UVScale;
            float _GridSize;

            float3 anim(float2 uv, float3 vertex)
            {
                float4 noise = tex2Dlod(_NoiseTex, float4(uv, 0, 0));
                float co =  _MyTime * _WaveSpeed;
                co *= noise.r;
                float p1 = sin(2 * co) * _WaveAmp;
                vertex.x += p1;
                vertex.y += cos(co) * _WaveAmp;
                vertex.z += p1;
                return vertex;
            }

            v2f vert (appdata v)
            {
                v2f o;

#if ENABLE_ANIMATION
                v.vertex = float4(anim(v.texCoord.xy, v.vertex), 1);
#endif

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screen_pos = ComputeScreenPos(o.vertex);
                o.world_pos = mul(unity_ObjectToWorld, v.vertex);

                float3 originPos = v.vertex.xyz;

                float offset = _NormalScale * _GridSize;
                float2 tangentUVDiff =  float2(offset, 0) * _UVScale;
                float3 tangentVertexDiff = float3(offset, 0, 0);
                float3 tangentPos = anim(v.texCoord  + tangentUVDiff, v.vertex + tangentVertexDiff);

                float2 bitangentUVDiff = float2(0, offset) * _UVScale;
                float3 bitangentVertexDiff = float3(0, 0, offset);
                float3 bitangentPos = anim(v.texCoord + bitangentUVDiff, v.vertex + bitangentVertexDiff);

                float3 normal = -normalize(cross(tangentPos - originPos, bitangentPos - originPos));
                o.normal = UnityObjectToWorldNormal(normal);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
#if FLAT_SHADING
                float3 x_dif = ddx(i.world_pos).xyz;
                float3 y_dif = ddy(i.world_pos).xyz;
                float3 normal = -normalize(cross(x_dif, y_dif));
#else
                float3 normal = i.normal;
#endif
#if DEBUG_NORMAL
                return fixed4(normal, 1);
#endif

                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.world_pos.xyz));
                float3 refL = reflect(-worldViewDir, normal);

                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, refL);
                half3 _skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
                half4 skyColor = half4(_skyColor, 1);

                float nl = saturate(dot(normal, _WorldSpaceLightPos0.xyz));
                float3 diffuse = nl * _LightColor0;

#if _FOAM_NONE
                return _Color * nl + float4(ShadeSH9(half4(normal, 1)), 0) * 0.1 + skyColor * _RelectionIntensity;
#else
                float depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screen_pos).r;
                float viewZ = LinearEyeDepth(depth);
                float foamLine =  step(i.screen_pos.w, viewZ) * saturate(_EdgeWidth / (viewZ - i.screen_pos.w));
#if _FOAM_SIMPLE
                float4 col = _Color ;//+ (1 - foamLine) * _EdgeColor;;
#elif _FOAM_RAMP
                float4 foamRamp = tex2D(_DepthRampTex, float2(foamLine, 0.1));
                float4 col = _Color * foamRamp;
#endif
                return col * nl + float4(ShadeSH9(half4(normal, 1)), 0) * 0.1 + skyColor * _RelectionIntensity +  foamLine * _EdgeColor;
#endif
            }
            ENDHLSL
        }
    }
}
