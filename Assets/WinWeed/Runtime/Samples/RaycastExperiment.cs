using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastExperiment : MonoBehaviour
{
    Camera selfCamera;

    private void Start() {
        selfCamera = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        var mousePosition = Mouse.current.position;
        Ray ray = selfCamera.ScreenPointToRay(mousePosition.value);

        if (Mouse.current.leftButton.wasPressedThisFrame) {

            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 50)) {
                Debug.Log($"Ray origin {hitInfo.point}");
                Debug.Log($"Ray triangleIndex {hitInfo.triangleIndex}");
                Debug.Log($"Ray barycentricCoordinate {hitInfo.barycentricCoordinate}");
                Debug.Log($"Ray normal {hitInfo.normal}");
                Debug.Log($"Ray uv {hitInfo.textureCoord}");
            };
        }

        //Physics.Raycast();
    }
}