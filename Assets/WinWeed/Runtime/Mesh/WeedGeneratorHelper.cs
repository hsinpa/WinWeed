using Hsinpa.Winweed.Uti;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading.Tasks;
using UnityEngine;


namespace Hsinpa.Winweed
{
    public class WeedGeneratorHelper 
    {
        private ComputeBuffer m_argsCommandBuffer;
        private ComputeBuffer m_meshCommandBuffer;

        private Mesh m_mesh;
        public Mesh Mesh => m_mesh;

        private GrassMesh m_grassMeshConstructor;
        private MaterialPropertyBlock m_PropertyBlock;
        private Material m_material;
        private Bounds m_bound;
        private int m_grass_segment;
        private WeedStatic.WindConfig m_windConfig;

        private MeshProperties[] m_meshProperties;
        private bool m_process_complete_flag = false;
        private System.Func<WeedStatic.PaintedWeedStruct> m_getRandomPointFunc;

        private KdTree.KdTree<float, Vector3Int> m_kdTree;

        public struct MeshProperties
        {
            public Matrix4x4 a_mat;
            public Vector3 a_bezier_startpoint;
            public Vector3 a_bezier_endpoint;
            public Vector3 a_bezier_startctrl;
            public Vector3 a_bezier_endctrl;
            public float a_height;

            public static int Size() {
                return
                    (sizeof(float) * 4 * 4) + // matrix;
                    (sizeof(float) * 3 * 4) + // bezier;
                    sizeof(float); // height;
            }
        }

        public WeedGeneratorHelper(
            Material material, Bounds bounds, int grass_segment, WeedStatic.WindConfig windConfig,
                                    System.Func<WeedStatic.PaintedWeedStruct> getRandomPointFunc) {

            this.m_grassMeshConstructor = new GrassMesh();
            this.m_PropertyBlock = new MaterialPropertyBlock();
            this.m_material = material;
            this.m_bound = bounds;
            this.m_grass_segment = grass_segment;
            this.m_windConfig = windConfig;
            this.m_getRandomPointFunc = getRandomPointFunc;
        }

        public void Render() {
            if (this.m_mesh == null) return;

            if (!m_process_complete_flag) {
                m_meshCommandBuffer.SetData(this.m_meshProperties);

                this.m_material.SetBuffer("_Properties", this.m_meshCommandBuffer);
            }

            Graphics.DrawMeshInstancedIndirect(this.m_mesh, 0, this.m_material, this.m_bound, this.m_argsCommandBuffer, properties: this.m_PropertyBlock,
                                castShadows: UnityEngine.Rendering.ShadowCastingMode.On);
        }


        public async void CreateGrassBufferData(int p_spawn_count, float p_grass_height, float p_grass_width, float p_grass_sharpness) {
            if (p_spawn_count <= 0) {
                return;
            }

            this.m_grassMeshConstructor = new GrassMesh();
            this.m_mesh = this.m_grassMeshConstructor.CreateMesh(height: p_grass_height, width: p_grass_width, sharpness: p_grass_sharpness, segment: this.m_grass_segment);
            this.m_meshProperties = new MeshProperties[p_spawn_count];

            Vector3 spawnCenterPosition = this.m_bound.center;
                    spawnCenterPosition.y -= p_grass_height * 0.5f;

            this.m_argsCommandBuffer = GetCommandShaderArg(this.m_mesh, p_spawn_count);

            this.m_meshCommandBuffer = new ComputeBuffer(p_spawn_count, MeshProperties.Size());
            m_meshCommandBuffer.SetData(this.m_meshProperties);

            GetCommandShaderMesh(p_spawn_count, p_grass_height, this.GetHashCode(), this.m_getRandomPointFunc);

            this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Wind_Direction, this.m_windConfig.wind_direction.normalized);
            this.m_PropertyBlock.SetFloat(WeedStatic.ShaderProperties.Wind_Strength, this.m_windConfig.wind_strength);
        }

        public static ComputeBuffer GetCommandShaderArg(Mesh grassMesh, int instance_count) {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            ComputeBuffer argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            args[0] = (uint)grassMesh.GetIndexCount(0);
            args[1] = (uint)instance_count;
            args[2] = (uint)grassMesh.GetIndexStart(0);
            args[3] = (uint)grassMesh.GetBaseVertex(0);

            argsBuffer.SetData(args);

            return argsBuffer;
        }

