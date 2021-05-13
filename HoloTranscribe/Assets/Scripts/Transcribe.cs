using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.MixedReality.Toolkit;
using TMPro;
using System.Globalization;
using System;
using UnityEngine.SceneManagement;
using System.Threading;

public class Transcribe : MonoBehaviour
{
    public Text recognizedText;
    private string userName;
    public Image background;

    public string SpeechServiceSubscriptionKey = "ab59a9d0ba1048d0976dfe0bb23f082d";
    public string SpeechServiceRegion = "uksouth";
    private string recognizedString;

    private object threadLocker = new object();

    private SpeechRecognizer recognizer;
    private TranslationRecognizer translator;
    private Request requester;
    private AudioSource audioSource;

    private string errorString = "";
    string fromLanguage = "en-us";


    const int FREQUENCY = 16000;

    AudioClip mic;
    int lastPos, pos;
    byte[] byteStream;
 

      void createSpeechRecognizer()
    {
        //Setup speech config based on subscription key
        SpeechConfig config = SpeechConfig.FromSubscription(SpeechServiceSubscriptionKey, SpeechServiceRegion);

        // Define language we are inputing. Only supporting english
        config.SpeechRecognitionLanguage = fromLanguage;

        //Create the speech recognizer from subscription on azure.
        recognizer = new SpeechRecognizer(config);

        // Subscribes to speech events.
        recognizer.Recognizing += RecognizingHandler;
        recognizer.Recognized += RecognizedHandler;
        recognizer.SpeechStartDetected += SpeechStartDetectedHandler;
        recognizer.SpeechEndDetected += SpeechEndDetectedHandler;
        recognizer.Canceled += CanceledHandler;
        recognizer.SessionStarted += SessionStartedHandler;
        recognizer.SessionStopped += SessionStoppedHandler;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Reset transcribed text
        recognizedText.text = "";

        //Confirm we have a microphone, 
        if (Microphone.devices.Length == 0) // if we don't, prompt the user.
        {
            recognizedText.color = Color.red;
            recognizedText.text = "No microphone detected";
        }
        else // If we do, begin a audio-loop of data.
        {
            audioSource = GetComponent<AudioSource>();
            mic = Microphone.Start(null, true, 10, FREQUENCY);
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = AudioClip.Create("byteStream", 10 * FREQUENCY, mic.channels, FREQUENCY, false);
            audio.loop = true;
        }


        // Check player prefrences to confirm if the user has previously opened the applicaiton.
        if (PlayerPrefs.HasKey("Username"))
        {
            //If they have, set the username up.
            userName = PlayerPrefs.GetString("Username");
            Debug.Log($"Username exists: {userName}");            
        }
        else
        {
            // If they haven't, load up the menu and prompt user to choose a profile.
            Debug.Log("Username doesn't exist");
            SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
            StartCoroutine(getMenu());

        }


       

        //Begin listening continously for speech and transcribe when heard.
        StartContinuousRecognition();
    }

    // Get the menu so we can adjust the status text to prompt the user.
    IEnumerator getMenu()
    {
        // Delay to give time for the menu to load.
        Debug.Log("Wating");
        yield return new WaitForSeconds(0.500F);

        //Find the menu, get the profile script and use the setStatus script to amend text.
        GameObject profile = GameObject.Find("Menu");
        NewProfile script = profile.GetComponent<NewProfile>();
        script.setStatus("Please select a user", Color.red);

    }

    // Begin listening for incoming speech.
    private async void StartContinuousRecognition()
    {
        UnityEngine.Debug.LogFormat("Starting Continuous Speech Recognition.");
        //Create the speech recognizer for transcription.
        createSpeechRecognizer();

        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

    }

    //Ran once per a frame.
    void Update()
    {
        // Reset text.
        recognizedText.text = "";
        //Checks if the menu isn't loaded, otherwise, this doesn't need to be executed.
        if (!(SceneManager.GetSceneByName("Menu").isLoaded))
        {
            if ((pos = Microphone.GetPosition(null)) > 0)
            {
                if (lastPos > pos) lastPos = 0;

                if (pos - lastPos > 0)
                {

                    // Allocate the space for the sample.
                    float[] sample = new float[(pos - lastPos) * mic.channels];

                    // Get the data from microphone.
                    mic.GetData(sample, lastPos);

                    // Put the data in the audio source, get the clip and convert to float array.
                    AudioSource audio = GetComponent<AudioSource>();
                    audio.clip.SetData(sample, lastPos);
                    byteStream = GetClipData(audio.clip);

                    // Record current postion of clip.
                    lastPos = pos;
                }
            }
        }
        // Used to update results on screen during updates
        lock (threadLocker)
        {

            //If keywords for closing the menu are detected
            if (recognizedString == "Close menu." || recognizedString == "close menu")
            {
                // Wipe transcription and unload the menu, reset the username as it may have been changed.
                recognizedText.text = "";
                recognizedString = "";
                recognizedText.enabled = true;
                background.enabled = true;
                SceneManager.UnloadSceneAsync("Menu");
                userName = PlayerPrefs.GetString("Username");         
            }

            // If the menu is loaded
            if (SceneManager.GetSceneByName("Menu").isLoaded)
            {       
                //Clear this otherwise another menu will load.
                recognizedString = "";

                // Turn off transcription. 
                recognizedText.enabled = false;
                background.enabled = false;
            }

            // If keywords for showing menu are said
            if (recognizedString == "Show menu." || recognizedString == "show menu")
            {
                // Reset transcription and disable transcription to prevent text moving in the background.
                recognizedText.text = "";
                recognizedString = "";
                recognizedText.enabled = false;
                background.enabled = false;
                SceneManager.LoadScene("Menu", LoadSceneMode.Additive);         
            }

            try
            {
                // If the user wasn't detected
                if (!(requester.ServerMessage == userName))
                {
                    //transcribe to screen
                    recognizedText.enabled = true;
                    recognizedText.text = recognizedString;
                }
            }
            catch
            {
                //No user been returned yet.
                recognizedText.text = recognizedString;
            }

        }
    }
    public void load_menu()
    {

        //Load the menu to the current scene.
        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
    }

