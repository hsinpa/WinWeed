using Codice.Client.BaseCommands.Merge;
using Hsinpa.Winweed.Uti;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading.Tasks;
using UnityEngine;
using static Hsinpa.Winweed.WeedStatic;

namespace Hsinpa.Winweed
{
    public class WeedGeneratorHelper 
    {
        private ComputeBuffer m_argsCommandBuffer;
        private ComputeBuffer m_meshCommandBuffer;

        private Dictionary<Vector3Int, CullBlockStruct> m_cullblock_dict;

        private Mesh m_mesh;
        public Mesh Mesh => m_mesh;

        private GrassMesh m_grassMeshConstructor;
        private MaterialPropertyBlock m_PropertyBlock;
        private Material m_material;
        private Bounds m_bound;

        private int m_grass_segment;
        private int m_cull_segment;

        private WeedStatic.WindConfig m_windConfig;

        private MeshProperties[] m_meshProperties;
        private bool m_process_complete_flag = false;
        private System.Func<WeedStatic.PaintedWeedStruct> m_getRandomPointFunc;

        private KdTree.KdTree<float, Vector3Int> m_kdTree;


        public WeedGeneratorHelper(
            Material material, Bounds bounds, int cull_segment, int grass_segment, WeedStatic.WindConfig windConfig,
                                    System.Func<WeedStatic.PaintedWeedStruct> getRandomPointFunc) {

            this.m_grassMeshConstructor = new GrassMesh();
            this.m_PropertyBlock = new MaterialPropertyBlock();
            this.m_material = material;
            this.m_bound = bounds;
            this.m_cull_segment = cull_segment;
            this.m_grass_segment = grass_segment;
            this.m_windConfig = windConfig;
            this.m_getRandomPointFunc = getRandomPointFunc;
        }

        public void Render() {
            if (this.m_mesh == null) return;

            if (!m_process_complete_flag) return;

            m_meshCommandBuffer.SetData(this.m_meshProperties);
            this.m_material.SetBuffer("_Properties", this.m_meshCommandBuffer);
            
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

            await GetCommandShaderMesh(p_spawn_count, p_grass_height, this.GetHashCode(), this.m_getRandomPointFunc);

            this.m_meshCommandBuffer = new ComputeBuffer(p_spawn_count, MeshProperties.Size());
            m_meshCommandBuffer.SetData(this.m_meshProperties);

            this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Wind_Direction, this.m_windConfig.wind_direction.normalized);
            this.m_PropertyBlock.SetFloat(WeedStatic.ShaderProperties.Wind_Strength, this.m_windConfig.wind_strength);

            m_process_complete_flag = true;
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

        public async Task GetCommandShaderMesh(int instance_count, float peak_height,
                                         int seed, System.Func<WeedStatic.PaintedWeedStruct> GetRandomPointFunc) {
            if (instance_count <= 0) return;

            UtilityFunc.SetRandomSeed(seed);
            Vector3 scale = Vector3.one;

            Dictionary<Vector3Int, CullBlockStruct> cullblock_dict = new Dictionary<Vector3Int, CullBlockStruct>();

            await Task.Run( () => {
                //var rangePartitioner = Partitioner.Create(0, instance_count, 500);
                //Parallel.ForEach(rangePartitioner, (range, loopState) =>
                //{
                //    for (int i = range.Item1; i < range.Item2; i++) {

                    for (int i = 0; i < instance_count; i++) {
                    MeshProperties props = new MeshProperties();

                    var paintedWeedStruct = GetRandomPointFunc();

                    float pos_x = paintedWeedStruct.position.x;
                    float pos_y = paintedWeedStruct.position.y;
                    float pos_z = paintedWeedStruct.position.z;


                    //Debug.Log($"Pos X {pos_x}, Pos Y {pos_y}, Pos Z {pos_z}");

                    int mod_x = (int) System.Math.Round(pos_x % m_cull_segment, 0, System.MidpointRounding.AwayFromZero);
                    int start_point_x = Mathf.RoundToInt(pos_x / m_cull_segment);
                    int start_point_y = Mathf.RoundToInt(pos_y / m_cull_segment);
                    int start_point_z = Mathf.RoundToInt(pos_z / m_cull_segment);
                    Vector3Int key = new Vector3Int(start_point_x, start_point_y, start_point_z);

                    if (!cullblock_dict.ContainsKey(key)) cullblock_dict[key] = new CullBlockStruct();
                    CullBlockStruct cullBlockStruct;
                    cullblock_dict.TryGetValue(new Vector3Int(start_point_x, start_point_y, start_point_z), out cullBlockStruct);

                    cullBlockStruct.block_position = key;
                    cullBlockStruct.block_size = m_cull_segment;

                    //Debug.Log($"Mod X {mod_x}, StartPointX {start_point_x}");

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

                    cullBlockStruct.mesh_properties_list.Add(props);
                }
            });
        }

        public void Dispose() {
            Debug.Log("m_weedGeneratorHelper Dispose");
            if (this.m_meshCommandBuffer != null)
                this.m_meshCommandBuffer.Release();
            this.m_meshCommandBuffer = null;

            if (this.m_argsCommandBuffer != null)
                this.m_argsCommandBuffer.Release();
            this.m_argsCommandBuffer = null;
        }
    }
}