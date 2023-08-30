using Hsinpa.Winweed.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastExperiment : MonoBehaviour
{
    Camera selfCamera;
    TerrainModel terrainModel;

    private void Start() {
        selfCamera = this.GetComponent<Camera>();
        terrainModel = new TerrainModel(null, 0 << 1, digitPrecision: 1);
    }


    public void ProcessRaycast(Ray ray) {
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 50))
        {
            //Debug.Log($"Ray origin {hitInfo.point}");
            //Debug.Log($"Ray triangleIndex {hitInfo.triangleIndex}");
            //Debug.Log($"Ray barycentricCoordinate {hitInfo.barycentricCoordinate}");
            //Debug.Log($"Ray normal {hitInfo.normal}");
            //Debug.Log($"Ray uv {hitInfo.textureCoord}");

            terrainModel.Insert(hitInfo.point, hitInfo.normal, 1);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        
        var mousePosition = Mouse.current.position;
        Ray ray = selfCamera.ScreenPointToRay(mousePosition.value);
        ProcessRaycast(ray);
    }
}
