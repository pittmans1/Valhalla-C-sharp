using UnityEngine;
using UnityEngine.UI;

public class VoiceRowUI : MonoBehaviour
{
    [Header("Row Elements")]
    [SerializeField] private Text playerNameText;
    [SerializeField] private Slider volumeSlider;

    private string targetPlayerID;
    public bool isProtectedFromCatActive = MenuController.Instance.catIsntSabotaging;

    

    public void InitalizeRowProfile(string realPlayerID, string displayPlayerName, float currentVolumeSetting)
    {
        targetPlayerID = realPlayerID;

        if(playerNameText != null)
        {
            playerNameText.text = displayPlayerName;
        }

        if(volumeSlider != null)
        {
            volumeSlider.value = currentVolumeSetting;

            //clean up old listeners just in case
            volumeSlider.onValueChanged.RemoveAllListeners();

            // bind slider to targe
            volumeSlider.onValueChanged.AddListener((sliderValue) => OnVolumeSliderMoved(sliderValue));
        }
    }

    private void OnVolumeSliderMoved(float newValue)
    {
        if(VoiceChatBridge.Instance != null)
        {
            VoiceChatBridge.Instance.AdjustRemotePlayerIncomingVolume(targetPlayerID, newValue);
        }

        if (MenuController.Instance != null)
        {
            MenuController.Instance.EvaluateCatInterferenceIntervention(volumeSlider, false);
        }
    }
}