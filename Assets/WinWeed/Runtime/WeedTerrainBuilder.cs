using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed
{
    [ExecuteInEditMode]
    public class WeedTerrainBuilder : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int subdivide = Vector2Int.one;
        public Vector2Int Subdivide => subdivide;

        [SerializeField]
        private Vector2 terrain_size = Vector2.one;
        public Vector2 Terrain_Size => terrain_size;

        private void OnDrawGizmosSelected()
        {
            float space = 0.9f;
            float grid_size_x = (terrain_size.x / subdivide.x);
            float grid_size_y = (terrain_size.y / subdivide.y);

            Vector3 gridSize = new Vector3(grid_size_x * space, 1, grid_size_y * space);

            float start_x = transform.position.x - (terrain_size.x * 0.5f) + (gridSize.x * 0.5f);
            float start_z = transform.position.z - (terrain_size.y * 0.5f) + (gridSize.z * 0.5f);


            for (int x = 0; x < subdivide.x; x++) {
                for (int y = 0; y < subdivide.y; y++)
                {
                    float box_pos_x = start_x + (grid_size_x * x);
                    float box_pos_y = start_z + (grid_size_y * y);


                    Vector3 visualBoxCenter = new Vector3(box_pos_x, transform.position.y + 0.5f, box_pos_y);

                    Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                    Gizmos.DrawCube(visualBoxCenter, gridSize);
                }
            }
        }
    }
}