        public async void GetCommandShaderMesh(int instance_count, float peak_height,
                                         int seed, System.Func<WeedStatic.PaintedWeedStruct> GetRandomPointFunc) {
            if (instance_count <= 0) return;

            UtilityFunc.SetRandomSeed(seed);

            await Task.Run( () => {
                    Vector3 scale = Vector3.one;

                    var rangePartitioner = Partitioner.Create(0, instance_count, 500);
                    Parallel.ForEach(rangePartitioner, (range, loopState) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            MeshProperties props = this.m_meshProperties[i];

                            var paintedWeedStruct = GetRandomPointFunc();

                            float pos_x = paintedWeedStruct.position.x;
                            float pos_y = paintedWeedStruct.position.y;
                            float pos_z = paintedWeedStruct.position.z;

                            float random_height_bias = (peak_height * (paintedWeedStruct.weight * 0.25f * UtilityFunc.Random()));

                            var grassBezierPoints = GBezierCurve.GenerateRandomCurve(height: peak_height, end_point_radius: 0.5f * peak_height);

                            Vector3 position = new Vector3(pos_x, pos_y, pos_z);

                            Quaternion rotation = Quaternion.identity;
                            Vector3 grassFaceAt = new Vector3(Mathf.Sin(UtilityFunc.RandomNegativeToOne()), 0, Mathf.Cos(UtilityFunc.RandomNegativeToOne()));
                            rotation.SetLookRotation(grassFaceAt);

                            props.a_mat = Matrix4x4.TRS(position, rotation, scale * paintedWeedStruct.weight);

                            props.a_bezier_startpoint = grassBezierPoints.start_point;

                            props.a_bezier_startctrl = grassBezierPoints.start_ctrl;
                            props.a_bezier_endpoint = grassBezierPoints.end_point;
                            props.a_bezier_endctrl = grassBezierPoints.end_ctrl;

                            props.a_bezier_startctrl.y -= random_height_bias;
                            props.a_bezier_endpoint.y -= random_height_bias;
                            props.a_bezier_endctrl.y -= random_height_bias;

                            //Debug.Log($"x {props.a_bezier_endpoint.x}, y {props.a_bezier_endpoint.y}, z {props.a_bezier_endpoint.z}");
                            //Debug.Log($"peak_height {peak_height}");

                            props.a_height = peak_height;

                            this.m_meshProperties[i] = props;
                        }
                    });

            //        Parallel.For(0, instance_count, i => {
            //        MeshProperties props = this.m_meshProperties[i];

            //        var paintedWeedStruct = GetRandomPointFunc();

            //        float pos_x = paintedWeedStruct.position.x;
            //        float pos_y = paintedWeedStruct.position.y;
            //        float pos_z = paintedWeedStruct.position.z;

            //        float random_height_bias = (peak_height * (paintedWeedStruct.weight * 0.25f * UtilityFunc.Random()));

            //        var grassBezierPoints = GBezierCurve.GenerateRandomCurve(height: peak_height, end_point_radius: 0.5f * peak_height);

            //        Vector3 position = new Vector3(pos_x, pos_y, pos_z);

            //        Quaternion rotation = Quaternion.identity;
            //        Vector3 grassFaceAt = new Vector3(Mathf.Sin(UtilityFunc.RandomNegativeToOne()), 0, Mathf.Cos(UtilityFunc.RandomNegativeToOne()));
            //        rotation.SetLookRotation(grassFaceAt);

            //        props.a_mat = Matrix4x4.TRS(position, rotation, scale * paintedWeedStruct.weight);

            //        props.a_bezier_startpoint = grassBezierPoints.start_point;

            //        props.a_bezier_startctrl = grassBezierPoints.start_ctrl;
            //        props.a_bezier_endpoint = grassBezierPoints.end_point;
            //        props.a_bezier_endctrl = grassBezierPoints.end_ctrl;

            //        props.a_bezier_startctrl.y -= random_height_bias;
            //        props.a_bezier_endpoint.y -= random_height_bias;
            //        props.a_bezier_endctrl.y -= random_height_bias;

            //        //Debug.Log($"x {props.a_bezier_endpoint.x}, y {props.a_bezier_endpoint.y}, z {props.a_bezier_endpoint.z}");
            //        //Debug.Log($"peak_height {peak_height}");

            //        props.a_height = peak_height;

            //        this.m_meshProperties[i] = props;
            //    });
            });

            m_process_complete_flag = true;
        }

        public void Dispose() {
            if (this.m_meshCommandBuffer != null)
                this.m_meshCommandBuffer.Release();
            this.m_meshCommandBuffer = null;

            if (this.m_argsCommandBuffer != null)
                this.m_argsCommandBuffer.Release();
            this.m_argsCommandBuffer = null;
        }
    }
}