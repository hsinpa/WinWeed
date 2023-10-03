using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hsinpa.Winweed.Terrain;
using Hsinpa.Winweed.Uti;
using KdTree.Math;
using KdTree;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TerrainUtils;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace Hsinpa.Winweed
{
    [ExecuteInEditMode]

    public class WeedTerrainBuilderV2 : MonoBehaviour
    {
        [SerializeField]
        private LayerMask layerMask;

        [SerializeField]
        private TerrainSRPV2 terrainSRP;
        public TerrainSRPV2 TerrainSRP => this.terrainSRP;

        private TerrainModel terrainModel;
        public TerrainModel TerrainModel => terrainModel;

        private KdTree.KdTree<float, Vector3Int> kdTree = new KdTree<float, Vector3Int>(3, new FloatMath());
        public KdTree.KdTree<float, Vector3Int> KDTree => kdTree;

        public void SetUp()
        {
            terrainModel = new TerrainModel(this.transform, layerMask, digitPrecision: 1);

            if (terrainSRP != null) terrainModel.Load(terrainSRP.data);
        }

        public void ProcessRaycast(Ray ray)
        {
            if (terrainModel != null && Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 50))
            {
                //Debug.Log($"Ray origin {hitInfo.point}");
                //Debug.Log($"Ray triangleIndex {hitInfo.triangleIndex}");
                //Debug.Log($"Ray barycentricCoordinate {hitInfo.barycentricCoordinate}");
                //Debug.Log($"Ray normal {hitInfo.normal}");
                //Debug.Log($"Ray uv {hitInfo.textureCoord}");

                terrainModel.Insert(hitInfo.point, hitInfo.normal, 1);
            };
        }

        public void Save() {
            if (terrainModel == null || terrainSRP == null) return;

            terrainSRP.Save(terrainModel.DataSet);
        }

        public Task BuildKDTree() {
            kdTree.Clear();

            Matrix4x4 selfTransform = this.transform.localToWorldMatrix;

            return Task.Run(() => {
                var dataset = terrainModel.DataSet;

                foreach (var d in dataset) {
                    Matrix4x4 data_matrix = d.Value.local_matrix;
                    Vector3 targetPoint = data_matrix.GetPosition();

                    kdTree.Add(new[] { targetPoint.x, targetPoint.y, targetPoint.z }, d.Key);
                }
            });
        }

        public WeedStatic.PaintedWeedStruct GetPainteWeedStruct() {
            int count = terrainSRP.data.Count;
            int random_point = UtilityFunc.RandomRange(0, count);

            return default(WeedStatic.PaintedWeedStruct);
        }

        public void OnDrawGizmos() {
            if (terrainModel == null) return;

            Matrix4x4 selfTransform = this.transform.localToWorldMatrix;

            var dataset = terrainModel.DataSet;

            foreach (var d in dataset) {
                Matrix4x4 data_matrix = selfTransform * d.Value.local_matrix ;
                Vector3 world_normal = selfTransform * d.Value.normal;

                Gizmos.color = Color.blue;

                Vector3 targetPoint = data_matrix.GetPosition() + (world_normal * 0.1f);
                Gizmos.DrawLine(data_matrix.GetPosition(), targetPoint);
            }
            
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(selfTransform.GetPosition() + terrainSRP.Bounds.center, terrainSRP.Bounds.size);
        }
    }
}