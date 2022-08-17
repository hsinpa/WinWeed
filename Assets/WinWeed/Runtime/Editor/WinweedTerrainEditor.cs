using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Hsinpa.Winweed;
using Hsinpa.Winweed.Uti;
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

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            builder = (WeedTerrainBuilder)target;

            string lockString = (lockInspectorFlag) ? "Unlock" : "Lock";
            if (GUILayout.Button(lockString))
            {
                lockInspectorFlag = !lockInspectorFlag;
                ActiveEditorTracker.sharedTracker.isLocked = lockInspectorFlag;

                if (!lockInspectorFlag)
                    ActiveEditorTracker.sharedTracker.ForceRebuild();

                return;
            }
        }

        private void OnSceneGUI()
        {
            Event guiEvent = Event.current;
            if (lockInspectorFlag)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
                _mouseClickFlag = true;

            if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
                _mouseClickFlag = false;

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
            
            if (actionResult.valid && builder.TerrainSRP != null) {
                Vector2 uv = new Vector2(0, 0);
                Vector3 landing_pos = mousePos + (direction * actionResult.t);
                CollisionUti.GetPlaneIntersectUV(new Vector2(plane_position.x, plane_position.z), builder.TerrainSRP.Terrain_Size,
                                                 new Vector2(landing_pos.x, landing_pos.z), out uv);

                builder.SetMouseUV(uv);
                Vector2Int gridIndex = builder.TerrainSRP.GetGridIndexFromUV(uv);

                if (_mouseClickFlag && gridIndex.x >= 0 && gridIndex.y >= 0 && lockInspectorFlag) {
                    builder.TerrainSRP.PaintTerrain(new TerrainSRP.PaintedTerrainStruct() { index = gridIndex, weight = builder.Brush_Weight });

                    EditorUtility.SetDirty(builder.TerrainSRP);
                }

                SceneView.RepaintAll();
                //Debug.Log($"landing_pos {landing_pos.x}, {landing_pos.y}, {landing_pos.z}");
                //Debug.Log($"UV {uv.x}, {uv.y}");
            }

            //Debug.Log($"Mouse Pos {mousePos.x}, {mousePos.y}, {mousePos.z}");
            //Debug.Log($"Mouse Direction {direction.x}, {direction.y}, {direction.z}");
        }

        private void OnEnable()
        {
            builder = (WeedTerrainBuilder)target;

        }

    }
}