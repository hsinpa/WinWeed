using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.Algorithm.KDTree
{
    public class KDNode
    {
        private KDStruct.KDVector _point;
        public KDStruct.KDVector Point => _point;

        //BST
        public KDNode left;
        public KDNode right;

        //Level in BST
        private int _level;
        public int Level => _level;

        public KDNode(KDStruct.KDVector point, KDNode left, KDNode right, int level) {
            this.left = left;
            this.right = right;

            _point = point;
            _level = level;
        }

        public static KDNode Min(int k_index, params KDNode[] nodes) {
            float minValue = float.PositiveInfinity;
            KDNode minNode = null;

            int n_lens = nodes.Length;
            
            for (int i = 0; i < n_lens; i++) {

                float value = nodes[i].Point.data[k_index];
                if (value < minValue) {
                    minValue = value;
                    minNode = nodes[i];
                }
            }
            return minNode;
        }

    }
}
