using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class SDKSpeech : MonoBehaviour
{

    async static Task FromMic(SpeechConfig speechConfig)
    {
        var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        Console.WriteLine("Speak into your microphone.");
        var result = await recognizer.RecognizeOnceAsync();
        Debug.Log($"RECOGNIZED: Text={result.Text}");
    }

    // Start is called before the first frame update
    async void Start()
    {
        Debug.Log("Starting new session");
        var speechConfig = SpeechConfig.FromSubscription("<paste-your-subscription-key>", "<paste-your-region>");
        await FromMic(speechConfig);
    }

    // Update is called once per frame
    void Update()
    {
    }


}
