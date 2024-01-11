using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed
{
    public class WeedCulling
    {
        private List<Camera> _cameras = new List<Camera>();
        private List<WinWeedGeneratorV2> _terrainBounds = new List<WinWeedGeneratorV2>();

        private Plane[] _cache_planes = new Plane[6];

        public void RegisterCamera(Camera cam) {
            _cameras.Add(cam);
        }

        public void RegisterWeed(WinWeedGeneratorV2 terrainBound) {
            _terrainBounds.Add(terrainBound);
        }

        public void OnUpdate()
        {
            int cam_lens = _cameras.Count;
            int terrain_lens = _terrainBounds.Count;

            if (cam_lens <= 0 || terrain_lens <= 0) return;

            for (int i = cam_lens - 1; i >= 0; i--) {

                bool cam_exist = IsNullAndCleanup(ref _cameras, i);
                if (cam_exist) {
                    continue;
                }

                for (int k = terrain_lens - 1; k >= 0; k--) {
                    bool bound_exist = IsNullAndCleanup(ref _terrainBounds, k);
                    if (bound_exist) {
                        continue;
                    }

                    Camera c = _cameras[i];
                    Bounds b = _terrainBounds[k].bounds;

                    GeometryUtility.CalculateFrustumPlanes(c, _cache_planes);
                    bool is_collide_with_cam = GeometryUtility.TestPlanesAABB(_cache_planes, b);

                    _terrainBounds[k].SetCulling(is_collide_with_cam);
                }
            }
        }

        bool IsNullAndCleanup<T>(ref List<T> list, int index) {
            T t_object = list[index];

            if (t_object == null) {
                list.RemoveAt(index);
                return true;
            }

            return false;
        }
    }
}