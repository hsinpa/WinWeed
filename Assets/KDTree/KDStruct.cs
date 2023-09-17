using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hsinpa.Algorithm.KDTree
{
    public class KDStruct
    {
        [System.Serializable]
        public struct KDVector {
            public object id;
            public float[] data;

            public static bool operator == (KDVector lhs, KDVector rhs) {
                return lhs.data.SequenceEqual(rhs.data);
            }
            public static bool operator != (KDVector lhs, KDVector rhs) {
                return !lhs.data.SequenceEqual(rhs.data);
            }

            public static float Distance(KDVector lhs, KDVector rhs) {
                if (lhs.data.Length != rhs.data.Length) return 0;

                float sum = 0;
                int lens = lhs.data.Length;

                for (int i = 0; i < lens; i++) {
                    sum += Mathf.Pow(lhs.data[i] - rhs.data[i], 2);
                }

                return Mathf.Sqrt(sum);
            }

        }

        [System.Serializable]
        public struct NNStruct
        {
            public float nnDist;
            public KDNode node;

            public bool is_valid => node != null;
        }

    }
}