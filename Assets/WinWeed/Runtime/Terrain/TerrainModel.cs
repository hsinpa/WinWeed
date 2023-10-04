using Hsinpa.Winweed.Uti;
using KdTree;
using KdTree.Math;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UnityEngine;

namespace Hsinpa.Winweed.Terrain
{
    public class TerrainModel
    {
        private int _precision;
        public int DigitPrecision => _precision;

        private Transform _parentTransform;

        private Dictionary<Vector3Int, TerrainSRPV2.TerrainData> dataset = new Dictionary<Vector3Int, TerrainSRPV2.TerrainData>();
        public Dictionary<Vector3Int, TerrainSRPV2.TerrainData>  DataSet => dataset;

        private KdTree.KdTree<float, Vector3Int> kdTree = new KdTree<float, Vector3Int>(3, new FloatMath());
        public KdTree.KdTree<float, Vector3Int> KDTree => kdTree;

        public TerrainModel(
            Transform parentTransform,
            LayerMask layers,
            int digitPrecision)
        {
            this._parentTransform = parentTransform;
            this._precision = digitPrecision;
        }

        public void Load(List<TerrainSRPV2.TerrainData> terrainDataList) {
            if (terrainDataList == null) return;


            int data_count = terrainDataList.Count;

            for (int i = 0; i < data_count; i++) {
                UtilityFunc.SetDictionary(dataset, VectorKeyPosition(terrainDataList[i].local_matrix.GetPosition()), terrainDataList[i]);
            }
        }

        public void Insert(Vector3 position, Vector3 rotation, float strength) {
            Matrix4x4 matrix4X4 = Matrix4x4.TRS(position, Quaternion.Euler(rotation.x, rotation.y, rotation.z), Vector3.one);
            Matrix4x4 local_matrix = _parentTransform.worldToLocalMatrix * matrix4X4;
            Vector3 local_rotation = _parentTransform.worldToLocalMatrix * rotation;

            Vector3 grid_position = GridPosition(local_matrix.GetPosition());
            Vector3Int vector_key = VectorKeyPosition(grid_position);

            if (dataset.TryGetValue(vector_key, out TerrainSRPV2.TerrainData p_terrainData)) {
                //p_terrainData.local_matrix = local_matrix;
                p_terrainData.strength = Mathf.Clamp(p_terrainData.strength + strength, 0, 1);
                //p_terrainData.normal = local_rotation;

                UtilityFunc.SetDictionary(dataset, vector_key, p_terrainData);
                return;
            }

            dataset.Add(vector_key, new TerrainSRPV2.TerrainData() {
                local_matrix = local_matrix,
                normal = local_rotation,
                strength = strength
            });
        }

        public Task BuildKDTree()
        {
            kdTree.Clear();

            Matrix4x4 selfTransform = _parentTransform.localToWorldMatrix;

            return Task.Run(() => {
                foreach (var d in this.dataset)
                {
                    Matrix4x4 data_matrix = d.Value.local_matrix;
                    Vector3 targetPoint = data_matrix.GetPosition();

                    kdTree.Add(new[] { targetPoint.x, targetPoint.y, targetPoint.z }, d.Key);
                }
            });
        }

        private Vector3 GridPosition(Vector3 position) {
            //Round down
            position.x = (float)System.Math.Round(position.x, _precision, System.MidpointRounding.AwayFromZero);
            position.y = (float)System.Math.Round(position.y, _precision, System.MidpointRounding.AwayFromZero);
            position.z = (float)System.Math.Round(position.z, _precision, System.MidpointRounding.AwayFromZero);

            return position;
        }

        private Vector3Int VectorKeyPosition(Vector3 position) {
            var vectorInt = new Vector3Int();

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
