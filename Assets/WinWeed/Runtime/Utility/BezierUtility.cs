using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Utility {
    public class BezierUtility
    {

        public static Vector3 BezierCurve(Vector3 StartPoint, Vector3 StartCtrlPoint, Vector3 EndCtrlPoint, Vector3 EndPoint, float t)
        {

            Vector3 PS = Vector3.Lerp(StartPoint, StartCtrlPoint, t);
            Vector3 PE = Vector3.Lerp(EndCtrlPoint, EndPoint, t);

            Vector3 CC = Vector3.Lerp(StartCtrlPoint, EndCtrlPoint, t);

            Vector3 SC = Vector3.Lerp(PS, CC, t);
            Vector3 EC = Vector3.Lerp(CC, PE, t);

            return Vector3.Lerp(SC, EC, t);
        }

        public static Vector3 RotateTriangle(Vector3 point_a, Vector3 point_b, Vector3 point_c, Vector3 direction) {

            Vector3 centerPoint = GetTriangleCenter(point_a, point_b, point_c);

            Quaternion rotation = Quaternion.Euler(direction.x, direction.y, direction.z);
            Matrix4x4 m = Matrix4x4.Rotate(rotation);


            return Vector3.zero;
        }

        public static Vector3 GetTriangleCenter(Vector3 point_a, Vector3 point_b, Vector3 point_c) {
            float x = (point_a.x + point_b.x + point_c.x) / 3f;
            float y = (point_a.y + point_b.y + point_c.y) / 3f;
            float z = (point_a.z + point_b.z + point_c.z) / 3f;

            return new Vector3(x, y ,z);
        }

        public static Vector3 VectorAddition(Vector3 vector_a, float scalar){
            vector_a.Set(scalar + vector_a.x, scalar + vector_a.y, scalar + vector_a.z);

            return vector_a;
        }

        public static Vector3 VectorMulti(Vector3 vector_a, Vector3 vector_b)
        {
            return new Vector3(vector_a.x * vector_b.x, vector_a.y * vector_b.y, vector_a.z * vector_b.z);
        }

    }
}
