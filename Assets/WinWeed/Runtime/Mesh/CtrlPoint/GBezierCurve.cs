using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed {
    public class GBezierCurve
    {
        public static GrassBezierPoint GenerateRandomCurve(float height, float end_point_radius) {

            Vector2 end_point = new Vector2(UtilityFunc.RandomNegativeToOne() * end_point_radius, UtilityFunc.RandomNegativeToOne() * end_point_radius);

            GrassBezierPoint grassBezierPoint = GenerateCurve(height, end_point: end_point, end_factor: -end_point);

            return grassBezierPoint;
        }

        private static GrassBezierPoint GenerateCurve(float height, Vector2 end_point, Vector2 end_factor) {
            GrassBezierPoint grassBezierPoint = new GrassBezierPoint();
            //Assume Start point is alwasy 0,0,0
            grassBezierPoint.start_point = new Vector3(0, 0, 0);

            //Start ctrl
            float start_ctrl_weight = 0.5f;
            float average_height = height * start_ctrl_weight;
            float random_plus_height = UtilityFunc.RandomNegativeToOne() * height * 0.05f;
            float start_ctrl_height = average_height + random_plus_height;
            grassBezierPoint.start_ctrl = new Vector3(0, start_ctrl_height, 0);

            //End point
            grassBezierPoint.end_point = new Vector3(end_point.x, height, end_point.y);

            //End ctrl
            float end_point_y = (UtilityFunc.RandomNegativeToOne() * height * 0.05f) + height;
            float end_point_x = (UtilityFunc.Random() * end_factor.x) + grassBezierPoint.end_point.x;
            float end_point_z = (UtilityFunc.Random() * end_factor.y) + grassBezierPoint.end_point.z;

            grassBezierPoint.end_ctrl = new Vector3(end_point_x, end_point_y, end_point_z);

            return grassBezierPoint;
        }

        public struct GrassBezierPoint {
            public Vector3 start_point;
            public Vector3 start_ctrl;
            public Vector3 end_point;
            public Vector3 end_ctrl;
        }
    }
}