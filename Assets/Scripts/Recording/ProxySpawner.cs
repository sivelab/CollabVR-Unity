using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NetIdList : SyncListStruct<NetworkInstanceId> { }

public class ProxySpawner : NetworkBehaviour
{
    private Recordable[] recordables;
    private RecordingManager manager;

    private NetIdList proxies = new NetIdList();
    private Material proxyMaterial;

    public void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        manager = FindObjectOfType<RecordingManager>();
        manager.EventPlayStart += OnPlayStart;
        manager.EventPlayStop += OnPlayStop;

        CmdRespawn();
    }

    [Command]
    public void CmdRespawn()
    {
        manager = FindObjectOfType<RecordingManager>();
        proxyMaterial = Resources.Load(manager.ProxyMaterialName(), typeof(Material)) as Material;
        foreach (var proxy in proxies)
        {
            ClientScene.UnregisterPrefab(NetworkServer.FindLocalObject(proxy));
            NetworkServer.Destroy(NetworkServer.FindLocalObject(proxy));
        }
        proxies.Clear();
        recordables = FindObjectsOfType<Recordable>();

        foreach (var recordable in recordables)
        {
            recordable.ClearProxies();

            foreach (var target in recordable.Targets)
            {
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
                        Destroy(component);
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
                proxies.Add(proxy.GetComponent<NetworkIdentity>().netId);
                recordable.AddProxy(target.GetComponent<NetworkIdentity>().netId, proxy.GetComponent<NetworkIdentity>().netId);
            }
        }

        RpcDisableProxies();
    }

    [Server]
    private void OnPlayStart()
    {
        proxies.ToList().ForEach(p => NetworkServer.FindLocalObject(p).SetActive(true));
        RpcEnableProxies();
    }

    [Server]
    private void OnPlayStop()
    {
        proxies.ToList().ForEach(p => NetworkServer.FindLocalObject(p).SetActive(false));
        RpcDisableProxies();
    }

    [ClientRpc]
    private void RpcDisableProxies()
    {
        foreach (var netId in proxies)
        {
            ClientScene.FindLocalObject(netId).gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void RpcEnableProxies()
    {
        foreach (var netId in proxies)
        {
            ClientScene.FindLocalObject(netId).gameObject.SetActive(true);
        }
    }

}
