using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed
{
    [System.Serializable, CreateAssetMenu(fileName = "TerrainSRPV2", menuName = "SRP/WinWeed/Create TerrainSRPV2", order = 2)]
    public class TerrainSRPV2 : ScriptableObject
    {
        public List<TerrainData> data;

        public int Count => data.Count;
        public Bounds Bounds;

        public void Save(Dictionary<Vector3Int, TerrainData> dict) {
            data.Clear();

            float top = 0, bottom =0, left = 0, right = 0, front = 0, back = 0;

            foreach (var keyPair in dict) {
                data.Add(keyPair.Value);

                var position = keyPair.Value.local_matrix.GetPosition();

                if (position.y > top) top = position.y;
                if (position.y < bottom) bottom = position.y;

                if (position.x > right) right = position.x;
                if (position.x < left) left = position.x;

                if (position.z > front) front = position.z;
                if (position.z < back) back = position.z;
            }

            Vector3 bound_size = new Vector3() {
                x = right - left,
                y = top - bottom,
                z = front - back
            };

            Vector3 bound_position = new Vector3();
            bound_position.y = bottom + (bound_size.y * 0.5f);
            bound_position.x = left + (bound_size.x * 0.5f);
            bound_position.z = back + (bound_size.z * 0.5f);


            Bounds.size = bound_size;
            Bounds.center = bound_position;
        }

        [System.Serializable]
        public struct TerrainData
        {
            public Matrix4x4 local_matrix;
            public Vector3 normal;
            public float strength;
        }
    }
}