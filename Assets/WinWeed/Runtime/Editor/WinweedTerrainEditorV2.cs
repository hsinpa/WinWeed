using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.InputSystem;
using System;

namespace Hsinpa.Winweed.EditorCode
{
    public struct RaycastContactStruct {
        public Vector3 contact_point;
        public bool is_contact;
    }

    [CustomEditor(typeof(WeedTerrainBuilderV2))]
    public class WinweedTerrainEditorV2 : Editor
    {
        public PaintRadiusView PaintRadiusViewPrefab;

        private WeedTerrainBuilderV2 builderV2;
        private bool _mouseClickFlag;
        System.Random rnd = new System.Random();

        PaintRadiusView _paintRadiusView;
        private RaycastHit[] physicsHits = new RaycastHit[1];

        private Color32 EditColor = new Color32(124, 216, 243, 255);
        private Color32 EraseColor = new Color32(214, 66, 82, 255);
        RaycastContactStruct _raycastContactStruct = new RaycastContactStruct();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            builderV2 = (WeedTerrainBuilderV2)target;

            //Set Painter related config
            if (_paintRadiusView != null) {
                var painter_scale = _paintRadiusView.transform.localScale;
                painter_scale.Set(builderV2.PaintEffectRange, builderV2.PaintEffectRange, builderV2.PaintEffectRange);
                _paintRadiusView.transform.localScale = painter_scale;
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (builderV2.editorState != WeedTerrainBuilderV2.EditorState.Edit) return;
            
            SceneView.RepaintAll();

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Event guiEvent = Event.current;

            Input(guiEvent);
        }

        void Input(Event guiEvent)
        {
            if (builderV2 == null) return;

            Ray worldRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                _mouseClickFlag = true;
            }

            if (guiEvent.type == EventType.MouseUp) {
                _mouseClickFlag = false;

                builderV2.Save();

                builderV2.TerrainModel.BuildKDTree();

                EditorUtility.SetDirty(builderV2.TerrainSRP);
            }

            RaycastContactStruct contact_struct = ProcessPaintRadiusDecay(worldRay);

            if (_mouseClickFlag)
            {
                ProcessGroupRaycast(worldRay, max_ray_count: 50, contact_struct);
            }

        }

        private void ProcessGroupRaycast(Ray worldRay, int max_ray_count, RaycastContactStruct contact_struct) {
            Vector3 right_dir = Vector3.Cross(worldRay.direction, Vector3.up).normalized;
            Vector3 up_dir = Vector3.Cross(worldRay.direction, right_dir).normalized;

            float radius = builderV2.PaintEffectRange * 0.5f;
            Vector3 original_pos = worldRay.origin;

            if (builderV2.paintState == WeedTerrainBuilderV2.PaintState.Append)
            {
                builderV2.ProcessRaycast(worldRay);

                for (int i = 0; i < max_ray_count; i++)
                {
                    float random_x = (float)(rnd.NextDouble() * 2 - 1) * radius;
                    float random_y = (float)(rnd.NextDouble() * 2 - 1) * radius;

                    Vector3 offset_position = original_pos + (right_dir * random_x) + (up_dir * random_y);

                    float dist = Vector3.Distance(offset_position, original_pos);

                    if (dist > radius) continue;
                    worldRay.origin = offset_position;

                    builderV2.ProcessRaycast(worldRay);
                }
            }

            if (builderV2.paintState == WeedTerrainBuilderV2.PaintState.Delete && contact_struct.is_contact)
            {
                builderV2.RemoveWeedFromRange(contact_struct.contact_point, radius);
            }
        }

        private RaycastContactStruct ProcessPaintRadiusDecay(Ray worldRay) {
            _raycastContactStruct.is_contact = false;
            int hit_lens = Physics.RaycastNonAlloc(worldRay, physicsHits, maxDistance: 50);


            if (hit_lens > 0)
            {
                _paintRadiusView.gameObject.SetActive(true);
                _paintRadiusView.transform.position = physicsHits[0].point;

                _raycastContactStruct.is_contact = true;
                _raycastContactStruct.contact_point = _paintRadiusView.transform.position;

                return _raycastContactStruct;
            }

            if (builderV2 != null)
            {
                _paintRadiusView.SetColor(builderV2.paintState == WeedTerrainBuilderV2.PaintState.Append ? WeedStatic.Color.EditColor : WeedStatic.Color.EraseColor);
            }

            _paintRadiusView.gameObject.SetActive(false);

            return _raycastContactStruct;
        }

        private void FindOrCreatePaintRadiusView() {
            _paintRadiusView = GameObject.FindObjectOfType<PaintRadiusView>(includeInactive: true);

            if (_paintRadiusView == null)
            {
                _paintRadiusView = GameObject.Instantiate<PaintRadiusView>(PaintRadiusViewPrefab);
            }

            _paintRadiusView.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            FindOrCreatePaintRadiusView();

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