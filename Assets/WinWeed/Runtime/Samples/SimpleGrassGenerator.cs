using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hsinpa.Winweed.Uti;

namespace Hsinpa.Winweed.Sample
{
    public class SimpleGrassGenerator : MonoBehaviour
    {

        [Header("Basic Config")]
        [SerializeField]
        private Material material;

        [SerializeField]
        private int spawnInstanceCount;

        [SerializeField]
        private float grass_height;

        [SerializeField, Range(0 , 1)]
        private float random_strength;

        [SerializeField]
        private TerrainSRP terrainSRP;

        [Header("Wind Config")]
        [SerializeField]
        private Vector3 wind_direction;

        [SerializeField]
        private float wind_strength;

        private GrassMesh _grassMesh;
        private const int SEGMENT = 2;

        GBezierCurve.GrassBezierPoint _grassBezierPoints;

        private MaterialPropertyBlock m_PropertyBlock;
        private Bounds m_bound;
        private Vector3 m_bound_size => new Vector3(terrainSRP.Terrain_Size.x, grass_height, terrainSRP.Terrain_Size.y);
        private Vector3 m_bound_position {
            get {
                var boundPosition = this.transform.position;
                boundPosition.y += grass_height * 0.5f;
                return boundPosition;
            }
        }

        private Mesh m_grassMesh;
        private ComputeBuffer m_argsCommandBuffer;
        private ComputeBuffer m_meshCommandBuffer;

        private struct MeshProperties
        {
            public Matrix4x4 a_mat;
            public Vector3 a_bezier_startpoint;
            public Vector3 a_bezier_endpoint;
            public Vector3 a_bezier_startctrl;
            public Vector3 a_bezier_endctrl;
            public float a_height;

            public static int Size()
            {
                return
                    (sizeof(float) * 4 * 4)+ // matrix;
                    (sizeof(float) * 3 * 4) + // bezier;
                    sizeof(float); // height;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            this.m_bound = new Bounds(m_bound_position, m_bound_size);
            //UtilityFunc.SetRandomSeed(System.DateTime.Now.Second);
            this.m_PropertyBlock = new MaterialPropertyBlock();

            this._grassMesh = new GrassMesh();

            this.m_grassMesh = this._grassMesh.CreateMesh(height: grass_height, width: 0.02f, sharpness: 0.3f, segment: SEGMENT);

            Vector3 spawnCenterPosition = m_bound.center;
            spawnCenterPosition.y -= grass_height * 0.5f;
            this.m_argsCommandBuffer = GetCommandShaderArg(this.m_grassMesh, spawnInstanceCount);
            this.m_meshCommandBuffer = GetCommandShaderMesh(spawnInstanceCount, spawnCenterPosition, grass_height);

            _grassBezierPoints = GBezierCurve.GenerateRandomCurve(height: grass_height, end_point_radius: 0.5f);

            this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Wind_Direction, wind_direction.normalized);
            this.m_PropertyBlock.SetFloat(WeedStatic.ShaderProperties.Wind_Strength, wind_strength);

            material.SetBuffer("_Properties", this.m_meshCommandBuffer);

            //_renderer.SetPropertyBlock(this.m_PropertyBlock);
        }

        private void Update()
        {
            var newBound = this.m_bound;
            Graphics.DrawMeshInstancedIndirect(this.m_grassMesh, 0, material, this.m_bound, this.m_argsCommandBuffer, properties: this.m_PropertyBlock, 
                                                castShadows: UnityEngine.Rendering.ShadowCastingMode.On);
        }

        private ComputeBuffer GetCommandShaderArg(Mesh grassMesh, int instance_count) {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            ComputeBuffer argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            args[0] = (uint)grassMesh.GetIndexCount(0);
            args[1] = (uint)instance_count;
            args[2] = (uint)grassMesh.GetIndexStart(0);
            args[3] = (uint)grassMesh.GetBaseVertex(0);

            argsBuffer.SetData(args);

            return argsBuffer;
        }

        private ComputeBuffer GetCommandShaderMesh(int instance_count, Vector3 spawn_center, float peak_height)
        {
            MeshProperties[] properties = new MeshProperties[instance_count];

            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;

            var paintStructList = terrainSRP.Terrains;

            for (int i = 0; i < instance_count; i++) {
                MeshProperties props = new MeshProperties();

                var random2DTuple = terrainSRP.GetRandom2DPosition(paintStructList, spawn_center);
                Vector2 random2DPos = random2DTuple.Item1;
                TerrainSRP.PaintedTerrainStruct paintedStruct = random2DTuple.Item2;

                float pos_x = random2DPos.x; //  (spawn_size.x  * 0.5f * UtilityFunc.RandomNegativeToOne()) + spawn_center.x;
                float pos_y = spawn_center.y;
                float pos_z = random2DPos.y; //(spawn_size.z * 0.5f * UtilityFunc.RandomNegativeToOne()) + spawn_center.z;
                float random_height_bias = (peak_height * (random_strength * 0.25f * UtilityFunc.Random()));

                //Debug.Log($"x {pos_x}, y {pos_y}, z {pos_z}");
                var grassBezierPoints = GBezierCurve.GenerateRandomCurve(height: peak_height, end_point_radius: 0.5f);

                Vector3 position = new Vector3(pos_x, pos_y, pos_z);

                rotation.SetLookRotation(new Vector3(Mathf.Sin(UtilityFunc.RandomNegativeToOne()), 0, Mathf.Cos(UtilityFunc.RandomNegativeToOne())));

                props.a_mat = Matrix4x4.TRS(position, rotation, scale);

                props.a_bezier_startpoint = grassBezierPoints.start_point;
                props.a_bezier_startctrl = grassBezierPoints.start_ctrl * paintedStruct.weight;
                props.a_bezier_endpoint = grassBezierPoints.end_point * paintedStruct.weight;
                props.a_bezier_endctrl = grassBezierPoints.end_ctrl * paintedStruct.weight;

                props.a_bezier_startctrl.y -= random_height_bias;
                props.a_bezier_endpoint.y -= random_height_bias;
                props.a_bezier_endctrl.y -= random_height_bias;

                props.a_height = peak_height;

                properties[i] = props;
            }

            ComputeBuffer meshPropertiesBuffer = new ComputeBuffer(instance_count, MeshProperties.Size());
            meshPropertiesBuffer.SetData(properties);
            return meshPropertiesBuffer;
        }

        private void OnDrawGizmosSelected()
        {
            var bound = new Bounds(m_bound_position, m_bound_size);

            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(bound.center, bound.size);
        }

        private void OnDisable()
        {
            if (this.m_meshCommandBuffer != null)
                this.m_meshCommandBuffer.Release();
            this.m_meshCommandBuffer = null;

            if (this.m_argsCommandBuffer != null)
                this.m_argsCommandBuffer.Release();
            this.m_argsCommandBuffer = null;
        }
    }
}