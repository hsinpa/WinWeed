using Hsinpa.Winweed.Terrain;
using Hsinpa.Winweed.Uti;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TerrainUtils;

namespace Hsinpa.Winweed
{
    [RequireComponent(typeof(WeedTerrainBuilderV2))]
    public class WinWeedGeneratorV2 : MonoBehaviour
    {
        [Header("Basic Config")]
        [SerializeField]
        private Material material;

        [SerializeField, Range(0, 100000)]
        private int instance_count = 0;

        [SerializeField]
        private float grass_height = 1;

        [SerializeField]
        private float grass_width = 0.02f;

        [SerializeField]
        private float grass_sharpness = 0.3f;

        [SerializeField, Range(0, 1)]
        private float random_strength;

        [Header("Wind Config")]
        [SerializeField]
        private WeedStatic.WindConfig wind_config;

        private const int SEGMENT = 2;

        private WeedTerrainBuilderV2 m_weedTerrainBuilderV2;
        private WeedGeneratorHelper m_weedGeneratorHelper;
        private MaterialPropertyBlock m_PropertyBlock;

        private bool kdtree_ready = false;
        Matrix4x4 m_transfromMatrix;
        float[] m_kd_tree_key_cache = new float[3];

        private async void ConstructGrassMesh(int p_instance_count = 0) {

            if (this.m_PropertyBlock == null)
                this.m_PropertyBlock = new MaterialPropertyBlock();

            if (m_weedTerrainBuilderV2.TerrainSRP.data == null ||
                m_weedTerrainBuilderV2.TerrainSRP.data.Count <= 0) return;

             m_weedGeneratorHelper.CreateGrassBufferData(p_instance_count, grass_height, grass_width, grass_sharpness);
        }

        private Vector3 RandomPointOnPlane(Vector3 position, Vector3 normal, float radius) {
            Vector3 randomPoint = Vector3.Cross(new Vector3(
                UtilityFunc.RandomRange(-1f, 1f),
                UtilityFunc.RandomRange(-1f, 1f),
                UtilityFunc.RandomRange(-1f, 1f)
            ), normal);


            randomPoint.Normalize();
            randomPoint *= radius;
            randomPoint += position;

            return randomPoint;
        }

        private WeedStatic.PaintedWeedStruct GetPainteWeedStruct() {
            var terrainSRP = m_weedTerrainBuilderV2.TerrainSRP;
            int random_terrain_index = UtilityFunc.RandomRange(0, terrainSRP.Count);

            if (terrainSRP.data.Count < random_terrain_index) return default(WeedStatic.PaintedWeedStruct);

            TerrainSRPV2.TerrainData terrainData = terrainSRP.data[random_terrain_index];

            Matrix4x4 world_matrix = m_transfromMatrix * terrainData.local_matrix ;
            Vector3 local_position = terrainData.local_matrix.GetPosition();

            m_kd_tree_key_cache[0] = local_position.x;
            m_kd_tree_key_cache[1] = local_position.y;
            m_kd_tree_key_cache[2] = local_position.z;
            //var radial_search = m_weedTerrainBuilderV2.TerrainModel.KDTree.RadialSearch(m_kd_tree_key_cache, 0.15f);

            Vector3 average_position = world_matrix.GetPosition();
            Vector3 average_normal = m_transfromMatrix * terrainData.normal;

            average_position = RandomPointOnPlane(average_position, average_normal, 0.1f);
            //foreach (var d in radial_search) {
            //    float ratio = UtilityFunc.Random();

            //    if (m_weedTerrainBuilderV2.TerrainModel.DataSet.TryGetValue(d.Value, out var neighborTerrain)) {
            //        Matrix4x4 world_neighbor_matrix = m_transfromMatrix * neighborTerrain.local_matrix;

            //        Vector3 world_neighbor_position = world_neighbor_matrix.GetPosition();
            //        Vector3 world_neighbor_normal = m_transfromMatrix * neighborTerrain.normal;

            //        average_position = Vector3.Lerp(average_position, world_neighbor_position, ratio);
            //        average_normal = Vector3.Lerp(average_normal, world_neighbor_normal, ratio);
            //    }
            //}

            return new WeedStatic.PaintedWeedStruct {
                weight = terrainData.strength,
                position = average_position,
                normal = average_normal
            };
        }

        #region Monobehavior
        private async void Start() {
            m_transfromMatrix = this.transform.localToWorldMatrix;

            m_weedTerrainBuilderV2 = GetComponent<WeedTerrainBuilderV2>();
            m_weedTerrainBuilderV2.SetUp();

            Bounds transfrom_bound = m_weedTerrainBuilderV2.TerrainSRP.Bounds;
            transfrom_bound.center = m_transfromMatrix * transfrom_bound.center;

            m_weedGeneratorHelper = new WeedGeneratorHelper(material, transfrom_bound, SEGMENT, wind_config, GetPainteWeedStruct);

            Debug.Log("Start KDTree");
            //await m_weedTerrainBuilderV2.TerrainModel.BuildKDTree();
            Debug.Log("End KDTree");

            this.ConstructGrassMesh(this.instance_count);
            kdtree_ready = true;
        }

        private void Update() {
            if (!kdtree_ready) return;
            this.m_weedGeneratorHelper.Render();
        }

        private void OnDestroy()
        {
            if (this.m_weedGeneratorHelper != null) this.m_weedGeneratorHelper.Dispose();
        }
        #endregion
    }
}