using System.Collections.Generic;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Services;
using Unity.Barracuda;
using Unity.VisualScripting;
using UnityEngine;


namespace YoloHolo.Services
{
    [System.Runtime.InteropServices.Guid("c585457f-2408-4e23-a6e4-e76612e61058")]
    public class YoloProcessor : BaseServiceWithConstructor, IYoloProcessor
    {
        private readonly YoloProcessorProfile profile;
        private IWorker worker;

        //Stopwatch 생성
        System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();
        float[] dummyOutput = new float[44100];

        public YoloProcessor(string name, uint priority, YoloProcessorProfile profile)
            : base(name, priority)
        {
            this.profile = profile;
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            // Load the YOLOv7 model from the provided NNModel asset
            var model = ModelLoader.Load(profile.Model);

            // Create a Barracuda worker to run the model on the GPU
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
        }

        public async Task<List<YoloItem>> RecognizeObjects(Texture2D texture)
        {
            Debug.Log("Debug2-1. Start RecognizeObjects");
            sw1.Start();
            var inputTensor = new Tensor(texture, channels: profile.Channels);
            await Task.Delay(32);

            Debug.Log("Debug2-2. Call ForwardAsync");

            // Run the model on the input tensor
            var outputTensor = await ForwardAsync(worker, inputTensor);
            //var outputTensor = new Tensor(1, 1, 7, 6300, dummyOutput);
            //Debug.Log("**Output Tensor : " + outputTensor.ToString());

            inputTensor.Dispose();
            
            Debug.Log("Debug2-3. Call GetYoloData");

            
            var yoloItems = outputTensor.GetYoloData(profile.ClassTranslator, 
                profile.MinimumProbability, profile.OverlapThreshold);

            outputTensor.Dispose();
            

            //var yoloItems = new List<YoloItem>();

            Debug.Log("Debug2-4. End RecognizeObjects");
            sw1.Stop();
            Debug.Log("** RecognizeObjects 시간 : " + sw1.ElapsedMilliseconds + "ms");
            sw1.Reset();

            return yoloItems;
        }

        // Nicked from https://github.com/Unity-Technologies/barracuda-release/issues/236#issue-1049168663
        public async Task<Tensor> ForwardAsync(IWorker modelWorker, Tensor inputs)
        {
            Debug.Log("Debug2-2-1. Start ForwardAsync");
            sw2.Start();

            var executor = worker.StartManualSchedule(inputs);
            var it = 0;
            bool hasMoreWork;
            do
            {
                hasMoreWork = executor.MoveNext();
                if (++it % 20 == 0)
                {
                    worker.FlushSchedule();
                    await Task.Delay(32);
                }
            } while (hasMoreWork);

            Debug.Log("Debug2-2-2. End ForwardAsync");
            sw2.Stop();
            Debug.Log("** ForwardAsync 시간 : " + sw2.ElapsedMilliseconds + "ms");
            sw2.Reset();

            return modelWorker.PeekOutput();
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            // Dispose of the Barracuda worker when it is no longer needed
            worker?.Dispose();
        }
    }
}
