using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed
{
    [System.Serializable, CreateAssetMenu(fileName = "TerrainSRPV2", menuName = "SRP/WinWeed/Create TerrainSRPV2", order = 2)]
    public class TerrainSRPV2 : ScriptableObject
    {
        public List<TerrainData> data = new List<TerrainData>();

        public int Count => data.Count;
        public Bounds Bounds;

        public void Save(Dictionary<Vector3Int, TerrainData> dict) {
            data.Clear();

            float top = 0, bottom =0, left = 0, right = 0, front = 0, back = 0;

            foreach (var keyPair in dict) {
                data.Add(keyPair.Value);

                if (keyPair.Key.y > top) top = keyPair.Key.y;
                if (keyPair.Key.y < bottom) bottom = keyPair.Key.y;

                if (keyPair.Key.x > right) right = keyPair.Key.x;
                if (keyPair.Key.x < left) left = keyPair.Key.x;

                if (keyPair.Key.z > front) front = keyPair.Key.z;
                if (keyPair.Key.z < back) back = keyPair.Key.z;
            }

            Vector3 size = new Vector3() {
                x = right - left,
                y = top - bottom,
                z = front - back
            };

            Bounds.size = size;
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