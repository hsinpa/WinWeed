using Hsinpa.Winweed.Terrain;
using Hsinpa.Winweed.Uti;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hsinpa.Winweed
{
    [RequireComponent(typeof(WeedTerrainBuilderV2)), ExecuteInEditMode]
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

        [SerializeField]
        private float wind_strength;

        private const int SEGMENT = 2;

        private WeedTerrainBuilderV2 m_weedTerrainBuilderV2;

        private WeedGeneratorHelper m_weedGeneratorHelper;
        private MaterialPropertyBlock m_PropertyBlock;

        private bool kdtree_ready = false;
        Matrix4x4 m_transfromMatrix;
        float[] m_kd_tree_key_cache = new float[3];

        public void ReDraw(int p_instance_count = 0) {

            if (this.m_PropertyBlock == null)
                this.m_PropertyBlock = new MaterialPropertyBlock();

            m_weedGeneratorHelper.CreateGrassBufferData(m_weedTerrainBuilderV2.TerrainSRP.Bounds, p_instance_count, grass_height, grass_width, grass_sharpness);

            Debug.Log("ReDraw");
        }

        private WeedStatic.PaintedWeedStruct GetPainteWeedStruct() {
            var terrainSRP = m_weedTerrainBuilderV2.TerrainSRP;
            int random_terrain_index = UtilityFunc.RandomRange(0, terrainSRP.Count);
            TerrainSRPV2.TerrainData terrainData = terrainSRP.data[random_terrain_index];

            Matrix4x4 world_matrix = m_transfromMatrix * terrainData.local_matrix ;
            Vector3 local_position = terrainData.local_matrix.GetPosition();

            m_kd_tree_key_cache[0] = local_position.x;
            m_kd_tree_key_cache[1] = local_position.y;
            m_kd_tree_key_cache[2] = local_position.z;

            var radial_search = m_weedTerrainBuilderV2.KDTree.RadialSearch(m_kd_tree_key_cache, 0.15f);

            Vector3 average_position = world_matrix.GetPosition();
            Vector3 average_normal = m_transfromMatrix * terrainData.normal;

            foreach (var d in radial_search) {
                float ratio = UtilityFunc.RandomRange(0f, 1f);

                if (m_weedTerrainBuilderV2.TerrainModel.DataSet.TryGetValue(d.Value, out var neighborTerrain)) {
                    Matrix4x4 world_neighbor_matrix = m_transfromMatrix * neighborTerrain.local_matrix;

                    Vector3 world_neighbor_position = world_neighbor_matrix.GetPosition();
                    Vector3 world_neighbor_normal = m_transfromMatrix * neighborTerrain.normal;

                    average_position = Vector3.Lerp(average_position, world_neighbor_position, ratio);
                    average_normal = Vector3.Lerp(average_normal, world_neighbor_normal, ratio);
                }
            }

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
            m_weedGeneratorHelper = new WeedGeneratorHelper(material, SEGMENT, wind_config, GetPainteWeedStruct);

            await m_weedTerrainBuilderV2.BuildKDTree();
            kdtree_ready = true;

            ReDraw(instance_count);
        }

        private void Update() {
            //ReDraw(instance_count);
        }
        #endregion
    }
}