Shader "Hsinpa/WeedShader"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _MainColor("Diffuse", Color) = (1, 1, 1, 1)

        _TraceMaskTex("TraceMask Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        LOD 100
        Cull Off
        ZWrite On
        ZTest On

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _MainColor;

            float4 _TraceMaskTex_ST;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma target 5.0

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal        : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex_CS : SV_POSITION; //Clip space
                float3 vertex_WS : TEXCOORD1; // World space
                half3 normal        : TEXCOORD2;
                half3 viewDir: TEXCOORD3;
            };

            struct MeshProperties {
                float4x4 a_mat;

                float3 a_bezier_startpoint;
                float3 a_bezier_endpoint;
                float3 a_bezier_startctrl;
                float3 a_bezier_endctrl;
                float a_height;
            };
            
            // This macro declares _BaseMap as a Texture2D object.
            TEXTURE2D(_MainTex);
            //TEXTURE2D(_CameraDepthTexture);

            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_TraceMaskTex);
            //SAMPLER(sampler_CameraDepthTexture);

            sampler2D _TraceMaskTex;

            uniform half u_wind_strength;
            uniform half3 u_wind_direction;

            uniform half2 u_terrain_bottom_left_point;
            uniform half2 u_terrain_top_right_point;

            StructuredBuffer<MeshProperties> _Properties;

            float3 BezierCurve(float3 StartPoint, float3 StartCtrlPoint, float3 EndPoint, float3 EndCtrlPoint, float t)
            {
                float3 PS = lerp(StartPoint, StartCtrlPoint, t);
                float3 PE = lerp(EndCtrlPoint, EndPoint, t);

                float3 CC = lerp(StartCtrlPoint, EndCtrlPoint, t);

                float3 SC = lerp(PS, CC, t);
                float3 EC = lerp(CC, PE, t);

                return lerp(SC, EC, t);
            }

            half N21(half2 p) {
                p = frac(p * half2(233.34, 851.73));
                p += dot(p, p + 23.45);
                return frac(p.x + p.y);
            }

            half FindTraceWeight(half3 world_position) {
                half terrain_width = u_terrain_top_right_point.x - u_terrain_bottom_left_point.x;
                half terrain_height = u_terrain_top_right_point.y - u_terrain_bottom_left_point.y;

                half uv_x = 1.0 - ( (u_terrain_top_right_point.x - world_position.x) / terrain_width );
                half uv_y = 1.0 - ((u_terrain_top_right_point.y - world_position.z) / terrain_height);
                //return 0;
                return tex2Dlod(_TraceMaskTex, float4(uv_x, uv_y, 0.0, 0.0)).r;
            }

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;

                half4 absolute_pos = mul(_Properties[instanceID].a_mat, v.vertex);

                //Calculate Wind effect
                half randam_factor = N21(half2(v.vertex.y + instanceID, instanceID + v.vertex.x + v.vertex.z));
                half3 wind_factor = (sin(_Time.z + randam_factor) * 0.05) * u_wind_direction * u_wind_strength;
                float3 bezier_startctrl = _Properties[instanceID].a_bezier_startctrl + (wind_factor * 0.5f);

                float3 bezier_endpoint = _Properties[instanceID].a_bezier_endpoint + (wind_factor);
                float3 bezier_endctrl = _Properties[instanceID].a_bezier_endctrl + (wind_factor);

                //Calculate bezier curve
                float t = v.vertex.y / _Properties[instanceID].a_height;
                float4 bezier_vertex_pos = float4(BezierCurve(_Properties[instanceID].a_bezier_startpoint, bezier_startctrl, bezier_endpoint, bezier_endctrl, t), 0);
                float4 offset = bezier_vertex_pos - v.vertex;
                v.vertex -= offset;

                //Trace Weight
                //half trace_weight = FindTraceWeight(absolute_pos.xyz);
                half trace_weight = 0.0;

                half3 fall_direction = (_Properties[instanceID].a_bezier_endctrl - _Properties[instanceID].a_bezier_endpoint);
                half fall_length = length(fall_direction);
                      fall_direction = normalize(fall_direction * t) * fall_length * 2;

                half original_y = v.vertex.y;
                v.vertex.xyz += (fall_direction * trace_weight);
                v.vertex.y = original_y - (trace_weight * (v.vertex.y * 0.5));
                v.vertex.w = 1;

                float4 pos = mul(_Properties[instanceID].a_mat, v.vertex);
                float3 world_normal = mul((float3x3)_Properties[instanceID].a_mat, v.normal);

                o.vertex_CS = TransformWorldToHClip(pos.xyz);
                o.vertex_WS = pos.xyz;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = normalize(world_normal);
                o.viewDir = GetCameraPositionWS() - pos.xyz;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {

                //Light light = GetMainLight();
                //float3 lightDirWS = light.direction;
                //float lightWeight = dot(-lightDirWS, i.normal);

                float4 diffuseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                //float4 depthColor = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);

                float4 shadowCoord = TransformWorldToShadowCoord(i.vertex_WS);
                Light mainLight = GetMainLight(shadowCoord);
                float3 lightDir = mainLight.direction;
                float3 lightColor = mainLight.color;
                float3 normalWS = i.normal;

                float4 color = float4(1, 1, 1, 1);
                float minDotLN = 0.2;

                float3 ambientLight = lightColor * 0.1;

                color.rgb = max(minDotLN, abs(dot(lightDir, normalWS))) * lightColor * diffuseColor.rgb * _MainColor.rgb * mainLight.shadowAttenuation;
                color.rgb += ambientLight;
                //color.rgb = depthColor.rgb;
                //half3 texture_col = ((SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _MainColor).xyz * light.color * lightWeight) + ambientLight;

                return color;
            }
            ENDHLSL
        }
    }
}
