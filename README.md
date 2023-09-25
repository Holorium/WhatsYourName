# WhatsYourName
**Introduction: **

What's Your Name is a project that distinguishes fish species in a fish tank.
This project was presented as a prototype and awarded 2nd place at Professor Hyungsin Kim's Ambient AI Bootcamp, a program offered from Graduate School of Data Science, Seoul National University.
Due to the time constraints, the project is able to distinguish 5 different fish species.


**Background: **

An aquarium's purpose is to showcase different fish species or animal species (based on the theme of the aquarium section) to the audience.
Often times, multiple fish species are placed into one fish tank; the descriptions of each species are not enough for the audience to identify each of the species.
Our team noticed that the aquarium is unable to meet its ultimate goal in this area and decided to create an application that would assist the visiting guests on differentiating the fish species.
Considering that this application should assist the audience and not handicap them; Microsoft's HoloLens 2 seemed like the most suiting device to deploy the application to.

**Mechanism: **

What's Your Name uses YOLOv7-tiny, one of the latest object detection models for on-device purpose, and takes input image sizes as 16x16.
HoloLens 2 has both a CPU and a GPU but can only use its CPU for computing operations, meaning that the application can allocate a limited amount of resource for computing.
Thus, the model size was reduced to an extent where the accuracy would not drop too much.

Additionally, the object detection model was converted to ONNX, a tool that is supported on multiple platforms.
Most DNN models are based on Pytorch or Tensorflow, making them incompatible to HoloLens 2's applications which are based on Unity.
Therefore, YOLOv7-tiny model was converted using ONNX so that the DNN could be deployed onto Unity.
