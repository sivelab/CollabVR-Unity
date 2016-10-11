using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Text;

public class RecordingManager : NetworkBehaviour
{
    #region Instance
    private static RecordingManager instance;

    public static RecordingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RecordingManager>();
            }

            return instance;
        }
    }
    #endregion

    // We need these specific delegates because UNet will complain about
    // using generic parameters with ActionEvents as SyncEvents.
    public delegate void PlaybackSpeedDelegate(float value);
    public delegate void PausedDelegate(bool value);

    [SyncEvent]
    public event Action EventPlayStart;
    [SyncEvent]
    public event Action EventPlayStop;
    [SyncEvent]
    public event Action EventRecordStart;
    [SyncEvent]
    public event Action EventRecordStop;
    [SyncEvent]
    public event PlaybackSpeedDelegate EventPlaybackSpeed;
    [SyncEvent]
    public event PausedDelegate EventPaused;

    public Material proxyMaterial;

    [SerializeField]
    private Recordable[] recordables;
    [SerializeField]
    private GameObject recordingButtonPrefab;
    [SerializeField]
    private GameObject recordingsPanel;

    [SyncVar]
    private bool isRecording;
    [SyncVar]
    private bool isPlaying;
    private string recordingsDirectory;
    private SyncListString recordingFiles = new SyncListString();
    [SyncVar]
    private float duration; // the duration of the current loaded recording
    [SyncVar]
    private float playbackSpeed = 1f;
    [SyncVar]
    private bool paused = false;

    #region Public Properties

    public bool IsRecording
    {
        get { return isRecording; }
        set
        {
            CmdSetIsRecording(value);
        }
    }

    [Command]
    private void CmdSetIsRecording(bool value)
    {
        if (value)
        {
            // start recording
            // stop playing
            if (IsPlaying)
            {
                IsPlaying = false;
            }

            OnRecordStart();
            Debug.Log(name + ": Recording started.");
        }
        else
        {
            // stop recording
            OnRecordStop();
            Debug.Log(name + ": Recording stopped.");
        }

        isRecording = value;
    }

    public bool IsPlaying
    {
        get { return isPlaying; }
        set
        {
            CmdSetIsPlaying(value);
        }
    }

    [Command]
    private void CmdSetIsPlaying(bool value)
    {
        if (value)
        {
            // start playing
            // stop recording
            if (IsRecording)
            {
                IsRecording = false;
            }

            OnPlayStart();
            //StartCoroutine("Play");
            Debug.Log(name + ": Playback started.");
        }
        else
        {
            // stop playing
            OnPlayStop();
            //StopCoroutine("Play");
            //StopPlaying();
            Debug.Log(name + ": Playback stopped.");
        }

        isPlaying = value;
    }

    public float PlaybackSpeed
    {
        get { return playbackSpeed; }
        set
        {
            CmdPlaybackSpeed(value);
        }
    }

    [Command]
    private void CmdPlaybackSpeed(float value)
    {
        playbackSpeed = value;
        OnPlaybackSpeed(playbackSpeed);
    }

    public bool IsPaused
    {
        get { return paused; }
        set
        {
            CmdPaused(value);
        }
    }

    [Command]
    private void CmdPaused(bool value)
    {
        paused = value;
        OnPause(paused);
    }

    #endregion

    #region MonoBehaviour

    [Server]
    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            Debug.LogError("There is only one RecordingManager... and he does not share power.");
            return;
        }

        instance = this;
        isRecording = false;
        isPlaying = false;
        recordingsDirectory = Application.persistentDataPath + "/recordings/";
        if (!Directory.Exists(recordingsDirectory))
        {
            Debug.Log("Creating recording directory: " + recordingsDirectory);
            Directory.CreateDirectory(recordingsDirectory);
        }

        // update recordables
        recordables = FindObjectsOfType<Recordable>();

        // get recordings
        foreach (var file in Directory.GetFiles(recordingsDirectory))
        {
            recordingFiles.Add(file);
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    #endregion

    #region NetworkBehaviour

    public override void OnStartClient()
    {
        recordingFiles.Callback = OnUpdateRecordings;
    }

    #endregion

    #region Event Calls

    private void OnPlayStart()
    {
        if (EventPlayStart != null)
        {
            EventPlayStart();
        }
    }

    private void OnPlayStop()
    {
        if (EventPlayStop != null)
        {
            EventPlayStop();
        }
    }

    private void OnRecordStart()
    {
        if (EventRecordStart != null)
        {
            EventRecordStart();
        }
    }

    private void OnRecordStop()
    {
        if (EventRecordStop != null)
        {
            EventRecordStop();
        }
        UpdateDuration();
    }

    private void OnPlaybackSpeed(float speed)
    {
        if (EventPlaybackSpeed != null)
        {
            EventPlaybackSpeed(speed);
        }
    }

    private void OnPause(bool paused)
    {
        if (EventPaused != null)
        {
            EventPaused(paused);
        }
    }

    #endregion

    [Server]
    private void UpdateDuration()
    {
        // find the last timestamp in all recordables and set the duration with the greatest one
        var maxDuration = 0f;
        foreach (var recordable in recordables)
        {
            foreach (var recording in recordable.Recordings)
            {
                if (recording.Duration() > maxDuration)
                {
                    maxDuration = recording.Duration();
                }
            }
        }
        duration = maxDuration;
    }

    private void OnUpdateRecordings(SyncListString.Operation op, int index)
    {
        var files = recordingFiles.Select(p => Path.GetFileName(p)).ToArray();

        // update options
        float previousY = 0;
        foreach (var file in files)
        {
            // instantiate a new recording button
            var newButton = Instantiate(recordingButtonPrefab);
            newButton.transform.SetParent(recordingsPanel.transform, false);
            newButton.transform.localPosition = new Vector3(0, previousY, 0);
            var newButtonText = newButton.GetComponentInChildren<Text>();
            newButtonText.text = file;
            // make sure the correct OnClick() function will actually be called
            newButton.GetComponent<Button>().onClick.AddListener(delegate { LoadFromTextButton(newButtonText); });
            previousY -= newButton.GetComponent<RectTransform>().rect.height;
        }
    }

    #region IO

    public void SaveRecording()
    {
        CmdWrite();
    }

    [Command]
    private void CmdWrite()
    {
        var filename = recordingsDirectory + string.Format("{0:yyyy-MM-dd-HH-mm-ss}", DateTime.UtcNow) + ".rec";
        Debug.Log("Writing to: " + filename);
        var output = new StringBuilder();
        // write to file
        foreach (var recordable in recordables)
        {
            foreach (var recording in recordable.Recordings)
            {
                output.AppendLine(JsonUtility.ToJson(recording));
            }
        }
        Debug.Log(output.ToString());
        File.WriteAllText(filename, output.ToString());
        recordingFiles.Add(filename);
    }

    public void LoadFromTextButton(Text recordingButton)
    {
        var filename = recordingsDirectory + recordingButton.text;
        CmdRead(filename);
    }

    [Command]
    private void CmdRead(string filename)
    {
        // read from file
        Debug.Log("Reading from: " + filename);
        var input = File.ReadAllText(filename);
        using (var reader = new StringReader(input))
        {
            string line;
            foreach (var recordable in recordables)
            {
                foreach (var recording in recordable.Recordings)
                {
                    line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    Debug.Log(line);
                    JsonUtility.FromJsonOverwrite(line, recording);
                }
            }
        }
    }

    #endregion

    public string ProxyMaterialName()
    {
        return proxyMaterial.name;
    }
}
