using Hsinpa.Winweed;
using Hsinpa.Winweed.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastExperiment : MonoBehaviour
{
    [SerializeField] private Transform child_transform;

    [SerializeField]
    private TerrainSRPV2 terrainSRP;

    Matrix4x4 child_matrix_origin = Matrix4x4.identity;

    private void Start() {
        child_matrix_origin = transform.worldToLocalMatrix * child_transform.localToWorldMatrix;
    }

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Matrix4x4 world_matrix = transform.localToWorldMatrix * child_matrix_origin;

        Debug.Log($"Local position {world_matrix.GetPosition()}");

        child_transform.position = world_matrix.GetPosition();
        child_transform.rotation = world_matrix.rotation;
        child_transform.localScale = world_matrix.lossyScale;
    }
}
