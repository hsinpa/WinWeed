using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.Winweed.Sample
{
    public class SimpleGrassGenerator : MonoBehaviour
    {

        [Header("Basic Config")]
        [SerializeField]
        private Material material;

        [SerializeField]
        private Vector3 boundingSize;

        [SerializeField]
        private int spawnInstanceCount;

        [SerializeField]
        private float grass_height;

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
            this.m_bound = new Bounds(this.transform.position, boundingSize);
            //UtilityFunc.SetRandomSeed(System.DateTime.Now.Second);
            this.m_PropertyBlock = new MaterialPropertyBlock();

            this._grassMesh = new GrassMesh();

            this.m_grassMesh = this._grassMesh.CreateMesh(height: grass_height, width: 0.05f, sharpness: 0.3f, segment: SEGMENT);

            this.m_argsCommandBuffer = GetCommandShaderArg(this.m_grassMesh, spawnInstanceCount);
            this.m_meshCommandBuffer = GetCommandShaderMesh(spawnInstanceCount, this.transform.position, boundingSize, grass_height);

            _grassBezierPoints = GBezierCurve.GenerateRandomCurve(height: grass_height, end_point_radius: 0.5f);

            this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Wind_Direction, wind_direction.normalized);
            this.m_PropertyBlock.SetFloat(WeedStatic.ShaderProperties.Wind_Strength, wind_strength);

            //this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Bezier_StartPoint, _grassBezierPoints.start_point);
            //this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Bezier_StartCtrl, _grassBezierPoints.start_ctrl);
            //this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Bezier_EndPoint, _grassBezierPoints.end_point);
            //this.m_PropertyBlock.SetVector(WeedStatic.ShaderProperties.Bezier_EndCtrl, _grassBezierPoints.end_ctrl);

            //Debug.Log($"Start Point {_grassBezierPoints.start_point.x}, {_grassBezierPoints.start_point.y}, {_grassBezierPoints.start_point.z}");
            //Debug.Log($"Start Ctrl {_grassBezierPoints.start_ctrl.x}, {_grassBezierPoints.start_ctrl.y}, {_grassBezierPoints.start_ctrl.z}");
            //Debug.Log($"End Point {_grassBezierPoints.end_point.x}, {_grassBezierPoints.end_point.y}, {_grassBezierPoints.end_point.z}");
            //Debug.Log($"End Ctrl {_grassBezierPoints.end_ctrl.x}, {_grassBezierPoints.end_ctrl.y}, {_grassBezierPoints.end_ctrl.z}");

            material.SetBuffer("_Properties", this.m_meshCommandBuffer);

            //_renderer.SetPropertyBlock(this.m_PropertyBlock);
        }

        private void Update()
        {
            Graphics.DrawMeshInstancedIndirect(this.m_grassMesh, 0, material, this.m_bound, this.m_argsCommandBuffer, properties: this.m_PropertyBlock);
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

        private ComputeBuffer GetCommandShaderMesh(int instance_count, Vector3 spawn_center, Vector3 spawn_size, float height)
        {
            MeshProperties[] properties = new MeshProperties[instance_count];

            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;

            for (int i = 0; i < instance_count; i++) {
                var grassBezierPoints = GBezierCurve.GenerateRandomCurve(height: height, end_point_radius: 0.5f);

                MeshProperties props = new MeshProperties();

                float pos_x = (spawn_size.x  * 0.5f * UtilityFunc.RandomNegativeToOne()) + spawn_center.x;
                float pos_y = spawn_center.y;
                float pos_z = (spawn_size.z * 0.5f * UtilityFunc.RandomNegativeToOne()) + spawn_center.z;

               // Debug.Log($"x {pos_x}, y {pos_y}, z {pos_z}");

                Vector3 position = new Vector3(pos_x, pos_y, pos_z);
                props.a_mat = Matrix4x4.TRS(position, rotation, scale);

                props.a_bezier_startpoint = grassBezierPoints.start_point;
                props.a_bezier_startctrl = grassBezierPoints.start_ctrl;
                props.a_bezier_endpoint = grassBezierPoints.end_point;
                props.a_bezier_endctrl = grassBezierPoints.end_ctrl;
                props.a_height = height;

                properties[i] = props;
            }

            ComputeBuffer meshPropertiesBuffer = new ComputeBuffer(instance_count, MeshProperties.Size());
            meshPropertiesBuffer.SetData(properties);
            return meshPropertiesBuffer;
        }

        private void OnDrawGizmosSelected()
        {
            //    float radius = 0.05f;
            //    Gizmos.color = Color.red;
            //    Gizmos.DrawSphere(_grassBezierPoints.start_point, radius);
            //    //Debug.Log($"Start Point {_grassBezierPoints.start_point.x}, {_grassBezierPoints.start_point.y}, {_grassBezierPoints.start_point.z}");

            //    Gizmos.color = Color.gray;
            //    Gizmos.DrawSphere(_grassBezierPoints.start_ctrl, radius);
            //    //Debug.Log($"Start Ctrl {_grassBezierPoints.start_ctrl.x}, {_grassBezierPoints.start_ctrl.y}, {_grassBezierPoints.start_ctrl.z}");

            //    Gizmos.color = Color.red;
            //    Gizmos.DrawSphere(_grassBezierPoints.end_point, radius);
            //    //Debug.Log($"End Point {_grassBezierPoints.end_point.x}, {_grassBezierPoints.end_point.y}, {_grassBezierPoints.end_point.z}");

            //    Gizmos.color = Color.gray;
            //    Gizmos.DrawSphere(_grassBezierPoints.end_ctrl, radius);
            //    //Debug.Log($"End Ctrl {_grassBezierPoints.end_ctrl.x}, {_grassBezierPoints.end_ctrl.y}, {_grassBezierPoints.end_ctrl.z}");

            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(this.transform.position, boundingSize);
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