using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed {
    public class WeedStatic
    {
        public class ShaderProperties {

            //Bezier Curve
            public const string Bezier_StartPoint = "u_bezier_startpoint";
            public const string Bezier_StartCtrl = "u_bezier_startctrl";
            public const string Bezier_EndPoint = "u_bezier_endpoint";
            public const string Bezier_EndCtrl = "u_bezier_endctrl";
            public const string Height = "u_height";

            //Wind
            public const string Wind_Direction = "u_wind_direction";
            public const string Wind_Strength = "u_wind_strength";
        }
        
        [System.Serializable]
        public struct PaintedWeedStruct {
            public Vector3 position;
            public Vector3? normal;

            public float weight;
        }

        [System.Serializable]
        public struct WindConfig{
            public Vector3 wind_direction;
            public float wind_strength;
        }

        public class Color {
            public static Color32 EditColor = new Color32(124, 216, 243, 255);
            public static Color32 EraseColor = new Color32(214, 66, 82, 255);
        }
    }
}
