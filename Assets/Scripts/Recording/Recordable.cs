//
// Place this component on and GameObject you want the RecordingManager handle recording.
// So far it works by only recording the transform on the object as it exists on the sever, 
// since the RecordingManager is serverOnly. This ensures we record those objects as any
// client would see them on the network.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Recordable : NetworkBehaviour
{
    private static float deltaThreshhold = 0.001f;

    [SerializeField]
    private GameObject[] targets;

    private IDictionary<GameObject, Recording> targetRecordings;
    private IDictionary<GameObject, GameObject> targetProxies;
    private float playbackStartTime;
    private float recordingStartTime;
    private float playbackSpeed = 1f;
    private bool paused = false;

    public ICollection<Recording> Recordings
    {
        get { return targetRecordings.Values; }
        //set { recording = value; }
    }

    #region MonoBehavior

    private void Start()
    {
        if (!isServer)
        {
            return;
        }

        targetRecordings = new Dictionary<GameObject, Recording>();
        targetProxies = new Dictionary<GameObject, GameObject>();

        // register playback and recording handlers with the RecordingManager
        RecordingManager.Instance.EventPlayStart += HandlePlayStart;
        RecordingManager.Instance.EventPlayStop += HandlePlayStop;
        RecordingManager.Instance.EventRecordStart += HandleRecordStart;
        RecordingManager.Instance.EventRecordStop += HandleRecordStop;
        RecordingManager.Instance.EventPlaybackSpeed += HandlePlaybackSpeedChange;
        RecordingManager.Instance.EventPaused += HandlePaused;

        foreach (GameObject target in targets)
        {
            // create new recording for target
            targetRecordings[target] = new Recording(target.name);

            // create and spawn proxy
            var proxy = Instantiate(target);
            var renderers = proxy.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = RecordingManager.Instance.proxyMaterial;
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
            NetworkServer.Spawn(proxy);
        }
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

    #region Event Handlers

    private void HandlePlayStart()
    {
        playbackStartTime = Time.realtimeSinceStartup;
        foreach (var proxy in targetProxies.Values)
        {
            proxy.SetActive(true);
        }
        StartCoroutine("Play");
    }

    private void HandlePlayStop()
    {
        StopCoroutine("Play");
        // disable proxy
        foreach (var proxy in targetProxies.Values)
        {
            proxy.SetActive(false);
        }
        // reset recording position
        foreach (var recording in targetRecordings.Values)
        {
            recording.Stop();
        }
    }

    private void HandleRecordStart()
    {
        // clear recording
        foreach (var recording in targetRecordings.Values)
        {
            recording.data.Clear();
        }
        recordingStartTime = Time.realtimeSinceStartup;
        StartCoroutine("Record");
    }

    private void HandleRecordStop()
    {
        StopCoroutine("Record");
    }

    private void HandlePlaybackSpeedChange(float speed)
    {
        playbackSpeed = speed;
    }

    private void HandlePaused(bool value)
    {
        paused = value;
    }

    #endregion

    #region Coroutines

    private IEnumerator Play()
    {
        // playback loop
        while (true && !paused)
        {
            foreach (var target in targets)
            {
                var timestamp = (Time.realtimeSinceStartup - playbackStartTime) * playbackSpeed;
                var transformData = targetRecordings[target].Next(timestamp, timestamp > playbackStartTime);
                transformData.ToTransform(targetProxies[target].transform);
            }
            yield return null;
        }
    }

    private IEnumerator Record()
    {
        // recording loop
        while (true)
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
            yield return null;
        }
    }

    #endregion
}
