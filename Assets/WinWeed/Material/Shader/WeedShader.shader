Shader "Hsinpa/WeedShader"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _MainColor("Diffuse", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        LOD 100
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _MainColor;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

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
                float4 vertex : SV_POSITION;
                half3 normal        : TEXCOORD1;
                half3 viewDir: TEXCOORD2;
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
            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_MainTex);

            uniform half u_wind_strength;
            uniform half3 u_wind_direction;


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

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;

                //Calculate Wind effect
                half randam_factor = N21(half2(v.vertex.y + instanceID, instanceID + v.vertex.x + v.vertex.z));
                half3 wind_factor = (sin(_Time.z + randam_factor) * 0.05) * u_wind_direction * u_wind_strength;
                float3 bezier_startctrl = _Properties[instanceID].a_bezier_startctrl + (wind_factor * 0.5f);
                float3 bezier_endpoint = _Properties[instanceID].a_bezier_endpoint + (wind_factor);
                float3 bezier_endctrl = _Properties[instanceID].a_bezier_endctrl + (wind_factor);

                //Calculate bezier curve
                float t = v.vertex.y / _Properties[instanceID].a_height;
                v.vertex.y = 0;
                float4 bezier_vertex_pos = float4(BezierCurve(_Properties[instanceID].a_bezier_startpoint, bezier_startctrl, bezier_endpoint, bezier_endctrl, t), 0);
                bezier_vertex_pos -= v.vertex;

                float4 pos = mul(_Properties[instanceID].a_mat, bezier_vertex_pos);

                o.vertex = TransformObjectToHClip(pos.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.viewDir = GetCameraPositionWS() - pos.xyz;



                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                Light light = GetMainLight();
                float3 lightDirWS = light.direction;

                half3 texture_col = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _MainColor).xyz;


                return half4(texture_col, 1.0);
            }
            ENDHLSL
        }
    }
}
