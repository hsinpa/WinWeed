using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.Winweed
{
    public class UtilityFunc
    {
        public static float NormalizeByRange(float target, float min, float max)
        {
            return (target - min) / (max - min);
        }

    }
}