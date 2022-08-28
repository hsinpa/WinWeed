using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hsinpa.Winweed.Uti;
using System.Threading.Tasks;

namespace Hsinpa.Winweed
{
    [RequireComponent(typeof(WeedTerrainBuilder)), ExecuteInEditMode]
    public class SimpleWeedGenerator : MonoBehaviour
    {
        [Header("Basic Config")]
        [SerializeField]
        private Material material;
        private Material _material;

        [SerializeField]
        private int spawnInstanceCount;
        private int dynamicInstanceCount;

        [SerializeField]
        private float grass_height = 1;

        [SerializeField]
        private float grass_width = 0.02f;

        [SerializeField]
        private float grass_sharpness = 0.3f;

        [SerializeField, Range(0 , 1)]
        private float random_strength;

        private WeedTerrainBuilder _terrain;
        public WeedTerrainBuilder Terrain { get {
                if (_terrain != null) return _terrain;

                _terrain = GetComponent<WeedTerrainBuilder>();

                return _terrain;
            } }

        [Header("Wind Config")]
        [SerializeField]
        private Vector3 wind_direction;

        [SerializeField]
        private float wind_strength;
        
        private const int SEGMENT = 2;

        private MaterialPropertyBlock m_PropertyBlock;
        private Bounds m_bound;
        private Vector3 m_bound_size => new Vector3(Terrain.DataSRP.Size.x, grass_height, Terrain.DataSRP.Size.y);
        private Vector3 m_bound_position {
            get {
                var boundPosition = this.transform.position;
                boundPosition.y += grass_height * 0.5f;
                return boundPosition;
            }
        }
        public Bounds Bounds => m_bound;
        private GrassMesh m_grassMeshConstructor;

        private Mesh m_mesh;
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

        private bool PreviewMode => Terrain.editor_state == WeedTerrainBuilder.State.Preview && Application.isEditor;
        private bool m_is_cull = false;
        public bool IsCull => m_is_cull;

        private int m_lod_level = 0; // Lower is better
        public int LOD_Level => m_lod_level;

        private bool allow_create_buffer_flag = true; //Prevent multithread weird behavior
        private bool Allow_create_buffer_flag => allow_create_buffer_flag || Application.isEditor; //Prevent multithread weird behavior

        #region Public API
        public void ReDraw(int p_instance_count = -1)
        {
            if (!Allow_create_buffer_flag) return;

            if (p_instance_count <= 0)
                p_instance_count = spawnInstanceCount;

            if (this.m_PropertyBlock == null)
                this.m_PropertyBlock = new MaterialPropertyBlock();

            Dispose();
            CreateGrassBufferData(p_spawn_count: p_instance_count, p_grass_height: grass_height, p_grass_width: grass_width, p_grass_sharpness: grass_sharpness);
        }

        public void SetCulling(bool is_cull)
        {
            this.m_is_cull = is_cull;
        }

        public void SetLOD(int lod, float lod_weight) {
            this.m_lod_level = lod;

            dynamicInstanceCount = Mathf.FloorToInt(spawnInstanceCount * lod_weight);
            if (dynamicInstanceCount > 0)
                ReDraw(dynamicInstanceCount);
        }

        public void Dispose()
        {
            if (this.m_meshCommandBuffer != null)
                this.m_meshCommandBuffer.Release();
            this.m_meshCommandBuffer = null;

            if (this.m_argsCommandBuffer != null)
                this.m_argsCommandBuffer.Release();
            this.m_argsCommandBuffer = null;

            if (this.m_mesh != null) {
                this.m_mesh.Clear();
                UtilityFunc.DeleteObject(this.m_mesh);
                this.m_mesh = null;
            }
        }
        #endregion

        #region Monobehavior
        // Start is called before the first frame update
        void Start()
        {
            this.m_PropertyBlock = new MaterialPropertyBlock();

            dynamicInstanceCount = spawnInstanceCount;
            ReDraw(dynamicInstanceCount);
        }

