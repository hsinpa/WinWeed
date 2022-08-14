using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.Winweed
{
    public class UtilityFunc
    {
        private static System.Random random = new System.Random();

        public static void SetRandomSeed(int seed) {
            random = new System.Random(seed);
        }

        public static float NormalizeByRange(float target, float min, float max)
        {
            return (target - min) / (max - min);
        }

        //0f-1f
        public static float Random()
        {
            return (float)random.NextDouble();
        }

        public static float RandomNegativeToOne()
        {
            return UtilityFunc.ScaleFloat_Clip_Space(UtilityFunc.Random());
        }

        public static float ScaleFloat_Clip_Space(float value)
        {
            return value * 2 - 1;
        }

        public static float ScaleFloat_UV_Space(float value)
        {
            return value + 1 * 0.5f;
        }
    }
}