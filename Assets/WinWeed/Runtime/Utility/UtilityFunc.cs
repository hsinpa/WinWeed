using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.Winweed.Uti
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

        public static int RandomRange(int min, int max) {
            return random.Next(min, max);
        }

        public static float RandomRange(float min, float max)
        {
            return ((float)random.NextDouble() * (max - min)) + min;
        }

        public static Dictionary<T, K> SetDictionary<T, K>(Dictionary<T, K> dict, T key, K addValue)
        {

            if (dict.ContainsKey(key))
            {
                dict[key] = addValue;
            }
            else
            {
                dict.Add(key, addValue);
            }

            return dict;
        }


        public static Dictionary<string, List<T>> SetListDictionary<T>(Dictionary<string, List<T>> dict, string id, T dataStruct) {
            if (dict.ContainsKey(id)) {
                dict[id].Add(dataStruct);
            } else {
                dict.Add(id, new List<T>() { dataStruct });
            }
            return dict;
        }


        public static void DeleteObject(GameObject p_object)
        {
            if (Application.isPlaying) GameObject.Destroy(p_object);
            if (Application.isEditor) GameObject.DestroyImmediate(p_object);
        }

        public static void DeleteObject(Object p_object)
        {
            if (Application.isPlaying) Object.Destroy(p_object);
            if (Application.isEditor) Object.DestroyImmediate(p_object);
        }
    }
}