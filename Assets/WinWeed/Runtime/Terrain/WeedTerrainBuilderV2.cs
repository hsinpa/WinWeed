using System.Collections;
using System.Collections.Generic;
using Hsinpa.Winweed.Terrain;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TerrainUtils;

namespace Hsinpa.Winweed
{
    [ExecuteInEditMode]

    public class WeedTerrainBuilderV2 : MonoBehaviour
    {
        [SerializeField]
        private LayerMask layerMask;

        [SerializeField]
        private TerrainSRPV2 terrainSRP;

        TerrainModel terrainModel;

        private void OnEnable()
        {
            terrainModel = new TerrainModel(this.transform, layerMask, digitPrecision: 1);
        }

        public void ProcessRaycast(Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 50))
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

        public void OnDrawGizmos() {
            Matrix4x4 selfTransform = this.transform.localToWorldMatrix;

            var dataset = terrainModel.DataSet;

            foreach (var d in dataset) {
                Matrix4x4 data_matrix = selfTransform * d.Value.local_matrix ;
                Vector3 world_normal = selfTransform * d.Value.normal;

                Gizmos.color = Color.blue;

                Vector3 targetPoint = data_matrix.GetPosition() + (world_normal * 0.1f);
                Gizmos.DrawLine(data_matrix.GetPosition(), targetPoint);
            }
        }
    }
}