    //Returns data from an AudioClip as a byte array.
    public static byte[] GetClipData(AudioClip _clip)
    {
        // Get data
        float[] floatData = new float[_clip.samples * _clip.channels];
        _clip.GetData(floatData, 0);
        // Convert to byte array
        byte[] byteData = new byte[floatData.Length * 4];
        Buffer.BlockCopy(floatData, 0, byteData, 0, byteData.Length);
        return (byteData);
    }

    #region Speech Recognition event handlers
    //Listens for a new session starting e.g. applcation launch
    private void SessionStartedHandler(object sender, SessionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"\n    Session started event. Event: {e.ToString()}.");
    }

    //Listens for session stop, e.g. the user stops talking for a very long period of time.
    private void SessionStoppedHandler(object sender, SessionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"\n    Session event. Event: {e.ToString()}.");
        UnityEngine.Debug.LogFormat($"Session Stop detected. Stop the recognition.");
    }

    //User begins to speeak, is only launched once and doesn't occur again until SpeechEndDetectedHandler() is launched.
    private void SpeechStartDetectedHandler(object sender, RecognitionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"SpeechStartDetected received: offset: {e.Offset}.");
    }


    // User has not spoken for a prolonged period of time.
    private void SpeechEndDetectedHandler(object sender, RecognitionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"SpeechEndDetected received: offset: {e.Offset}.");
        UnityEngine.Debug.LogFormat($"Speech end detected.");
    }

    // "Recognizing" events are fired every time we receive interim results during recognition (i.e. hypotheses)
    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {

        //If we are recognizing speech
        if (e.Result.Reason == ResultReason.RecognizingSpeech)
        { 
            //Check who is talking
            requester = new Request("2", byteStream);
            requester.Start();

            UnityEngine.Debug.LogFormat($"HYPOTHESIS: Text={e.Result.Text}");
            UnityEngine.Debug.LogFormat($"Returned Message: {requester.ServerMessage}");

            //Join thread. Wait for action to complete
            requester.Stop();
            
            // Populate recognized string, ready for transcription.
            recognizedString = e.Result.Text;

            //If the user is detected
            if (requester.ServerMessage == userName)
            {
                // Dont transcribe and disable to stop Update() from changing it.
                recognizedText.enabled = false;
                UnityEngine.Debug.LogFormat($"User found or null value");
                recognizedText.text = "";
                

            }
            else
            {    
                // Transcribe
                recognizedText.text = recognizedString;
            }

           
        }
    }


    // "Recognized" events are fired when the utterance end was detected by the server
    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {

        // If we have detected the utterance ending.
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {

            UnityEngine.Debug.LogFormat($"RECOGNIZED: Text={e.Result.Text}");

            //Set the string, ready to transcribe.
             recognizedString = e.Result.Text;

        }
        else if (e.Result.Reason == ResultReason.NoMatch)
        {
            UnityEngine.Debug.LogFormat($"NOMATCH: Speech could not be recognized.");
        }
    }

    // "Canceled" events are fired if the server encounters some kind of error.
    // This is often caused by invalid subscription credentials.
    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"CANCELED: Reason={e.Reason}");
        errorString = e.ToString();

        if (e.Reason == CancellationReason.Error)
        {
            UnityEngine.Debug.LogFormat($"CANCELED: ErrorDetails={e.ErrorDetails}");
            UnityEngine.Debug.LogFormat($"CANCELED: Did you update the subscription info?");
        }
    }
    #endregion

    //On disabling the application
    void OnDisable()
    {
        try
        {
            //Stop recognition and stop any requests.
            StopRecognition();
            requester.Stop();
        }
        catch
        {
        }
    }

    //Unload events.
    public async void StopRecognition()
    {

        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        recognizer.Recognizing -= RecognizingHandler;
        recognizer.Recognized -= RecognizedHandler;
        recognizer.SpeechStartDetected -= SpeechStartDetectedHandler;
        recognizer.SpeechEndDetected -= SpeechEndDetectedHandler;
        recognizer.Canceled -= CanceledHandler;
        recognizer.SessionStarted -= SessionStartedHandler;
        recognizer.SessionStopped -= SessionStoppedHandler;
        recognizer.Dispose();
        recognizer = null;
        recognizedString = "Speech Recognizer is now stopped.";
        UnityEngine.Debug.LogFormat("Speech Recognizer is now stopped.");
    }
}
