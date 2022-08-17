using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hsinpa.Winweed.Uti;

namespace Hsinpa.Winweed
{
    [System.Serializable, CreateAssetMenu(fileName = "TerrainSRP", menuName = "SRP/WinWeed/Create TerrainSRP", order = 1)]
    public class TerrainSRP : ScriptableObject
    {
        [SerializeField]
        private Vector2Int subdivide = Vector2Int.one;
        public Vector2Int Subdivide => subdivide;

        [SerializeField]
        private Vector2 terrain_size = Vector2.one;
        public Vector2 Terrain_Size => terrain_size;

        //[SerializeField]
        //TerrainDictionary<int, PaintedTerrainStruct> _recordTerrains = new TerrainDictionary<int, PaintedTerrainStruct>();

        [SerializeField]
        TerrainDictionary _recordTerrains = new TerrainDictionary();
        public IDictionary<int, PaintedTerrainStruct> RawTerrains
        {
            get { return _recordTerrains; }
            set { _recordTerrains.CopyFrom(value); }
        }

        public List<PaintedTerrainStruct> Terrains => _recordTerrains.Values.ToList();

        public int Grid_Count => subdivide.x * subdivide.y;

        private Vector2Int _cacheVectorInt = new Vector2Int(-1, -1);

        public PaintedTerrainStruct GetPaintedStruct(int x, int y) {
            this._cacheVectorInt.Set(x, y);

            int index = GetIndexByGrid(x, y);

            if (_recordTerrains.TryGetValue(index, out var paintedStruct)) {
                return paintedStruct;
            }

            return default(PaintedTerrainStruct);
        }

        public void PaintTerrain(PaintedTerrainStruct p_data) {
            int index = GetIndexByGrid(p_data.index.x, p_data.index.y);

            //Remove
            if (p_data.weight < 0.01f) {
                _recordTerrains.Remove(index);
                return;
            }

            if (RawTerrains.ContainsKey(index)) {
                RawTerrains[index] = p_data;
            } else {
                RawTerrains.Add(index, p_data);
            }

        }

        public Vector2Int GetGridIndexFromUV(Vector2 uv)
        {
            Vector2Int index = new Vector2Int(Mathf.FloorToInt(Subdivide.x * uv.x),
                                               Mathf.FloorToInt(Subdivide.y * uv.y));

            return index;
        }

        public int GetIndexByGrid(int grix_x, int grid_y) {
            return (Subdivide.x * grid_y) + grix_x;
        }

        [System.Serializable]
        public struct PaintedTerrainStruct {
            public Vector2Int index;
            public float weight;
            public bool is_valid => weight > 0;
        }

        public void Dispose() {
            _recordTerrains.Clear();
        }

    }
}