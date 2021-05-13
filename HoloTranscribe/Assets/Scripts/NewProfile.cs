using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit;
using System.Linq;

// ref https://www.youtube.com/watch?v=WCgd_HY1F14

// This class is used for any menu related tasks, including creating new profiles.
public class NewProfile : MonoBehaviour
{

    public TextMeshPro status;
    public GameObject inputCanvas;
    public GameObject Menu;
    public TMP_InputField profileName;
    public TMP_Dropdown dropdown;
    public Interactable newProfileButton;
    private int totalProfiles;

    private string[] profileList { get; set; }
    public string username { get; set; }


    //On start
    private void Start()
    {
        //Load the dropdown.
        LoadDropdown();
    }
    void LoadDropdown()
    {

        status.color = Color.black;
        status.SetText("");

        //Hide training textbox
        showCanvas(false);

        //Get current user profiles.
        List<string> options = getUserProfiles();

        //Confirm we have a username
        if (PlayerPrefs.HasKey("Username"))
        {
            //If we do, get the username.
            username = PlayerPrefs.GetString("Username");

            //Clear the dropdown of values then add the profiles in.
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = dropdown.options.FindIndex(option => option.text == username);

            Debug.Log($"User value {dropdown.value}");
            Debug.Log($"Username exists5: {username}");
        }
        else
        {
            //If we don't have a user profile
            Debug.Log($"Username doesn't exist: {username}");

            //Add a blank so not to pick user by default.
            options.Add("");

            //Clear and add options. 
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            //Set it to the the "" value.
            dropdown.value = options.Count;
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //When the scene is loaded, reload the dropdown.
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        LoadDropdown();
    }

    public void setStatus(string text, Color statusColour)
    {
        // Set the colour and text of the status bar.
        status.text = text;
        status.color = statusColour;
    }

    //When the user changes a profile, this function is launched.
    public void changedProfile()
    {
        status.color = Color.black;
        Debug.Log($"Profile changed before:{username}");

        //Set the username to the current selected value.
        username = dropdown.options[dropdown.value].text;

        //Count the options
        int optionsCount = dropdown.options.Count;
        //If we still have "" in our options
        if ((optionsCount > totalProfiles) & (username != ""))
        {
            //Remove it.
            dropdown.options.RemoveAt(totalProfiles);
            dropdown.RefreshShownValue();
        }
       
        Debug.Log($"Profile changed after:{username}");

        //Set new username as we've changed profiles.
        PlayerPrefs.SetString("Username",username );
    }
    private void showCanvas(bool view)
    {
        //Show the text field
        inputCanvas.SetActive(view);
    }
    public void newProfile()
    {
        //New profile button has been clicked, show the text box.
        setStatus("Please enter a profile name", Color.black);
        showCanvas(true);
   
    }


    //Enter is clicked on the keyboard.
    public void createNewProfile()
    {
        // If user has left text-box blank
        if (profileName.text == null)
        {
            //Tell them to add something in it.
            setStatus("Do not leave the input field blank", Color.red);
        }
        else
        {
            //Remove the text box, it isn't needed anymore.
            showCanvas(false);
            
            //Stop user interactions
            newProfileButton.enabled = false;
            dropdown.enabled = false;

            //Set current username to the new profile
            username = profileName.text;

            //Begin training process by recording data.
            record();
        }
    }

    private List<string> getUserProfiles()
    {     
        //Send request for new profiles
        Request request = new Request("3");
        request.Start();
        request.Stop();

        //Split the returned list. Need to do this as ServerMessage is a string and not an array.
        profileList = request.ServerMessage.Split(',');

        //Create a list from our split list.
        List<string> listProfile = new List<string>(profileList);

        //Used for checking for the loose "" profile when no user is selected.
        totalProfiles = listProfile.Count;
        return listProfile;

    }

    //Record training data
    private void record()
    {

        setStatus("Say the words on screen, keep repeating them! ", Color.black);

        //Setup audio.
        AudioSource audio;
        audio = GetComponent<AudioSource>();      

        //Begin Corotuine for creating the files.
        StartCoroutine(createFile(audio));     
    }

    IEnumerator createFile(AudioSource audio)
    {

        //Give some information to the user. Use the wait to let the user read it.
        setStatus("We need to record your voice for 60 seconds. ", Color.black);
        yield return new WaitForSeconds(2);
        setStatus("Start speaking now until complete is shown. ", Color.black);
        yield return new WaitForSeconds(3);


        Request requester;
        for (int i = 0; i <5; i++)
        {
            Debug.Log($"-------------------ITERATION{i}");
            //Record clip for 20 seconds at rate of 16000.
            audio.clip = Microphone.Start(null, false, 20, 16000);
            status.SetText($"Recording...{i+1}/5");

            //Wait until recording is done.
            yield return new WaitForSeconds(audio.clip.length);

            //Save the file
            setStatus("Saving File", Color.black);
            Debug.Log($"--------- Saving file --------");

            //Save the file locally.
            SavWav.Save("AudioClip-" + i, audio.clip);
            setStatus("File saved", Color.black);

            //Get the file path
            string filepath = Application.persistentDataPath + "/AudioClip-" + i + ".wav";
            status.SetText("Set path");

            //Read in as byte format. This stops distortion rather than just sending the clip over and writting it there.
            byte[] byteAudio = File.ReadAllBytes(filepath);

            //Send training data over to the server.
            requester = new Request("1", username, byteAudio);
            requester.Start();
            requester.Stop();
            File.Delete(filepath);

        }
        setStatus("Training Model", Color.black);

        //Send request to train model.
        requester = new Request("4", username);
        requester.Start();
        requester.Stop();

        //Wait for model to be trained. Takes roughly 2-3 secs.
        yield return new WaitForSeconds(4);

        //Renable the buttons.
        newProfileButton.enabled = true;
        dropdown.enabled = true;

        //Reload the dropdown
        LoadDropdown();
 
    }


    //deleting a user profile
    public void deleteProfile()
    {
        Debug.Log("Deleting profile");
        status.color = Color.black;
        status.text = "Deleted profile";

        //If the user has deleted all profiles
        if (dropdown.options.Count - 1 == 0)
       {
          // Tell them they now have a problem.
          setStatus("No user profiles left", Color.red);
          //Adding a silly list because it won't let me just add a string..
           List<string> m_DropOptions = new List<string> { "" };
           dropdown.AddOptions(m_DropOptions);
           dropdown.RefreshShownValue();
       }

        int index = dropdown.options.FindIndex(option => option.text == username);
        dropdown.options.RemoveAt(index);

        //Sent request to delete profile
        Request request = new Request("5", username);
        request.Start();
        request.Stop();
        Debug.Log($"Username 1: {username}");
        //Delete locally saved username.
        PlayerPrefs.DeleteKey("Username");

        //Refresh the dropdown.
        dropdown.RefreshShownValue();           
        dropdown.value = dropdown.options.FindIndex(option => option.text == username);
        username = dropdown.options[dropdown.value].text;

        //Set new username
        PlayerPrefs.SetString("Username", username);
        Debug.Log($"Username 2: {username}");
        Debug.Log($"Username player prefs: {PlayerPrefs.GetString("Username")}");

    }
  

    public void setActive(bool active)
    {
        //Activate the menu
        Menu.SetActive(active);
    }

}
