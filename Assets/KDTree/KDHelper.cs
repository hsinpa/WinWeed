using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Hsinpa.Algorithm.KDTree.KDStruct;

namespace Hsinpa.Algorithm.KDTree
{
    public class KDHelper
    {
        public struct PartitionStruct {
            public List<KDStruct.KDVector> right;
            public List<KDStruct.KDVector> left;
            public KDStruct.KDVector median;
        }

        private static void Swap(ref float a, ref float b) {
            float temp = a + 0;

            a = b;
            b = temp;
        }

        private static int Partition(ref List<KDStruct.KDVector> point, int index, int front, int end) {
            float pivot = point[end].data[index];
            int i = front - 1;

            for (int j = front; j < end; j++) {
                if (point[j].data[index] < pivot) {
                    i++;
                    Swap(ref point[i].data[index], ref point[j].data[index]);
                }
            }

            i++;
            Swap(ref point[i].data[index], ref point[end].data[index]);
            return i;
        }

        public static void QuickSort(int axis, int front, int end, ref List<KDStruct.KDVector> point) {
            PartitionStruct partitionStruct = new PartitionStruct();
            partitionStruct.right = new List<KDVector>();
            partitionStruct.left = new List<KDVector>();

            if (front < end) {
                int pivot = Partition(ref point, axis, front, end);
                QuickSort(axis, front, pivot - 1, ref point);
                QuickSort(axis, pivot + 1, end, ref point);
            }
        }

        public static PartitionStruct GetPartitionStruct(int axis, List<KDStruct.KDVector> sort_point) {
            PartitionStruct partitionStruct = new PartitionStruct();

            if (sort_point == null || sort_point.Count == 0) return partitionStruct;

            int median = Mathf.FloorToInt(sort_point.Count / 2f);

            partitionStruct.median = sort_point[median];

            if (median > 0)
                partitionStruct.left = sort_point.GetRange(0, median - 1);

            if (median + 1 < sort_point.Count)
                partitionStruct.right = sort_point.GetRange(median + 1, sort_point.Count - (median + 1));

            return partitionStruct;
        }

    }
}