# HoloTranscribe

## Overview

This repo was developed for my final year project. This project is a HoloLens application that provides live captioning to users but will identify if the user is speaking and will not transcribe them. It uses a mix of Unity, C# and Python to achive this. See abstract

### Abstract
The possibility of increasing accessibility for those with disabilities using head-mounted displays is immense. This project presents \textit{HoloTranscribe}, an artifact for increasing accessibility for hard of hearing and deaf individuals by providing a real-time transcription to a Microsoft Hololens. The artifact provides a solution to an overlooked problem of similar works; when using automatic speech recognition to transcribe, you will also transcribe the user of the application. Individually trained GMM statistical models are used classify an incoming audio-stream from a Hololens to a Python server used for classification. Based on that classification the audio is transcribed or not. Development of the artifact was conducted in Unity using the Mixed Reality Toolkit while the speaker-recogniser is created in Python, they communicate over a local network.

Results obtained from our speaker-recogniser provided a overall accuracy of 88.7\%, showing the feasibility of the artefact, however limiting factors such as poor performance for short-utterances show that more work is needed before it is ready for personal use. 

## Features

- Azure ASR technology, transcribe live to screen
- NetMQ socket communication between Python server and C# Client in Unity
- User Identification using GMM ML algorithms

# Requirements

## Python

- Python 3.7.3
- See `requirements.txt`

## Unity

- Visual Studio 2019
- Unity 2019 version `2019.4.14f1`
- Mixed Reality ToolKit Unity packages ([ Releases ](https://github.com/Microsoft/MixedRealityToolkit-Unity/releases), [ HoloLens Unity Setup ](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html))
- I've put the Unity packages I used in `Assets\MRTK_Unity_packages` in-case you can no longer download them.
- NetMQ ([ Tutorial Setup ](https://vinnik-dmitry07.medium.com/a-python-unity-interface-with-zeromq-12720d6b7288))
- Microsoft Bisual C++ 14.0.

## Notes

- Configured for HoloLens 1
- All Python Requirements installed in a virtual env (I used Anaconda for the environment)

## Known install issues

### Python Requirements

`PyAudio` can be a pain to install, I know of two ways, install via pipwin

1. `pip install pipwin `
2. `pipwin install PyAudio`

Or use Anaconda to install it via commands or GUI which worked best for me.

# Setup

Follow the below procedure to setup the repo:

1. Install Python requirements `pip install -r requirements.txt` or for Conda: ` conda create --name <env> --file requirements.txt`
2. Download or ensure that the correct Unity version and Unity packages are installed.
3. Check if NetMQ is setup using this tutorial, if not - set it up.
4. When launching the app, make sure two models are trained on different voices.

# Quick Start

**Make sure to install requirements above.**

## Python Isolation

Python code can be ran in isolation, launch using `python server-mimic.py`

### Training the model in isolation

1. When presented, choose `1` and hit enter.
2. Enter your name of the model.
3. You will then be asked to record five pieces, this is to create training data for your voice. It can be viewed in `\Python\Isolation\training_set\` You can speak about anything but I normally used [ this ](https://en.wikipedia.org/wiki/Harvard_sentences) and repeated the list.
4. Once five samples have been recorded, the the model will then be trained on your data, these can be found in `\Python\Isolation\trained_models\`

**Important note:** There must be two models trained, these being different voices. The more distinct the difference, the better.

### Testing the new model in isolation

1. Launch the init file again - `python server-mimic.py`
2. When asked, choose `2` and hit enter.
3. The model will hit an infante loop which will use your mic and tell you who it detects based on the model.

## Running the server

1. Follow install requirements.
2. Launch CMD console in python based environment.
3. Launch `python server.py`

## Full Unity App without a HoloLens

1. Load up the "HoloTranscribe" scene in unity.
2. Fill in your Azure Credentials in the transcribe.cs of the main camera.
3. Ensure sever is running.

## Full Unity App with a HoloLens

1. Load up unity and let all the packages install.
2. Load up the "HoloTranscribe" scene in unity.
3. Fill in your Azure Credentials in the transcribe.cs of the main camera.
4. Build Unity solution
5. Deploy to Hololens.

## Socket Client

In order for C# and Python to communicate I use a TCP/IP protocol in order to send data from one application to another. The Python application is a server found in `\Assets\Python\server.py` and C# client can be found at `Assets\Scripts\sendRequest.cs`.

# Useful Links

Some useful references and links I have got from my work, may help you debug.
Useful links while developing

- HoloLens setup: https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html
- PyAudio error: https://www.youtube.com/watch?v=Laf5Cfh5NRo
- C# / Python socket communication: https://vinnik-dmitry07.medium.com/a-python-unity-interface-with-zeromq-12720d6b7288
- More info on NetMQ for sending and receiving: https://netmq.readthedocs.io/en/latest/receiving-sending/
- Azure SDK examples: https://github.com/Azure-Samples/cognitive-services-speech-sdk/
- Live MFCC code gist: https://gist.github.com/sshh12/62c740b329229c7292f2a7b520b0b6f3
- ZeroMQ guide: https://zguide.zeromq.org/#toc72
- Save AudioClip from unity: https://gist.github.com/darktable/2317063

# Issues

Any problems please raise an issue and I'll help as soon as I can.
