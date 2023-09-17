using System.Collections;
using System.Collections.Generic;
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

        public void ReDraw(int p_instance_count = 0) {
            if (!kdtree_ready) return;

            if (this.m_PropertyBlock == null)
                this.m_PropertyBlock = new MaterialPropertyBlock();


        }

        private WeedStatic.PaintedWeedStruct GetPainteWeedStruct() {







            return default(WeedStatic.PaintedWeedStruct);
        }

        #region Monobehavior
        private async void Start() {
            m_weedTerrainBuilderV2 = GetComponent<WeedTerrainBuilderV2>();
            m_weedGeneratorHelper = new WeedGeneratorHelper(material, SEGMENT, wind_config, GetPainteWeedStruct);

            await m_weedTerrainBuilderV2.BuildKDTree();
        }

        private void Update() {
            ReDraw(instance_count);
        }
        #endregion
    }
}