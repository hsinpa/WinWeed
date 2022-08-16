using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Hsinpa.Winweed;

namespace Hsinpa.Winweed.EditorCode
{
    [CustomEditor(typeof(WeedTerrainBuilder))]
    public class WinweedTerrainEditor : Editor
    {
        private WeedTerrainBuilder builder;
        private bool lockInspectorFlag = false;

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


            Input(guiEvent);
            //Draw(guiEvent);
        }

        void Input(Event guiEvent)
        {

            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
            Vector3 direction = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).direction;


            //Debug.Log($"Mouse Pos {mousePos.x}, {mousePos.y}, {mousePos.z}");
            //Debug.Log($"Mouse Direction {direction.x}, {direction.y}, {direction.z}");
        }

        private void OnEnable()
        {
            builder = (WeedTerrainBuilder)target;

        }

    }
}