using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Hsinpa.Winweed;
using Hsinpa.Winweed.Uti;
using Hsinpa.Winweed.Sample;

namespace Hsinpa.Winweed.EditorCode
{

    [CustomPropertyDrawer(typeof(TerrainDictionary))]
    public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

    [CustomEditor(typeof(WeedTerrainBuilder))]
    public class WinweedTerrainEditor : Editor
    {
        private WeedTerrainBuilder builder;
        private bool lockInspectorFlag = false;

        private bool _mouseClickFlag = false;
        private bool _grassPreviewFlag;
        private SimpleGrassGenerator simpleGrassGenerator;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            builder = (WeedTerrainBuilder)target;

            string lockString = (lockInspectorFlag) ? "Unlock" : "Lock";
            GUI.color =  (lockInspectorFlag) ? Color.red : Color.white;
            if (GUILayout.Button( new GUIContent() { text = lockString }))
            {
                LockInspector(!lockInspectorFlag);

                return;
            }
            GUI.color = Color.white;

            if (GUILayout.Button("Redraw"))
            {
                simpleGrassGenerator.ReDraw();

                return;
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            SceneView.RepaintAll();

            Event guiEvent = Event.current;
            if (lockInspectorFlag) {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
                _mouseClickFlag = true;

            Input(guiEvent);
            //Draw(guiEvent);
        }

        void Input(Event guiEvent)
        {
            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
            Vector3 direction = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).direction;

            Vector3 plane_position = builder.transform.position;
            Vector3 plane_normal = new Vector3(0, -1, 0);

            var actionResult = CollisionUti.IntersectionPlane(plane_position, plane_normal, mousePos, direction);
            
            if (actionResult.valid && builder.DataSRP != null) {
                Vector2 uv = new Vector2(0, 0);
                Vector3 landing_pos = mousePos + (direction * actionResult.t);
                CollisionUti.GetPlaneIntersectUV(new Vector2(plane_position.x, plane_position.z), builder.DataSRP.Size,
                                                 new Vector2(landing_pos.x, landing_pos.z), out uv);

                builder.SetMouseUV(uv);
                Vector2Int gridIndex = builder.DataSRP.GetGridIndexFromUV(uv);

                if (_mouseClickFlag && lockInspectorFlag && TerrainSRP.IsUVValid(uv)) {
                    builder.DataSRP.PaintTerrain(new TerrainSRP.PaintedTerrainStruct() { index = gridIndex, weight = builder.Brush_Weight });

                    EditorUtility.SetDirty(builder.DataSRP);
                }

                SceneView.RepaintAll();
            }
        }


        private void LockInspector(bool p_lock) {
            lockInspectorFlag = p_lock;

            ActiveEditorTracker.sharedTracker.isLocked = lockInspectorFlag;

            if (!lockInspectorFlag)
                ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        private void OnEnable()
        {
            builder = (WeedTerrainBuilder)target;
            simpleGrassGenerator = builder.GetComponent<SimpleGrassGenerator>();
            SceneView.duringSceneGui += OnSceneGUI;

            if (Application.isPlaying)
            {
                builder.enable_preview = false;

                if (ActiveEditorTracker.sharedTracker.isLocked) 
                    LockInspector(false);
            }
        }

        void OnDisable()
        {
            builder = (WeedTerrainBuilder)target;

            if (builder == null) return;

            SceneView.duringSceneGui -= OnSceneGUI;
            builder.enable_preview = false;
        }

    }
}