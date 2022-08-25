using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed
{
    public class WeedLODs : MonoBehaviour
    {
        [SerializeField, Range(1, 1000)]
        private int max_distance = 10;

        [SerializeField, Range(1, 10)]
        private int LOD_Segment = 1;

        private SimpleWeedGenerator[] simpleGrassGenerators;
        private Camera _mainCamera;

        private void Start()
        {
            simpleGrassGenerators = GetComponentsInChildren<SimpleWeedGenerator>(includeInactive: true);
        }

        private void Update()
        {
            if (simpleGrassGenerators == null || simpleGrassGenerators.Length <= 0) return;

            Camera camera = GetCamera();

            if (camera == null) return;
            Vector3 camera_pos = camera.transform.position;

            for (int i = 0; i < simpleGrassGenerators.Length; i++)
            {
                if (simpleGrassGenerators[i] == null) continue;

                ExecCulling(simpleGrassGenerators[i], camera);

                if (simpleGrassGenerators[i].IsCull) continue;
                    ExecLOD(simpleGrassGenerators[i], camera_pos);
            }
        }

        private void ExecCulling(SimpleWeedGenerator weed, Camera p_camera) {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(p_camera);

            bool is_within_frustum = !GeometryUtility.TestPlanesAABB(planes, weed.Bounds); // Not sure why its revert
            weed.SetCulling(is_within_frustum);
        }

        private void ExecLOD(SimpleWeedGenerator weed, Vector3 mainCharacterPos) {
            float distance = Vector3.Distance(weed.Bounds.ClosestPoint(mainCharacterPos), mainCharacterPos);

            float segment_dist = max_distance / LOD_Segment;

            if (distance > max_distance && weed.LOD_Level < LOD_Segment) {
                weed.SetCulling(true);
                weed.SetLOD(LOD_Segment, 0);
                return;
            }

            int lod_ceiling = Mathf.FloorToInt(distance / segment_dist);
            float weigth = 1 - Mathf.Clamp((float)lod_ceiling / (LOD_Segment), 0, 1);

            //Debug.Log($"lod_ceiling {lod_ceiling}, Dist {distance}, Segment {segment_dist}");

            if (weed.LOD_Level != lod_ceiling) {
                weed.SetLOD(lod_ceiling, weigth);
                weed.SetCulling(false);
            }
        }

        private Camera GetCamera() {
            if (_mainCamera != null) return _mainCamera;

            _mainCamera = Camera.main;

            return _mainCamera;
        }
    }
}