using UnityEngine;
using Hsinpa.Winweed.Uti;

namespace Hsinpa.Winweed
{
    public class GrassMesh
    {

        //Config
        private MeshStruct m_meshStruct;
        private float m_height;
        private float m_width;
        private float m_width_radius;

        private int m_segment;

        private const int VERTICE_BASE = 2;
        private const int TRI_PART = 3;

        //private float height_plus_tri => this.m_height + (0.2f * this.m_height);

        public GrassMesh() {
            this.m_meshStruct = new MeshStruct();
        }

        public Mesh CreateMesh(float height, float width, float sharpness, int segment) {
            Mesh mesh = new Mesh();

            this.m_segment = segment;
            this.m_width = width;
            this.m_height = height;
            this.m_width_radius = width * 0.5f;

            this.m_meshStruct = CreateTriVertice(this.m_meshStruct, height, width, sharpness, segment);

            mesh.SetVertices(this.m_meshStruct.vertices);
            mesh.SetIndices(this.m_meshStruct.triangles, MeshTopology.Triangles, 0);
            mesh.SetUVs(0, CreateUV(this.m_meshStruct.vertices, floor: 0, top: height, left: -this.m_width_radius, right: this.m_width_radius));
            mesh.RecalculateNormals();
            return mesh;
        }

        private MeshStruct CreateTriVertice(MeshStruct meshStruct, float height, float width, float sharpness, int segment)
        {
            float blade_height = (sharpness * this.m_height);
            float body_height = height - blade_height;

            float segment_height = body_height / (segment);
            float width_radius = width * 0.5f;

            int verticeLens = (VERTICE_BASE + (VERTICE_BASE * segment)) + 1; //2 => base, 2 => body increment, 1 => top triangle

            Vector3[] vertices = new Vector3[verticeLens];

            int triangleLens = ((TRI_PART * 2 * segment)) + TRI_PART; //Square is form with two triangles

            int[] triangles = new int[triangleLens];

            vertices[0] = new Vector3(-width_radius, 0, 0);
            vertices[1] = new Vector3(width_radius, 0, 0);

            //Build body
            for (int i = 0; i < segment; i++) {

                int A_Index = 0 + (i * VERTICE_BASE);
                int B_Index = 1 + (i * VERTICE_BASE);
                int C_Index = 2 + (i * VERTICE_BASE);
                int D_Index = 3 + (i * VERTICE_BASE);

                vertices[C_Index] = new Vector3(-width_radius, segment_height * (i + 1), 0);
                vertices[D_Index] = new Vector3(width_radius, segment_height * (i + 1), 0);

                triangles[0 + (i * 6)] = A_Index;
                triangles[1 + (i * 6)] = B_Index;
                triangles[2 + (i * 6)] = C_Index;

                triangles[3 + (i * 6)] = B_Index;
                triangles[4 + (i * 6)] = D_Index;
                triangles[5 + (i * 6)] = C_Index;
            }

            //Build head
            vertices[verticeLens - 1] = new Vector3(0, body_height + blade_height, 0);

            triangles[triangleLens - 3] = verticeLens - 3;
            triangles[triangleLens - 2] = verticeLens - 2;
            triangles[triangleLens - 1] = verticeLens - 1;

            //Debug.Log("Vertice");
            //DebugArray(vertices);

            //Debug.Log("Triangle");
            //DebugArray(triangles);

            meshStruct.vertices = vertices;
            meshStruct.triangles = triangles;

            return meshStruct;
        }

        private Vector2[] CreateUV(Vector3[] vertices, float floor, float top, float left, float right)
        {
            Vector2[] uv_array = new Vector2[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                uv_array[i] = new Vector2( UtilityFunc.NormalizeByRange(vertices[i].x, left, right),
                                           UtilityFunc.NormalizeByRange(vertices[i].y, floor, top)
                    );
            }

            return uv_array;
        }

        private Vector3[] CreateNormal(Vector3[] vertices) {
            Vector3[] normal_array = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                normal_array[i] = new Vector3(0, 0, -1);
            }

            return normal_array;
        }

        public struct MeshStruct {
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uv;
            public Vector3[] normal;
        }

        private void DebugArray<T>(T[] array) {
            foreach (T a in array)
                Debug.Log(a.ToString());
        }
    }
}
