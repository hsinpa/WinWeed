using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hsinpa.Winweed.Uti
{
    public class CollisionUti
    {
        public static IntersectionResult IntersectionPlane(Vector3 planePos, Vector3 planeNormal, Vector3 rayPos, Vector3 rayNormal)
        {
            var denom = Vector3.Dot(planeNormal, rayNormal);
            IntersectionResult result = new IntersectionResult();

            if (denom > 1e-6)
            {
                Vector3 p010 = planePos - rayPos;
                float t = Vector3.Dot(p010, planeNormal) / denom;

                result.t = t;
                result.valid = t >= 0;
                return result;
            }

            return result;
        }

        public static Vector2 AABBOverlapBox(Bounds boundA, Bounds boundB) {
            //Cal Diff
            Vector2 overlapArea = boundA.center - boundB.center;
            overlapArea.x = Mathf.Abs(overlapArea.x);
            overlapArea.y = Mathf.Abs(overlapArea.y);

            //Cal overlapArea
            overlapArea.x = (boundA.extents.x + boundB.extents.x) - overlapArea.x;
            overlapArea.y = (boundA.extents.y + boundB.extents.y) - overlapArea.y;

            return overlapArea;
        }

        public static bool PointBoxCollision(Vector2 point, Vector2 topRight, Vector2 topLeft, Vector2 bottomRight, Vector2 bottomLeft)
        {
            return (point.x > topLeft.x && point.y < topLeft.y) && //Top Left
                   (point.x < topRight.x && point.y < topRight.y) && //Top Right
                   (point.x > bottomLeft.x && point.y > bottomLeft.y) && //Bottom Left
                   (point.x < bottomRight.x && point.y > bottomRight.y); //Bottom Right
        }

        public static bool PointCircleCollision(Vector2 point, Vector2 targetPos, float radius)
        {
            return Vector2.Distance(point, targetPos) < radius; 
        }

        public static Vector3 PointLineProjection(Vector3 point, Vector3 lineA, Vector3 lineB) {
            return Vector3.Project( (point - lineA), (lineB - lineA) ) + lineA;
        }

        public static float Normalized(float targetMin, float targetMax, float refMin, float refMax, float m)
        {
            return (((m - refMin) * (targetMax - targetMin)) / (refMax - refMin)) + targetMin;
        }

        public static void ButtonHighLight(Button highlightBtn, Button[] buttonGroup, Color highlightColor, Color idleColor)
        {

            if (buttonGroup == null || highlightBtn == null) return;

            for (int i =0 ; i < buttonGroup.Length; i++)
            {
                Color c = highlightBtn == buttonGroup[i] ? highlightColor : idleColor;

                buttonGroup[i].image.color = c;
            }
        }

        public static Vector2 GetPlaneIntersectUV(Vector2 planeCenter, Vector2 planeSize, Vector2 landingPos, out Vector2 uv)
        {
            float radiusX = planeSize.x * 0.5f;
            float radiusY = planeSize.y * 0.5f;

            planeCenter.y = planeCenter.y - radiusY;
            planeCenter.x = planeCenter.x - radiusX;

            uv = landingPos - planeCenter;
            uv.y = CollisionUti.Normalized(0, 1, 0, planeSize.y, uv.y);
            uv.x = CollisionUti.Normalized(0, 1, 0, planeSize.x, uv.x);

            return uv;
        }

        public static bool IsUVValid(Vector2 uv)
        {
            return uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1;
        }

        public struct IntersectionResult
        {
            public bool valid;
            public float t;
        }
    }
}