        private void Update()
        {
            bool allow_draw = ((Application.isPlaying && !m_is_cull && dynamicInstanceCount > 0) || (PreviewMode));

            if (this.m_argsCommandBuffer == null) {
                ReDraw(dynamicInstanceCount);
                return;
            }

            if (allow_draw)
            {
                Graphics.DrawMeshInstancedIndirect(this.m_mesh, 0, this._material, this.m_bound, this.m_argsCommandBuffer, properties: this.m_PropertyBlock,
                                                castShadows: UnityEngine.Rendering.ShadowCastingMode.On);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var bound = new Bounds(m_bound_position, m_bound_size);

            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(bound.center, bound.size);
        }

        private void OnDisable()
        {
            Dispose();
        }
        #endregion

        #region Private API

        private async void CreateGrassBufferData(int p_spawn_count, float p_grass_height, float p_grass_width, float p_grass_sharpness) {
            if (p_spawn_count <= 0) {
                return;
            }

            allow_create_buffer_flag = false;

            this._material = (PreviewMode) ? material : new Material(material);
            this.m_bound = new Bounds(m_bound_position, m_bound_size);


            this.m_grassMeshConstructor = new GrassMesh();
            this.m_mesh = this.m_grassMeshConstructor.CreateMesh(height: p_grass_height, width: p_grass_width, sharpness: p_grass_sharpness, segment: SEGMENT);

            Vector3 spawnCenterPosition = m_bound.center;
            spawnCenterPosition.y -= grass_height * 0.5f;

            this.m_argsCommandBuffer = GetCommandShaderArg(this.m_mesh, p_spawn_count);

            this.m_meshCommandBuffer = new ComputeBuffer(p_spawn_count, MeshProperties.Size());

            MeshProperties[] meshProperties = await GetCommandShaderMesh(p_spawn_count, spawnCenterPosition, grass_height);

            m_meshCommandBuffer.SetData(meshProperties);

            this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Wind_Direction, wind_direction.normalized);
            this.m_PropertyBlock.SetFloat(WeedStatic.ShaderProperties.Wind_Strength, wind_strength);

            this._material.SetBuffer("_Properties", this.m_meshCommandBuffer);

            allow_create_buffer_flag = true;
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

        private async Task<MeshProperties[]> GetCommandShaderMesh(int instance_count, Vector3 spawn_center, float peak_height)
        {
            MeshProperties[]  properties = new MeshProperties[instance_count];

            var paintStructList = Terrain.DataSRP.Terrains;
            if (paintStructList.Count <= 0) return properties;
            int seed_offset = (int)(gameObject.GetInstanceID() + Time.time);

            await Task.Run(() =>
            {
                Vector3 scale = Vector3.one;

                Parallel.For(0, instance_count, i => {
                    UtilityFunc.SetRandomSeed(seed_offset + i);
                    MeshProperties props = new MeshProperties();

                    var random2DTuple = Terrain.DataSRP.GetRandom2DPosition(paintStructList, spawn_center);
                    Vector2 random2DPos = random2DTuple.Item1;
                    TerrainSRP.PaintedTerrainStruct paintedStruct = random2DTuple.Item2;

                    float pos_x = random2DPos.x; //  (spawn_size.x  * 0.5f * UtilityFunc.RandomNegativeToOne()) + spawn_center.x;
                    float pos_y = spawn_center.y;
                    float pos_z = random2DPos.y; //(spawn_size.z * 0.5f * UtilityFunc.RandomNegativeToOne()) + spawn_center.z;

                    float random_height_bias = (peak_height * (random_strength * 0.25f * UtilityFunc.Random()));

                    //Debug.Log($"x {pos_x}, y {pos_y}, z {pos_z}");
                    var grassBezierPoints = GBezierCurve.GenerateRandomCurve(height: peak_height, end_point_radius: 0.5f * peak_height);

                    Vector3 position = new Vector3(pos_x, pos_y, pos_z);

                    Quaternion rotation = Quaternion.identity;
                    Vector3 grassFaceAt = new Vector3(Mathf.Sin(UtilityFunc.RandomNegativeToOne()), 0, Mathf.Cos(UtilityFunc.RandomNegativeToOne()));
                    rotation.SetLookRotation(grassFaceAt);

                    props.a_mat = Matrix4x4.TRS(position, rotation, scale * paintedStruct.weight);

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

                    properties[i] = props;
                });
            });

            return properties;
        }

        #endregion
    }
}