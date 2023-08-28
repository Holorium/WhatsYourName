
using System;
using System.Collections.Generic;
using YoloHolo.Services;

namespace YoloHolo.YoloLabeling
{
    [Serializable]
    public class DemoModelTranslator : IYoloClassTranslator
    {
        public string GetName(int classIndex)
        {
            return detectableObjects[classIndex];
            //return "Object" + classIndex;
        }

        private static List<string> detectableObjects = new()
        {
            "Pacu",
            "Redtail Catfish"
        };
    }
}
