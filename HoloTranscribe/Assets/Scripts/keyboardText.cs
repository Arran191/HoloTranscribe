using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.Experimental.UI
{
    /// <summary>
    /// This component links the NonNativeKeyboard to a TMP_InputField
    /// Put it on the TMP_InputField and assign the NonNativeKeyboard.prefab
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class keyboardText : MonoBehaviour, IPointerDownHandler
    {
        [Experimental]
        [SerializeField] private NonNativeKeyboard keyboard = null;

        public void OnPointerDown(PointerEventData eventData)
        {
            //Add event
            keyboard.PresentKeyboard();
            keyboard.OnClosed += DisableKeyboard;
            keyboard.OnTextSubmitted += DisableKeyboard;
            keyboard.OnTextUpdated += UpdateText;
            keyboard.SubmitOnEnter = true;
            keyboard.OnTextSubmitted += enterClicked;
        }

        private void UpdateText(string text)
        {
            //Update the text in the text field.
            GetComponent<TMP_InputField>().text = text;
        }

        //Enter clicked on keyboard.
        private void enterClicked(object sender, EventArgs e)
        {      
            // Get new profile script on game object and start creating a new profile.
            GameObject profile = GameObject.Find("Menu");
            NewProfile script = profile.GetComponent<NewProfile>();
            Debug.Log($"Username in keybored text: {GetComponent<TMP_InputField>().text}");
            script.createNewProfile();
        }
        private void DisableKeyboard(object sender, EventArgs e)
        {
            //Remove events and close keyboard.
            keyboard.OnTextUpdated -= UpdateText;
            keyboard.OnClosed -= DisableKeyboard;
            keyboard.OnTextSubmitted -= DisableKeyboard;
            keyboard.OnTextSubmitted -= enterClicked;
            keyboard.Close();
        }
    }
}