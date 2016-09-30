using UnityEngine;
using UnityEngine.Networking;
using VoiceChat;
using VRStandardAssets.Utils;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private Reticle reticle;
    [SerializeField]
    private SelectionRadial radial;
    [SerializeField]
    private OnlineSimpleRotator rotator;
    [SerializeField]
    private OnlineSimpleMovement movement;
    [SerializeField]
    private GameObject[] gameObjectsToEnable;
    [SerializeField]
    private GameObject[] gameObjectsToDisable;
    [SerializeField]
    private Behaviour[] componentsToEnable;
    [SerializeField]
    private Behaviour[] componentsToDisable;

    public void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Cursor.visible = false;
        if (reticle != null)
        {
            reticle.Show();
            if (radial != null)
            {
                radial.Hide();
            }
        }

        // enable microphone stuff
        if (VoiceChatRecorder.Instance.AvailableDevices.Length > 0)
        {
            var device = VoiceChatRecorder.Instance.AvailableDevices[0];
            VoiceChatRecorder.Instance.Device = device;
            VoiceChatRecorder.Instance.StartRecording();
            Debug.Log("Found voice chat device: " + device);
        }
        else
        {
            Debug.Log("No voice chat devices found.");
        }

        /*
        if (!VRDevice.isPresent)
        {
            rotator.enabled = true;
            movement.enabled = true;
        }
        else
        {
            rotator.enabled = false;
            movement.enabled = false;
        }
        */

        foreach (var component in componentsToEnable)
        {
            component.enabled = true;
        }
        foreach (var component in componentsToDisable)
        {
            component.enabled = false;
        }

        foreach (var gameObject in gameObjectsToEnable)
        {
            gameObject.SetActive(true);
        }
        foreach (var gameObject in gameObjectsToDisable)
        {
            gameObject.SetActive(false);
        }
    }
}