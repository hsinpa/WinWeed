using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Hsinpa.Algorithm.KDTree.KDStruct;

namespace Hsinpa.Algorithm.KDTree
{
    public class KDTree 
    {
        KDNode _root;

        /// <summary>
        /// K Dimension
        /// </summary>
        int _k;

        public KDTree(List<KDStruct.KDVector> point) { 
        
        }

        #region Public Utility API
        public float GetNodeKey(KDNode node) {
            return GetPointKey(node.Point, node.Level);
        }

        public  float GetPointKey(KDVector kDVector, int level) {
            int index = level % _k;
            return kDVector.data[index];
        }

        public int Compare(KDVector point, KDNode node) {
            return Math.Sign( GetPointKey(point, node.Level) - GetNodeKey(node) );
        }
        #endregion
    }
}