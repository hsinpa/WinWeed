using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hsinpa.Winweed.Sample
{
    public class SampleWeedInitiater : MonoBehaviour
    {
        WeedCulling weedCulling;

        // Start is called before the first frame update
        void Start()
        {
            weedCulling = new WeedCulling();

            var weedGenerators = GetComponentsInChildren<WinWeedGeneratorV2>();

            weedCulling.RegisterCamera(Camera.main);
            for (int i = 0; i < weedGenerators.Length; i++) {
                weedCulling.RegisterWeed(weedGenerators[i]);
            }
        }

        // Update is called once per frame
        void Update()
        {
            weedCulling.OnUpdate();
        }
    }
}