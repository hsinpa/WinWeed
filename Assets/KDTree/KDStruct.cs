using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Algorithm.KDTree
{
    public class KDStruct
    {

        [System.Serializable]
        public struct KDVector {
            public int id;
            public int axis;
            public float[] data;
        }

    }
}