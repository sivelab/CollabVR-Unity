using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.IO;
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

    [SyncEvent]
    public event Action EventPlayStart;
    [SyncEvent]
    public event Action EventPlayStop;
    [SyncEvent]
    public event Action EventRecordStart;
    [SyncEvent]
    public event Action EventRecordStop;

    [SerializeField]
    private Recordable[] recordables;
    [SerializeField]
    public Material proxyMaterial;
    [SerializeField]
    private GameObject recordingButtonPrefab;
    [SerializeField]
    private GameObject recordingsPanel;

    private bool isRecording;
    private bool isPlaying;
    private string recordingsDirectory;
    private string[] recordingFiles;

    #region IsRecording
    public bool IsRecording
    {
        [Server]
        get { return isRecording; }
        [Server]
        set
        {
            if (value)
            {
                // start recording
                // stop playing
                if (IsPlaying)
                {
                    IsPlaying = false;
                }

                RecordStart();
                Debug.Log(name + ": Recording started.");
            }
            else
            {
                // stop recording
                RecordStop();
                Debug.Log(name + ": Recording stopped.");
            }

            isRecording = value;
        }
    }
    #endregion

    #region IsPlaying
    public bool IsPlaying
    {
        [Server]
        get { return isPlaying; }
        [Server]
        set
        {
            if (value)
            {
                // start playing
                // stop recording
                if (IsRecording)
                {
                    IsRecording = false;
                }

                PlayStart();
                //StartCoroutine("Play");
                Debug.Log(name + ": Playback started.");
            }
            else
            {
                // stop playing
                PlayStop();
                //StopCoroutine("Play");
                //StopPlaying();
                Debug.Log(name + ": Playback stopped.");
            }

            isPlaying = value;
        }
    }
    #endregion

    private void OnDestroy()
    {
        instance = null;
    }

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

        UpdateRecordableItems();
        UpdateRecordingsDropdown();
    }

    [Server]
    public void PlayStart()
    {
        if (EventPlayStart != null)
        {
            EventPlayStart();
        }
    }

    [Server]
    public void PlayStop()
    {
        if (EventPlayStop != null)
        {
            EventPlayStop();
        }
    }

    [Server]
    public void RecordStart()
    {
        if (EventRecordStart != null)
        {
            EventRecordStart();
        }
    }

    [Server]
    public void RecordStop()
    {
        if (EventRecordStop != null)
        {
            EventRecordStop();
        }
    }

    [Server]
    public void UpdateRecordableItems()
    {
        if (!isServer)
        {
            return;
        }

        recordables = FindObjectsOfType<Recordable>();
    }

    [Server]
    public void UpdateRecordingsDropdown()
    {
        if (Directory.Exists(recordingsDirectory))
        {
            recordingFiles = Directory.GetFiles(recordingsDirectory);
            // update options
            float previousY = 0;
            foreach (var file in recordingFiles)
            {
                // instantiate a new recording button
                var newButton = Instantiate(recordingButtonPrefab);
                newButton.transform.SetParent(recordingsPanel.transform, false);
                newButton.transform.localPosition = new Vector3(0, previousY, 0);
                var newButtonText = newButton.GetComponentInChildren<Text>();
                newButtonText.text = Path.GetFileName(file);
                // make sure the correct OnClick() function will actually be called
                newButton.GetComponent<Button>().onClick.AddListener(delegate { Read(newButtonText); });
                previousY -= newButton.GetComponent<RectTransform>().rect.height;
            }
        }
    }

    [Server]
    public void Write()
    {
        var filename = recordingsDirectory + string.Format("{0:yyyy-MM-dd-HH-mm-ss}", DateTime.UtcNow) + ".rec";
        Debug.Log("Writing to: " + filename);
        var output = new StringBuilder();
        // write to file
        foreach (var recordable in recordables)
        {
            output.AppendLine(JsonUtility.ToJson(recordable.Recordings));
        }
        File.WriteAllText(filename, output.ToString());
        UpdateRecordingsDropdown();
    }

    [Server]
    public void Read(Text recordingButton)
    {
        // read from file
        var filename = recordingsDirectory + recordingButton.text;
        Debug.Log("Reading from: " + filename);
        var input = File.ReadAllText(filename);
        using (var reader = new StringReader(input))
        {
            string line;
            foreach (var recordable in recordables)
            {
                line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                JsonUtility.FromJsonOverwrite(line, recordable.Recordings);
            }
        }
    }
}
