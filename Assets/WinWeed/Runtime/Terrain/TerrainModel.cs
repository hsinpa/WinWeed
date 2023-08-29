using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed.Terrain
{
    public class TerrainModel
    {
        public struct TerrainData {
            public Vector3 position;
            public Quaternion  rotation;
            public float strength;
        }

        private int _precision;
        public int DigitPrecision => _precision;

        private Dictionary<Vector3Int, TerrainData> dataset = new Dictionary<Vector3Int, TerrainData>();

        public TerrainModel(int digitPrecision)
        {
            this._precision = digitPrecision;
        }

        public void Insert(Vector3 position, Quaternion rotation, float strength) {
            
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
