using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using System.Threading.Tasks;
using UnityEngine;

namespace YoloHolo.Services
{
    public static class TensorExtensions
    {
        static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public static List<YoloItem> GetYoloData(this Tensor tensor, IYoloClassTranslator translator, 
            float minProbability, float overlapThreshold)
        {
            Debug.Log("Debug2-3-1. Start GetYoloData");
            sw.Start();

            var boxesMeetingConfidenceLevel = new List<YoloItem>();
/*
            Parallel.For(0, tensor.channels, (i) =>
            {
                Debug.Log("Debug. Parallel For"+i);
                var yoloItem = new YoloItem(tensor, i, translator);
                if (yoloItem.Confidence > minProbability)
                {
                    boxesMeetingConfidenceLevel.Add(yoloItem);
                }
            });
*/

            for (var i = 0; i < tensor.channels; i++)
            {
                var yoloItem = new YoloItem(tensor, i, translator);
                if (yoloItem.Confidence > minProbability)
                {
                    boxesMeetingConfidenceLevel.Add(yoloItem);
                }
            }

            sw.Stop();
            Debug.Log("** GetYoloData-After First Loop: " + sw.ElapsedMilliseconds + "ms");
            sw.Start();

            var result = new List<YoloItem>();
            var recognizedTypes = boxesMeetingConfidenceLevel.Select(b => b.MostLikelyObject).Distinct();
            foreach (var objType in recognizedTypes)
            {
                var boxesOfThisType = boxesMeetingConfidenceLevel.Where(b => b.MostLikelyObject == objType).ToList();
                result.AddRange(RemoveOverlappingBoxes(boxesOfThisType, overlapThreshold));
            }

            Debug.Log("Debug2-3-2. End GetYoloData");
            sw.Stop();
            Debug.Log("** GetYoloData 시간 : " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();
            return result;
        }

        // Code below largely courtesy of ChatGPT
        private static List<YoloItem> RemoveOverlappingBoxes(
            List<YoloItem> boxesMeetingConfidenceLevel, 
            float overlapThreshold)
        {
            // sort the boxesMeetingsConfidenceLevel by their confidence score in descending order  
            boxesMeetingConfidenceLevel.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
            var selectedBoxes = new List<YoloItem>();

            // loop through each box and check for overlap with higher-confidence boxesMeetingsConfidenceLevel  
            while (boxesMeetingConfidenceLevel.Count > 0)
            {
                var currentBox = boxesMeetingConfidenceLevel[0];
                selectedBoxes.Add(currentBox);
                boxesMeetingConfidenceLevel.RemoveAt(0);

                // compare the current box with all re
                //
                // ing boxesMeetingsConfidenceLevel  
                for (var i = 0; i < boxesMeetingConfidenceLevel.Count; i++)
                {
                    var otherBox = boxesMeetingConfidenceLevel[i];
                    var overlap = ComputeIoU(currentBox, otherBox);
                    if (overlap > overlapThreshold)
                    {
                        // remove the box if it has a high overlap with the current box  
                        boxesMeetingConfidenceLevel.RemoveAt(i);
                        i--;
                    }
                }
            }

            return selectedBoxes;
        }

        private static float ComputeIoU(YoloItem boxA, YoloItem boxB)
        {
            var xA = Math.Max(boxA.TopLeft.x, boxB.TopLeft.y);
            var yA = Math.Max(boxA.TopLeft.y, boxA.TopLeft.y);
            var xB = Math.Min(boxA.BottomRight.x, boxB.BottomRight.x);
            var yB = Math.Min(boxA.BottomRight.y, boxB.BottomRight.y);

            var intersectionArea = Math.Max(0, xB - xA + 1) * Math.Max(0, yB - yA + 1);
            var boxAArea = boxA.Size.x * boxA.Size.y;
            var boxBArea = boxB.Size.y * boxB.Size.y;
            var unionArea = boxAArea + boxBArea - intersectionArea;

            return intersectionArea / unionArea;
        }
    }
}
