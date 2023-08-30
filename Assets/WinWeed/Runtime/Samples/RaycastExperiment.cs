using Hsinpa.Winweed.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastExperiment : MonoBehaviour
{
    [SerializeField] private Transform child_transform;

    private void Start() {
    }

    // Update is called once per frame
    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Matrix4x4 child_matrix = Matrix4x4.identity;
        Matrix4x4 parent_matrix = transform.localToWorldMatrix;

        Matrix4x4 new_matrix = child_matrix * parent_matrix;

        child_transform.position = new_matrix.GetPosition();
        child_transform.rotation = new_matrix.rotation;
        child_transform.localScale = new_matrix.lossyScale;
    }
}
