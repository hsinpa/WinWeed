using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Winweed {
    public class WeedStatic
    {
        public class ShaderProperties {
            public const string Bezier_StartPoint = "u_bezier_startpoint";
            public const string Bezier_StartCtrl = "u_bezier_startctrl";
            public const string Bezier_EndPoint = "u_bezier_endpoint";
            public const string Bezier_EndCtrl = "u_bezier_endctrl";
            public const string Height = "u_height";

        }
    }
}
