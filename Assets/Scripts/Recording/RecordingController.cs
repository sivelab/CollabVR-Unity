using UnityEngine;
using UnityEngine.Networking;

public class RecordingController : NetworkBehaviour
{
    [SerializeField]
    private RecordingManager recordingManager;

    [SerializeField]
    private KeyCode playKey = KeyCode.P;

    [SerializeField]
    private KeyCode recordKey = KeyCode.O;

    private void Start()
    {
        if (recordingManager == null)
        {
            recordingManager = FindObjectOfType<RecordingManager>();
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // check input
        if (Input.GetKeyDown(playKey))
        {
            CmdTogglePlaying();
        }
        else if (Input.GetKeyDown(recordKey))
        {
            CmdToggleRecording();
        }
    }

    [Command]
    private void CmdTogglePlaying()
    {
        recordingManager.IsPlaying = !recordingManager.IsPlaying;
    }

    [Command]
    private void CmdToggleRecording()
    {
        recordingManager.IsRecording = !recordingManager.IsRecording;
    }
}
