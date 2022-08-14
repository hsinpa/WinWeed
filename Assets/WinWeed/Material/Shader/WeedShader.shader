Shader "Hsinpa/WeedShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct MeshProperties {
                float4x4 a_mat;

                float3 a_bezier_startpoint;
                float3 a_bezier_endpoint;
                float3 a_bezier_startctrl;
                float3 a_bezier_endctrl;
                float a_height;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

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

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;

                float t = v.vertex.y / _Properties[instanceID].a_height;
                v.vertex.y = 0;
                float4 new_vertex_pos = float4( BezierCurve(_Properties[instanceID].a_bezier_startpoint,
                                                    _Properties[instanceID].a_bezier_startctrl,
                                                    _Properties[instanceID].a_bezier_endpoint,
                                                    _Properties[instanceID].a_bezier_endctrl, t), 0);
                new_vertex_pos -= v.vertex;

                float4 pos = mul(_Properties[instanceID].a_mat, new_vertex_pos);

                o.vertex = UnityObjectToClipPos(pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
