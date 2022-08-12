using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.Winweed.Sample
{
    public class SimpleGrassGenerator : MonoBehaviour
    {
        [SerializeField]
        private Renderer _renderer;

        [SerializeField]
        private MeshFilter _meshFilter;

        private GrassMesh _grassMesh;
        private const int SEGMENT = 2;

        // Start is called before the first frame update
        void Start()
        {
            this._grassMesh = new GrassMesh();

            Mesh p_grassMesh = this._grassMesh.CreateMesh(height: 2, width: 0.1f, segment: SEGMENT);

            _meshFilter.mesh = p_grassMesh;
        }
    }
}