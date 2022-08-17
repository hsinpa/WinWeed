using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed
{
    [ExecuteInEditMode]
    public class WeedTerrainBuilder : MonoBehaviour
    {
        [Header("Terrain Config")]
        [SerializeField]
        private TerrainSRP terrainSRP;
        public TerrainSRP TerrainSRP => terrainSRP;

        [SerializeField, Range(0,1)]
        private float brush_weight = 0;
        public float Brush_Weight => brush_weight;

        [Header("Visual Config")]
        [SerializeField]
        private Color mouseHoverColor = new Color(0.1f, 0.6f, 0.1f, 1f);

        [SerializeField]
        private Color mousePaintedColor = new Color(0.0f, 0.8f, 0.8f, 1f);

        private Color gridColorDefault = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        private Vector2 mouse_uv;
        #region Editor Only Script

        public void SetMouseUV(Vector2 uv) {
            this.mouse_uv = uv;
        }

        private void OnDrawGizmos()
        {
            if (terrainSRP == null || Application.isPlaying) return;
            float space = 0.9f;
            float grid_size_x = (terrainSRP.Terrain_Size.x / terrainSRP.Subdivide.x);
            float grid_size_y = (terrainSRP.Terrain_Size.y / terrainSRP.Subdivide.y);

            float grid_height = 0.1f;
            Vector3 gridSize = new Vector3(grid_size_x * space, grid_height, grid_size_y * space);

            float start_x = transform.position.x - (terrainSRP.Terrain_Size.x * 0.5f) + (gridSize.x * 0.5f);
            float start_z = transform.position.z - (terrainSRP.Terrain_Size.y * 0.5f) + (gridSize.z * 0.5f);

            Vector2Int current_hover_grid = terrainSRP.GetGridIndexFromUV(this.mouse_uv);

            for (int x = 0; x < terrainSRP.Subdivide.x; x++) {
                for (int y = 0; y < terrainSRP.Subdivide.y; y++)
                {
                    float box_pos_x = start_x + (grid_size_x * x);
                    float box_pos_y = start_z + (grid_size_y * y);

                    Vector3 visualBoxCenter = new Vector3(box_pos_x, transform.position.y + (grid_height * 0.5f), box_pos_y);

                    if (x == current_hover_grid.x && y == current_hover_grid.y) {
                        GizmosDrawCube(mouseHoverColor, visualBoxCenter, gridSize);
                        continue;
                    }

                    bool isGizemoDrawed = GizmosShowPaintTerrain(x, y, visualBoxCenter, gridSize);
                    
                    if (!isGizemoDrawed)
                        GizmosDrawCube(gridColorDefault, visualBoxCenter, gridSize);
                }
            }
        }

        private void GizmosShowMouseHover() { 
            
        }

        private bool GizmosShowPaintTerrain(int index_x, int index_y, Vector3 position, Vector3 size) {

            var paintedStruct = terrainSRP.GetPaintedStruct(index_x, index_y);

            if (paintedStruct.is_valid) {
                GizmosDrawCube(mousePaintedColor * paintedStruct.weight, position, size);
            }

            return paintedStruct.is_valid;
        }

        private void GizmosDrawCube(Color color, Vector3 position, Vector3 size)
        {
            Gizmos.color = color;
            Gizmos.DrawCube(position, size);
            Gizmos.color = gridColorDefault;

        }
        #endregion
    }
}