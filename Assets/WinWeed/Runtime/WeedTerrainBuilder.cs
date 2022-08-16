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

        [SerializeField]
        private Vector2 terrain_size = Vector2.one;

        private void OnDrawGizmosSelected()
        {
            float grid_size_x = (terrain_size.x / subdivide.x);
            float grid_size_y = (terrain_size.y / subdivide.y);

            float start_x = transform.position.x - (terrain_size.x * 0.5f);
            float start_z = transform.position.z - (terrain_size.y * 0.5f);

            float space = 0.95f;
            Vector3 gridSize = new Vector3(grid_size_x * space, 1, grid_size_y * space);

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