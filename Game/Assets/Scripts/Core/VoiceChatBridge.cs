using System.Collections.Generic;
using UnityEngine;

public class VoiceChatBridge : MonoBehaviour
{
    public static VoiceChatBridge Instance;

    [Header("Local Microphone Status")]
    public string activeMicrophoneDevice = "None Detected";
    public float localMicInputSensitivity = 1.0f;
    private AudioClip micRecordingClip;

    [Header("Network Players Volume Attenuation Map")]

    private Dictionary<string, float> remotePlayerVolumeMap = new Dictionary<string, float>();

    private void Awake ()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start ()
    {
        ScanAndInitializeLocalMicrophone();
    }

    public void ScanAndInitializeLocalMicrophone ()
    {
        if (Microphone.devices.Length > 0)
        {
            activeMicrophoneDevice = Microphone.devices[0];
            Debug.Log($"Voice Chat initialized: Linked to {activeMicrophoneDevice}");

            micRecordingClip = Microphone.Start(activeMicrophoneDevice, true, 1, 44100);
        }
        else
        {
            Debug.LogWarning("NO microphone Hardware detected on this Device Profiles");
        }
    }

    /// <summary>
    /// checks real time physical micro-amplitude spike levels of your voice input.
    /// </summary>
    /// <returns></returns>
    public float GetCurrentMicAmplitude ()
    {
        if (micRecordingClip == null || !Microphone.IsRecording(activeMicrophoneDevice)) return 0f;
        
        int sampleWindowSize = 128;
        float[] waveDataSamples = new float[sampleWindowSize];
        int micPosition = Microphone.GetPosition(activeMicrophoneDevice) - sampleWindowSize;

        if (micPosition < 0) return 0f;

        micRecordingClip.GetData(waveDataSamples, micPosition);

        float peakAmplitude = 0f;
        for (int i = 0; i< sampleWindowSize; i++)
        {
            float absoluteVal = Mathf.Abs(waveDataSamples[i]);
            if (absoluteVal > peakAmplitude) peakAmplitude = absoluteVal;

        }
        return peakAmplitude * localMicInputSensitivity;
    }

    public void SetLocalMicSensitivity(float sliderValue)
    {
        localMicInputSensitivity = sliderValue;
        Debug.Log($"local mic sensitivity scale updated to: {sliderValue}");

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="individualVolumeScale"></param>
    public void AdjustRemotePlayerIncomingVolume(string playerID, float individualVolumeScale)
    {
        if (remotePlayerVolumeMap.ContainsKey(playerID))
            remotePlayerVolumeMap[playerID] = individualVolumeScale;
        else 
            remotePlayerVolumeMap.Add(playerID, individualVolumeScale);

        // TODO : final network audio framework or native (photon voice or vivax) pass float value straigh to player speaker stream.
    }

}