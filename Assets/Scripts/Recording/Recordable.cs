//
// Place this component on and GameObject you want the RecordingManager handle recording.
// So far it works by only recording the transform on the object as it exists on the sever, 
// since the RecordingManager is serverOnly. This ensures we record those objects as any
// client would see them on the network.
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Recordable : NetworkBehaviour
{
    private static float deltaThreshhold = 0.001f;

    /// <summary>
    /// The GameObjects we are going to record.
    /// </summary>
    [SerializeField]
    private GameObject[] targets;
    public GameObject[] Targets
    {
        get { return targets; }
    }

    /// <summary>
    /// The mapping of GameObjects to their Recordings.
    /// </summary>
    private IDictionary<GameObject, Recording> targetRecordings;
    private IDictionary<GameObject, GameObject> proxies;
    private float playbackStartTime;
    private float recordingStartTime;
    private float playbackSpeed = 1f;
    private bool paused = false;
    private bool isPlaying = false;
    private bool isRecording = false;

    public ICollection<Recording> Recordings
    {
        get { return targetRecordings.Values; }
        //set { recording = value; }
    }

    #region MonoBehavior

    private void Start()
    {
        // register playback and recording handlers with the RecordingManager
        RecordingManager.Instance.EventPlayStart += CmdHandlePlayStart;
        RecordingManager.Instance.EventPlayStop += CmdHandlePlayStop;
        RecordingManager.Instance.EventRecordStart += CmdHandleRecordStart;
        RecordingManager.Instance.EventRecordStop += CmdHandleRecordStop;
        RecordingManager.Instance.EventPlaybackSpeed += CmdHandlePlaybackSpeedChange;
        RecordingManager.Instance.EventPaused += CmdHandlePaused;
    }

    public override void OnStartServer()
    {
        targetRecordings = new Dictionary<GameObject, Recording>();
        proxies = new Dictionary<GameObject, GameObject>();
        foreach (var target in targets)
        {
            // create new recording for target
            targetRecordings[target] = new Recording(target.name);
        }
    }

    [ServerCallback]
    private void Update()
    {
        if (isPlaying)
        {
            var timestamp = (Time.realtimeSinceStartup - playbackStartTime) * playbackSpeed;
            foreach (var target in targets)
            {
                var transformData = targetRecordings[target].Current();
                if (timestamp > 0)
                {
                    transformData = targetRecordings[target].Next(timestamp);
                }
                else
                {
                    //transformData = targetRecordings[target].Previous();
                }
                transformData.ToTransform(proxies[target].transform);
            }
        }
        else if (isRecording)
        {
            foreach (var target in targets)
            {
                // check if the recordables tranform has moved enough to be worth recording
                var recording = targetRecordings[target];
                if (recording.data.Count != 0)
                {
                    var lastTransform = recording.data[recording.data.Count - 1];
                    var difference = Vector3.Distance(lastTransform.position, transform.localPosition)
                        + Quaternion.Angle(lastTransform.rotation, transform.rotation);

                    if (difference > deltaThreshhold)
                    {
                        recording.data.Add(new TransformData(target.transform, Time.realtimeSinceStartup - recordingStartTime));
                    }
                }
                else
                {
                    recording.data.Add(new TransformData(target.transform, Time.realtimeSinceStartup - recordingStartTime));
                }
            }
        }
    }

    #endregion

    public void AddProxy(GameObject target, GameObject proxy)
    {
        proxies[target] = proxy;
        Debug.Log(gameObject.name + " added " + target.name + " " + proxy.name);
    }

    #region Event Handlers

    [Server]
    private void CmdHandlePlayStart()
    {
        playbackStartTime = Time.realtimeSinceStartup;
        isPlaying = true;
    }

    [Server]
    private void CmdHandlePlayStop()
    {
        isPlaying = false;
        // reset recording position
        targetRecordings.Values.ToList().ForEach(r => r.Stop());
    }

    [Server]
    private void CmdHandleRecordStart()
    {
        // clear recording
        targetRecordings.Values.ToList().ForEach(r => r.data.Clear());
        recordingStartTime = Time.realtimeSinceStartup;
        isRecording = true;
    }

    [Server]
    private void CmdHandleRecordStop()
    {
        isRecording = false;
    }

    [Server]
    private void CmdHandlePlaybackSpeedChange(float speed)
    {
        playbackSpeed = speed;
    }

    [Server]
    private void CmdHandlePaused(bool value)
    {
        paused = value;
    }

    #endregion
}
