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

        public KDTree(int k) {
            this._k = k;
        }

        public void Build(List<KDStruct.KDVector> points) {
            this._root = this.ConstructKDTree(points);
        }

        public KDNode Search(KDNode node, KDVector target) {
            if (node == null) return null;

            if (node.Point == target) return node;

            if (Compare(target, node) < 0)
                return Search(node.left, target);
            else
                return Search(node.right, target);
        }

        public KDNode Insert(KDNode node, KDVector newPoint, int level = 0) {
            if (node == null) return new KDNode(newPoint, null, null, level);

            if (node.Point == newPoint) return node;

            if (Compare(newPoint, node) < 0) {
                node.left = Insert(node.left, newPoint, node.Level + 1);
                return node;
            } else {
                node.right = Insert(node.right, newPoint, node.Level + 1);
                return node;
            }
        }

        public KDNode FindMin(KDNode node, int coordinateIndex) {
            if (node == null) return null;

            if (node.Level == coordinateIndex) {
                if (node.left == null) {
                    return node;
                } else {
                    return FindMin(node.left, coordinateIndex);
                }
            } else {
                int index = coordinateIndex % this._k;
                var leftMin = FindMin(node.left, coordinateIndex);
                var rightMin = FindMin(node.right, coordinateIndex);

                return KDNode.Min(index, node, leftMin, rightMin);
            }
        }
        
        public KDStruct.NNStruct NearestNeighbor(KDNode node, KDVector target, KDStruct.NNStruct nnStruct) {
            if (node == null) return nnStruct;

            float dist = KDVector.Distance(node.Point, target);

            if (dist < nnStruct.nnDist) { 
            
            }

            return nnStruct;
        }


        private KDNode ConstructKDTree(List<KDStruct.KDVector> points, int level = 0) {
            if (points == null || points.Count == 0) return null;
            if (points.Count == 1) return new KDNode(points[0], null, null, level);

            int axis = level % _k;
            KDHelper.QuickSort(axis: axis, front: 0, end: points.Count - 1, point: ref points);
            var partitionStruct = KDHelper.GetPartitionStruct(axis, points);

            KDNode leftTree = ConstructKDTree(partitionStruct.left, level + 1);
            KDNode rightTree = ConstructKDTree(partitionStruct.right, level + 1);

            return new KDNode(partitionStruct.median, leftTree, rightTree, level);
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
            var s = Math.Sign( GetPointKey(point, node.Level) - GetNodeKey(node) );

            //If 0, by even number
            if (s == 0) return node.Level % 2 == 0 ? -1 : 1;

            return s;
        }

        public float splitDistance(KDVector point, KDNode node) {
            return Math.Abs(GetPointKey(point, node.Level) - GetNodeKey(node));
        }

        #endregion
    }
}