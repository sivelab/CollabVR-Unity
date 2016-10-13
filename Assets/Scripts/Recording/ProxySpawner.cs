using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ProxySpawner : NetworkBehaviour
{
    private Recordable[] recordables;
    private RecordingManager manager;

    private IList<GameObject> proxies;
    private Material proxyMaterial;

    public override void OnStartServer()
    {
        manager = FindObjectOfType<RecordingManager>();
        proxies = new List<GameObject>();

        proxyMaterial = Resources.Load(manager.ProxyMaterialName(), typeof(Material)) as Material;

        manager.EventPlayStart += OnPlayStart;
        manager.EventPlayStop += OnPlayStop;
    }

    public override void OnStartClient()
    {
        CmdRespawn();
    }

    [Command]
    public void CmdRespawn()
    {
        foreach (var proxy in proxies)
        {
            ClientScene.UnregisterPrefab(proxy);
            NetworkServer.Destroy(proxy);
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
                proxies.Add(proxy);
                recordable.AddProxy(target.GetComponent<NetworkIdentity>().netId, proxy.GetComponent<NetworkIdentity>().netId);
            }
        }

        RpcDisableProxies(ProxyNetIds());
    }

    [Server]
    private void OnPlayStart()
    {
        proxies.ToList().ForEach(p => p.SetActive(true));
        RpcEnableProxies(ProxyNetIds());
    }

    [Server]
    private void OnPlayStop()
    {
        proxies.ToList().ForEach(p => p.SetActive(false));
        RpcDisableProxies(ProxyNetIds());
    }

    [Server]
    private NetworkInstanceId[] ProxyNetIds()
    {
        return (from proxy in proxies select proxy.GetComponent<NetworkIdentity>().netId).ToArray();
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
            ClientScene.FindLocalObject(netId).gameObject.SetActive(true);
        }
    }

}
