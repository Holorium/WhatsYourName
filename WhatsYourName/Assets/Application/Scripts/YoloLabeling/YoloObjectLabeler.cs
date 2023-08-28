using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Services;
using UnityEngine;
using YoloHolo.Services;
using YoloHolo.Utilities;

namespace YoloHolo.YoloLabeling
{
    public class YoloObjectLabeler : MonoBehaviour
    {
        [SerializeField]
        private GameObject labelObject;

        [SerializeField]
        private int cameraFPS = 4;

        [SerializeField]
        private Vector2Int requestedCameraSize = new(896, 504);

        private Vector2Int actualCameraSize;

        [SerializeField]
        // private Vector2Int yoloImageSize = new(320, 256);
        //private Vector2Int yoloImageSize = new(320, 320);
        private Vector2Int yoloImageSize = new(160, 160);

        [SerializeField]
        private float virtualProjectionPlaneWidth = 1.356f;

        [SerializeField]
        //private float minIdenticalLabelDistance = 0.3f;
        private float minIdenticalLabelDistance = 0.15f;

        [SerializeField]
        //private float labelNotSeenTimeOut = 5f;
        private float labelNotSeenTimeOut = 1.5f;

        [SerializeField]
        private Renderer debugRenderer;

        private WebCamTexture webCamTexture;

        private IYoloProcessor yoloProcessor;

        private readonly List<YoloGameObject> yoloGameObjects = new();

        /*
        //Stopwatch 생성
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        */

        private void Start()
        {
            Debug.Log("Debug0. Application Start");
            yoloProcessor = ServiceManager.Instance.GetService<IYoloProcessor>();
            webCamTexture = new WebCamTexture(requestedCameraSize.x, requestedCameraSize.y, cameraFPS);
            webCamTexture.Play();
            StartRecognizingAsync();
        }

        private async Task StartRecognizingAsync()
        {
            await Task.Delay(1000);

            actualCameraSize = new Vector2Int(webCamTexture.width, webCamTexture.height);
            var renderTexture = new RenderTexture(yoloImageSize.x, yoloImageSize.y, 24);

            if (debugRenderer != null && debugRenderer.gameObject.activeInHierarchy)
            {
                debugRenderer.material.mainTexture = renderTexture;
            }

            while (true)
            {
                //stopwatch.Start();

                Debug.Log("Debug1. Loop Start");
                var cameraTransform = Camera.main.CopyCameraTransForm();
                Graphics.Blit(webCamTexture, renderTexture);
                await Task.Delay(32);

                var texture = renderTexture.ToTexture2D();
                await Task.Delay(32);

                Debug.Log("Debug2. Call RecognizeObjects");

                //stopwatch.Stop();
                //Debug.Log("디버깅1. input 생성 - 시간 : " + stopwatch.ElapsedMilliseconds + "ms");
                //stopwatch.Start();

                var foundObjects = await yoloProcessor.RecognizeObjects(texture);

                //stopwatch.Stop();
                //Debug.Log("디버깅2.  foundObjects - 시간 : " + stopwatch.ElapsedMilliseconds + "ms");
                //stopwatch.Start();

                Debug.Log("Debug3. Call ShowRecognitions");
                ShowRecognitions(foundObjects, cameraTransform);

                Destroy(texture);
                Destroy(cameraTransform.gameObject);


                Debug.Log("Debug4. Loop End");
                //stopwatch.Stop();
                //Debug.Log("디버깅3. Loop 끝 - 시간 : " + stopwatch.ElapsedMilliseconds + "ms");
                //stopwatch.Reset();

            }
        }


        private void ShowRecognitions(List<YoloItem> recognitions, Transform cameraTransform)
        {
            Debug.Log("Debug3-1. ShowRecognitions Start");
            
            foreach (var recognition in recognitions)
            {
                var newObj = new YoloGameObject(recognition, cameraTransform,
                    actualCameraSize, yoloImageSize, virtualProjectionPlaneWidth);

                //position이 존재하고 기존 yoloGameObjects에 없는 Obj를 yoloGameObjects에 추가
                if (newObj.PositionInSpace != null && !HasBeenSeenBefore(newObj))
                {
                    yoloGameObjects.Add(newObj);
                    newObj.DisplayObject = Instantiate(labelObject,
                        newObj.PositionInSpace.Value, Quaternion.identity);
                    newObj.DisplayObject.transform.parent = transform;
                    var labelController = newObj.DisplayObject.GetComponent<ObjectLabelController>();
                    labelController.SetText(newObj.Name);
                    Destroy(newObj.DisplayObject, labelNotSeenTimeOut);
                }
            }

            //yoloGameObjects에서 생성된 지 오래된 (> labelNotSeenTimeOut = 5f = 5초) obj 제거
            for (var i = yoloGameObjects.Count - 1; i >= 0; i--)
            {
                if (Time.time - yoloGameObjects[i].TimeLastSeen > labelNotSeenTimeOut)
                {
                    yoloGameObjects.RemoveAt(i);
                }
            }
            

/*
            foreach (var recognition in recognitions)
            {
                var newObj = new YoloGameObject(recognition, cameraTransform,
                    actualCameraSize, yoloImageSize, virtualProjectionPlaneWidth);

                //position 존재하는 Obj를 yoloGameObjects에 추가
                if (newObj.PositionInSpace != null)
                {
                    yoloGameObjects.Add(newObj);
                    newObj.DisplayObject = Instantiate(labelObject,
                        newObj.PositionInSpace.Value, Quaternion.identity);
                    newObj.DisplayObject.transform.parent = transform;
                    var labelController = newObj.DisplayObject.GetComponent<ObjectLabelController>();
                    labelController.SetText(newObj.Name);
                    Destroy(newObj.DisplayObject, labelNotSeenTimeOut);
                }
            }

            yoloGameObjects.Clear();
*/
            Debug.Log("Debug3-2. ShowRecognitions End");
        }

        private bool HasBeenSeenBefore(YoloGameObject obj)
        {
            /*
             * yoloGameObjects에 obj와 같은(Name이 동일하고 Distance가 minIdenticalLabelDistance 이내)
             * object가 존재하면 해당 object의 TimeLastSeen을 update하고 True 리턴
             * 존재하지 않으면 False 리턴
             */
             
            var seenBefore = yoloGameObjects.FirstOrDefault(
                ylo => ylo.Name == obj.Name &&
                Vector3.Distance(obj.PositionInSpace.Value, ylo.PositionInSpace.Value) < minIdenticalLabelDistance);
            if (seenBefore != null)
            {
                seenBefore.TimeLastSeen = Time.time;
            }
            return seenBefore != null;
        }
    }
}
