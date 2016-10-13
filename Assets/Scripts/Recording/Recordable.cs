//
// Place this component on and GameObject you want the RecordingManager handle recording.
// So far it works by only recording the transform on the object as it exists on the sever, 
// since the RecordingManager is serverOnly. This ensures we record those objects as any
// client would see them on the network.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public struct RecordingProxy
{
    public NetworkInstanceId target;
    public NetworkInstanceId proxy;

    public RecordingProxy(NetworkInstanceId t, NetworkInstanceId p)
    {
        target = t;
        proxy = p;
    }
}
public class ProxyList : SyncListStruct<RecordingProxy> { }

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
    private IDictionary<NetworkInstanceId, Recording> targetRecordings;
    private float playbackStartTime;
    private float recordingStartTime;
    private float playbackSpeed = 1f;
    private bool paused = false;
    private bool isPlaying = false;
    private bool isRecording = false;

    private RecordingManager manager;

    private ProxyList proxies = new ProxyList();

    public ICollection<Recording> Recordings
    {
        get { return targetRecordings.Values; }
        //set { recording = value; }
    }

    #region MonoBehavior

    public void Start()
    {
        manager = FindObjectOfType<RecordingManager>();

        // register playback and recording handlers with the RecordingManager
        manager.EventPlayStart += CmdHandlePlayStart;
        manager.EventPlayStop += CmdHandlePlayStop;
        manager.EventRecordStart += CmdHandleRecordStart;
        manager.EventRecordStop += CmdHandleRecordStop;
        manager.EventPlaybackSpeed += CmdHandlePlaybackSpeedChange;
        manager.EventPaused += CmdHandlePaused;

        targetRecordings = new Dictionary<NetworkInstanceId, Recording>();
        //localProxies = new Dictionary<GameObject, GameObject>();
        foreach (var target in targets)
        {
            // create new recording for target
            targetRecordings[target.GetComponent<NetworkIdentity>().netId] = new Recording(target.name);
        }
    }

    private void OnDestroy()
    {
        if (manager == null)
        {
            return;
        }
        manager.EventPlayStart -= CmdHandlePlayStart;
        manager.EventPlayStop -= CmdHandlePlayStop;
        manager.EventRecordStart -= CmdHandleRecordStart;
        manager.EventRecordStop -= CmdHandleRecordStop;
        manager.EventPlaybackSpeed -= CmdHandlePlaybackSpeedChange;
        manager.EventPaused -= CmdHandlePaused;
    }

    [ServerCallback]
    private void Update()
    {
        if (isPlaying)
        {
            var timestamp = (Time.realtimeSinceStartup - playbackStartTime) * playbackSpeed;
            foreach (var target in targets)
            {
                var netId = target.GetComponent<NetworkIdentity>().netId;
                var transformData = targetRecordings[netId].Current();
                if (timestamp > 0)
                {
                    transformData = targetRecordings[netId].Next(timestamp);
                }
                else
                {
                    //transformData = targetRecordings[target].Previous();
                }
                var proxy = NetworkServer.FindLocalObject(GetProxyForTarget(netId));
                transformData.ToTransform(proxy.transform);
            }
        }
        else if (isRecording)
        {
            foreach (var target in targets)
            {
                // check if the recordables tranform has moved enough to be worth recording
                var recording = targetRecordings[target.GetComponent<NetworkIdentity>().netId];
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

    public void AddProxy(NetworkInstanceId target, NetworkInstanceId proxy)
    {
        proxies.Add(new RecordingProxy(target, proxy));
        Debug.Log(gameObject.name + " added " + target.Value + " " + proxy.Value);
    }

    private NetworkInstanceId GetProxyForTarget(NetworkInstanceId netId)
    {
        return (from proxy in proxies where proxy.target.Value == netId.Value select proxy.proxy).First();
    }

    #region Event Handlers

    [Server]
    private void CmdHandlePlayStart()
    {
        playbackStartTime = Time.realtimeSinceStartup;
        isPlaying = true;
        Debug.Log(gameObject.name + " is playing.");
    }

    [Server]
    private void CmdHandlePlayStop()
    {
        isPlaying = false;
        // reset recording position
        targetRecordings.Values.ToList().ForEach(r => r.Stop());
        Debug.Log(gameObject.name + " is not playing.");
    }

    [Server]
    private void CmdHandleRecordStart()
    {
        // clear recording
        targetRecordings.Values.ToList().ForEach(r => r.data.Clear());
        recordingStartTime = Time.realtimeSinceStartup;
        isRecording = true;
        Debug.Log(gameObject.name + " is recording.");
    }

    [Server]
    private void CmdHandleRecordStop()
    {
        isRecording = false;
        Debug.Log(gameObject.name + " is not recording.");
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
