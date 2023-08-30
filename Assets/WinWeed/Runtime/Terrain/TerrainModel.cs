using Hsinpa.Winweed.Uti;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed.Terrain
{
    public class TerrainModel
    {
        [System.Serializable]
        public struct TerrainData {
            public Vector3 position;
            public Vector3 rotation;
            public float strength;
        }


        [System.Serializable]
        public struct TerrainTransform
        {
            Matrix4x4 localToWorldMatrice;
        }

        private int _precision;
        public int DigitPrecision => _precision;

        private Dictionary<Vector3Int, TerrainData> dataset = new Dictionary<Vector3Int, TerrainData>();

        public TerrainModel(
            Transform parentTransform,
            LayerMask layers,
            int digitPrecision)
        {
            this._precision = digitPrecision;
        }



        public void Insert(Vector3 position, Vector3 rotation, float strength) {
            Vector3Int vector_key = TransformPosition(position);

            Debug.Log($"Insert Key {vector_key}");

            if (dataset.TryGetValue(vector_key, out TerrainData p_terrainData)) {
                p_terrainData.rotation = Vector3.Lerp(p_terrainData.rotation, rotation, 0.5f);
                p_terrainData.strength = Mathf.Clamp(p_terrainData.strength + strength, 0, 1);

                UtilityFunc.SetDictionary(dataset, vector_key, p_terrainData);
                return;
            }

            dataset.Add(vector_key, new TerrainData() {
                position = position,
                rotation = rotation,
                strength = strength
            });

            Debug.Log($"Model Count {dataset.Count}");

        }



        private Vector3Int TransformPosition(Vector3 position) {
            var vectorInt =  new Vector3Int();

            //Round down
            position.x = (float)System.Math.Round(position.x, _precision, System.MidpointRounding.AwayFromZero);
            position.y = (float)System.Math.Round(position.y, _precision, System.MidpointRounding.AwayFromZero);
            position.z = (float)System.Math.Round(position.z, _precision, System.MidpointRounding.AwayFromZero);

            //To Int
            double scale = Mathf.Pow(10, _precision);

            position.x = (float)(position.x * scale);
            position.y = (float)(position.y * scale);
            position.z = (float)(position.z * scale);

            vectorInt.Set((int)position.x, (int)position.y, (int)position.z);

            return vectorInt;
        }


    }
}
