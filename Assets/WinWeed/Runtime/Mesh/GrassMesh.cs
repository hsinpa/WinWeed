using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.Winweed {
    public class GrassMesh
    {

        //Config
        private MeshStruct m_meshStruct;
        private float m_height;
        private float m_width;

        private int m_subdivide;

        public GrassMesh() {
            this.m_meshStruct = new MeshStruct();
        }

        public Mesh CreateMesh(float height, float width, int subdivide) {
            Mesh m = new Mesh();

            this.m_subdivide = subdivide;
            this.m_width = width;
            this.m_height = height;

            this.m_meshStruct = CreateTriVertice(this.m_meshStruct, height, width, subdivide);

            return m;
        }

        private MeshStruct CreateTriVertice(MeshStruct meshStruct, float height, float width, int subdivide) {

            Vector3 foot = new Vector3(0, 0, 0);
            Vector3 top = new Vector3(0, height, 0);
            float segment_length = height / subdivide;
            float width_radius = width * 0.5f;


            int verticeLens = (4 + (2 * subdivide)) + 1; //4 => base, 2 => body increment, 1 => top triangle
            Vector3[] vertices = new Vector3[verticeLens];

            int triangleLens = (6 + (6 * subdivide)) + 3; //4 => base, 2 => body increment, 1 => top triangle

            int[] triangles = new int[0];

            vertices[0] = new Vector3(-width_radius, 0, 0);
            vertices[1] = new Vector3(width_radius, 0, 0);

            //Build body
            for (int i = 0; i < subdivide; i++) {

                Vector3 A = vertices[0 + (i * 2)];

                Vector3 B = vertices[1 + (i * 2)];

                Vector3 C = new Vector3(-width_radius, segment_length * (i +1), 0);
                
                Vector3 D = new Vector3(width_radius, segment_length * (i + 1), 0);

                vertices[2 + (i * 2)] = C;
                vertices[3 + (i * 2)] = D;

                triangles[0 + (i * 6)] = 0;
                triangles[1 + (i * 6)] = 1;
                triangles[2 + (i * 6)] = 2;

                triangles[3 + (i * 6)] = 3;
                triangles[4 + (i * 6)] = 4;
                triangles[5 + (i * 6)] = 5;

            }


            //Build head

            return meshStruct;
        }

        private void CreateUV()
        {

        }

        private void CreateNormal() { 
        
        }

        public struct MeshStruct {
            public Vector3[] position;
            public int[] triangle;
            public Vector2[] uv;
            public Vector3[] normal;
        }

    }
}
