using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed
{
    public class PaintRadiusView : MonoBehaviour
    {
        [SerializeField]
        private Material material;

        public void SetColor(Color color) {
            if (material == null) return;

            material.SetColor("_Color", color);
        }

    }
}