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
        private KDNode _left;

        private KDNode _right;

        //Level in BST
        private int _level;
        public int Level => _level;

        public KDNode(KDStruct.KDVector point, KDNode left, KDNode right, int level) {
            _point = point;
            _left = left;
            _right = right;
            _level = level;
        }
    }
}
