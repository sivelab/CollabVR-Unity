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

    /// <summary>
    /// The mapping of GameObjects to their Recordings.
    /// </summary>
    private IDictionary<GameObject, Recording> targetRecordings;
    /// <summary>
    /// The mapping of recorded GameObjects to the proxy recording GameObjects.
    /// </summary>
    private IDictionary<GameObject, GameObject> targetProxies;
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

    [Server]
    private void Start()
    {
        targetRecordings = new Dictionary<GameObject, Recording>();
        targetProxies = new Dictionary<GameObject, GameObject>();

        // register playback and recording handlers with the RecordingManager
        RecordingManager.Instance.EventPlayStart += CmdHandlePlayStart;
        RecordingManager.Instance.EventPlayStop += CmdHandlePlayStop;
        RecordingManager.Instance.EventRecordStart += CmdHandleRecordStart;
        RecordingManager.Instance.EventRecordStop += CmdHandleRecordStop;
        RecordingManager.Instance.EventPlaybackSpeed += CmdHandlePlaybackSpeedChange;
        RecordingManager.Instance.EventPaused += CmdHandlePaused;

        CmdSpawnPrefab();
    }

    private void OnDestroy()
    {
        // destroy proxy
        if (isServer)
        {
            foreach (var proxy in targetProxies.Values)
            {
                NetworkServer.Destroy(proxy);
            }
        }
    }

    #endregion

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
                transformData.ToTransform(targetProxies[target].transform);
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

    #region Event Handlers

    [Server]
    private void CmdHandlePlayStart()
    {
        playbackStartTime = Time.realtimeSinceStartup;
        targetProxies.Values.ToList().ForEach(p => p.SetActive(true));
        RpcEnableProxies(ProxyNetIds());
        isPlaying = true;
    }

    [Server]
    private void CmdHandlePlayStop()
    {
        isPlaying = false;
        // disable proxy
        targetProxies.Values.ToList().ForEach(p => p.SetActive(false));
        RpcDisableProxies(ProxyNetIds());
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

    private void CmdHandlePaused(bool value)
    {
        paused = value;
    }

    #endregion

    [Command]
    private void CmdSpawnPrefab()
    {
        var proxyMaterial = Resources.Load(RecordingManager.Instance.ProxyMaterialName(), typeof(Material)) as Material;

        foreach (var target in targets)
        {
            // create new recording for target
            targetRecordings[target] = new Recording(target.name);

            // create and spawn proxy
            var proxy = Instantiate(target);
            var renderers = proxy.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = proxyMaterial;
            }

            // remove all components we don't need
            var components = proxy.GetComponentsInChildren<Component>();
            foreach (var component in components)
            {
                if (component is MeshFilter
                    || component is MeshRenderer
                    || component is NetworkIdentity
                    || component is NetworkTransform
                    || component is Transform)
                {
                    continue;
                }
                else
                {
                    if (component is Behaviour)
                    {
                        ((Behaviour)component).enabled = false;
                    }
                    else
                    {
                        Destroy(component);
                    }
                }
            }

            // Add a NetworkIdentity to this component if it doesn't have one.
            // This will most likely happen when the target is a child of a prefab.
            var netID = proxy.GetComponent<NetworkIdentity>();
            if (netID == null)
            {
                proxy.AddComponent<NetworkIdentity>();
            }

            // don't make it active until playback
            proxy.SetActive(false);
            ClientScene.RegisterPrefab(proxy);
            NetworkServer.Spawn(proxy);
            // set the proxy mapping
            targetProxies[target] = proxy;
        }

        RpcDisableProxies(ProxyNetIds());
    }

    private NetworkInstanceId[] ProxyNetIds()
    {
        return (from proxy in targetProxies.Values select proxy.GetComponent<NetworkIdentity>().netId).ToArray();
    }

    [ClientRpc]
    private void RpcDisableProxies(NetworkInstanceId[] netIds)
    {
        foreach (var netId in netIds)
        {
            ClientScene.FindLocalObject(netId).gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void RpcEnableProxies(NetworkInstanceId[] netIds)
    {
        foreach (var netId in netIds)
        {
            ClientScene.FindLocalObject(netId).gameObject.SetActive(false);
        }
    }
}
