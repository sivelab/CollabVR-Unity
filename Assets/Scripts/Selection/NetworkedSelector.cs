using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class NetworkedSelector : NetworkBehaviour, ISelector
{
    [SerializeField]
    private SelectorTrigger trigger;
    [SerializeField]
    private NetworkIdentity networkIdentity;

    private Selectable selectedObject;

    // Use this for initialization
    public virtual void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        trigger.OnTrigger += HandleTrigger;
        trigger.OnUntrigger += HandleUntrigger;
        trigger.OnOver += HandleOver;
        trigger.OnOut += HandleOut;
    }

    public virtual void HandleTrigger(Selectable selectable)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (selectable != null)
        {
            // check if reselecting the same object
            if (selectedObject == selectable)
            {
                HandleUntrigger(selectable);
            }
            else
            {
                CmdSelect(selectable.gameObject.GetComponent<NetworkIdentity>().netId);
                if (selectable.GetComponent<NetworkIdentity>().clientAuthorityOwner == networkIdentity.connectionToClient)
                {

                    Debug.Log(gameObject.name + " selects " + selectable.name);
                    selectable.Select();
                    Select(selectable);
                }
            }
        }
    }

    public virtual void HandleUntrigger(Selectable selectable)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        Debug.Log(gameObject.name + " deselects " + selectable.name);
        Deselect(selectable);
        CmdDeselect(selectable.gameObject.GetComponent<NetworkIdentity>().netId);
    }

    public virtual void HandleOver(Selectable selectable)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        Debug.Log(trigger.gameObject.name + " is over " + selectable.gameObject.name);
        selectable.Over();
    }

    public virtual void HandleOut(Selectable selectable)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        Debug.Log(trigger.gameObject.name + " is no longer over " + selectable.gameObject.name);
        selectable.Out();
    }

    public virtual void Select(Selectable selectable)
    {
        selectedObject = selectable;
    }

    public virtual void Deselect(Selectable selectable)
    {
        selectedObject = null;
    }

    [Command]
    private void CmdSelect(NetworkInstanceId netId)
    {
        var netObject = NetworkServer.FindLocalObject(netId).GetComponent<NetworkIdentity>();
        // assign authority only if this object has no client authority
        if (netObject.clientAuthorityOwner == null)
        {
            netObject.AssignClientAuthority(networkIdentity.connectionToClient);
        }
        else
        {
            Debug.Log(name + " cannot select " + netObject.gameObject.name + " because it is already selected by " + netObject.clientAuthorityOwner);
        }
    }

    [Command]
    private void CmdDeselect(NetworkInstanceId netId)
    {
        var netObject = NetworkServer.FindLocalObject(netId).GetComponent<NetworkIdentity>();
        // remove authority only if this object has a client authority
        if (netObject.clientAuthorityOwner == networkIdentity.connectionToClient)
        {
            netObject.RemoveClientAuthority(networkIdentity.connectionToClient);
        }
        else
        {
            Debug.Log(name + " cannot deselect " + netObject.gameObject.name + " because it is not selected by this object.");
        }
    }
}
