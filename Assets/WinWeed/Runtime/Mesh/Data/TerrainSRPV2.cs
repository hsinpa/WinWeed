using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed
{
    [System.Serializable, CreateAssetMenu(fileName = "TerrainSRPV2", menuName = "SRP/WinWeed/Create TerrainSRPV2", order = 2)]
    public class TerrainSRPV2 : ScriptableObject
    {
        public List<TerrainData> data = new List<TerrainData>();

        public void Save(Dictionary<Vector3Int, TerrainData> dict) {
            data.Clear();

            foreach (var keyPair in dict) {
                data.Add(keyPair.Value);
            }
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