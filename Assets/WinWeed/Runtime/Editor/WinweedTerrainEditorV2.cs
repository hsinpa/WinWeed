using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.InputSystem;
using System;

namespace Hsinpa.Winweed.EditorCode
{
    [CustomEditor(typeof(WeedTerrainBuilderV2))]
    public class WinweedTerrainEditorV2 : Editor
    {
        private WeedTerrainBuilderV2 builderV2;
        private bool _mouseClickFlag;
        System.Random rnd = new System.Random();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            builderV2 = (WeedTerrainBuilderV2)target;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            SceneView.RepaintAll();

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Event guiEvent = Event.current;

            Input(guiEvent);
        }

        void Input(Event guiEvent)
        {
            if (builderV2 == null) return;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                _mouseClickFlag = true;
            }

            if (guiEvent.type == EventType.MouseUp) {
                _mouseClickFlag = false;

                builderV2.Save();

                builderV2.BuildKDTree();

                EditorUtility.SetDirty(builderV2.TerrainSRP);
            }

            if (_mouseClickFlag)
            {
                Ray worldRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

                ProcessGroupRaycast(worldRay, max_ray_count: 50);
            }
        }

        private void ProcessGroupRaycast(Ray worldRay, int max_ray_count) {
            Vector3 right_dir = Vector3.Cross(worldRay.direction, Vector3.up).normalized;
            Vector3 up_dir = Vector3.Cross(worldRay.direction, right_dir).normalized;

            builderV2.ProcessRaycast(worldRay);

            int radius = 1;
            Vector3 original_pos = worldRay.origin;

            for (int i = 0; i < max_ray_count; i++) {
                float random_x = (float) (rnd.NextDouble() * 2 - 1) * radius;
                float random_y = (float) (rnd.NextDouble() * 2 - 1) * radius;

                Vector3 offset_position = original_pos + (right_dir * random_x) + (up_dir * random_y);

                float dist = Vector3.Distance(offset_position, original_pos);

                if (dist > radius) continue;
                worldRay.origin = offset_position;

                builderV2.ProcessRaycast(worldRay);
            }
        }

        private void OnEnable()
        {
            builderV2 = (WeedTerrainBuilderV2)target;
            builderV2.SetUp();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            builderV2 = (WeedTerrainBuilderV2)target;

            if (builderV2 == null) return;

            SceneView.duringSceneGui -= OnSceneGUI;
        }

    }
}