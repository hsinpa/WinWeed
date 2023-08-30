using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Hsinpa.Winweed.EditorCode
{
    [CustomEditor(typeof(WeedTerrainBuilderV2))]

    public class WinweedTerrainEditorV2 : Editor
    {
        private WeedTerrainBuilderV2 builderV2;
        private bool _mouseClickFlag;

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

            if (guiEvent.type == EventType.MouseDown)
            {
                _mouseClickFlag = true;
            }

            if (guiEvent.type == EventType.MouseUp) {
                _mouseClickFlag = false;
            }

            if (_mouseClickFlag)
            {
                Ray worldRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
                builderV2.ProcessRaycast(worldRay);
            }
        }


        private void OnEnable()
        {
            builderV2 = (WeedTerrainBuilderV2)target;
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