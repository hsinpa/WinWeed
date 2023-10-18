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

        [Header("Configuration")]
        [SerializeField, Range(0.1f, 100)]
        private float paintEffectRange = 1;
        public float PaintEffectRange => paintEffectRange;

        public enum EditorState { Preview, Render, Edit }
        public EditorState editorState;

        public enum PaintState { Append, Delete }
        public PaintState paintState;

        private TerrainModel terrainModel;
        public TerrainModel TerrainModel => terrainModel;

        private const int PHYSICS_HIT_MAX = 2;
        private RaycastHit[] physicsHits = new RaycastHit[PHYSICS_HIT_MAX];
        private float[] vector_cache = new float[3];
        private bool kd_processing_flag = false;

        public void SetUp()
        {
            terrainModel = new TerrainModel(this.transform, layerMask, digitPrecision:1);

            if (terrainSRP != null) terrainModel.Load(terrainSRP.data);
            terrainModel.BuildKDTree();
        }

        public void ProcessRaycast(Ray ray)
        {
            if (terrainModel != null && Physics.RaycastNonAlloc(ray, physicsHits, maxDistance: 50) > 0)
            {
                //Debug.Log($"Ray origin {hitInfo.point}");
                //Debug.Log($"Ray triangleIndex {hitInfo.triangleIndex}");
                //Debug.Log($"Ray barycentricCoordinate {hitInfo.barycentricCoordinate}");
                //Debug.Log($"Ray normal {hitInfo.normal}");
                //Debug.Log($"Ray uv {hitInfo.textureCoord}");
                RaycastHit hitInfo = physicsHits[0];
                terrainModel.Insert(hitInfo.point, hitInfo.normal, 1);
            };
        }

        public void RemoveWeedFromRange(Vector3 center, float radius) {
            vector_cache[0] = center.x;
            vector_cache[1] = center.y;
            vector_cache[2] = center.z;

            if (kd_processing_flag) return;

            kd_processing_flag = true;

            Task.Run(() =>
            {
                lock (terrainModel.dataSet)
                {
                    var find_nodes = terrainModel.KDTree.RadialSearch(vector_cache, radius);

                    foreach (var node in find_nodes)
                    {
                        terrainModel.dataSet.Remove(node.Value);
                    }

                    kd_processing_flag = false;
                }

            });            
        }

        public void Save() {
            if (terrainModel == null || terrainSRP == null) return;

            terrainSRP.Save(terrainModel.dataSet);
        }

        public WeedStatic.PaintedWeedStruct GetPainteWeedStruct() {
            int count = terrainSRP.data.Count;
            int random_point = UtilityFunc.RandomRange(0, count);

            return default(WeedStatic.PaintedWeedStruct);
        }

        public void OnDrawGizmos() {
            if (terrainModel == null) return;

            Matrix4x4 selfTransform = this.transform.localToWorldMatrix;

            var dataset = terrainModel.dataSet;

            try {
                foreach (var d in dataset)
                {
                    Matrix4x4 data_matrix = selfTransform * d.Value.local_matrix;
                    Vector3 world_normal = selfTransform * d.Value.normal;

                    Gizmos.color = Color.blue;

                    Vector3 targetPoint = data_matrix.GetPosition() + (world_normal * 0.1f);
                    Gizmos.DrawLine(data_matrix.GetPosition(), targetPoint);
                }

                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(selfTransform.GetPosition() + terrainSRP.Bounds.center, terrainSRP.Bounds.size);
            }
            catch
            {

            }
        }
    }
}