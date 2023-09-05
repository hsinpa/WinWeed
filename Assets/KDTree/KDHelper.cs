using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Hsinpa.Algorithm.KDTree.KDStruct;

namespace Hsinpa.Algorithm.KDTree
{
    public class KDHelper
    {
        public static float GetNodeKey(int k , KDNode node) {
            return KDHelper.GetPointKey(k, node.Point, node.Level);
        }

        public static float GetPointKey(int k, KDVector kDVector, int level) {
            int index = level % k;
            return kDVector.data[index];
        }
    }
}