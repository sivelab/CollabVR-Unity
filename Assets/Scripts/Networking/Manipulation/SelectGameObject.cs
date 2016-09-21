using UnityEngine;
using UnityEngine.Networking;

namespace MMADLab.Networking.Manipulation
{
    public class SelectGameObject : NetworkBehaviour
    {
        [SerializeField]
        private NetworkIdentity networkIdentity;
        [SerializeField]
        private SelectionRaycaster raycaster;
        [SerializeField]
        private Rigidbody attachPoint;

        private FixedJoint joint;

        private void Start()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            raycaster.OnSelection += HandleSelection;
            raycaster.OnDeselection += HandleDeselection; 
        }

        private void HandleSelection(Selectable selectable)
        {
            // check if we're not selecting UI
            if (selectable.gameObject.GetComponentInChildren<RectTransform>() != null)
            {
                return;
            }

            CmdSelect(selectable.gameObject.GetComponent<NetworkIdentity>().netId);
        }

        private void HandleDeselection(Selectable selectable)
        {
            // check if deselecting not UI here
            if (selectable.gameObject.GetComponentInChildren<RectTransform>() != null)
            {
                return;
            }

            CmdDeselect(selectable.gameObject.GetComponent<NetworkIdentity>().netId);
        }

        [ClientRpc]
        private void RpcCreateJoint(NetworkInstanceId netId)
        {
            var localObject = ClientScene.FindLocalObject(netId);

            // create fixed joint between attach point and selectable
            joint = localObject.AddComponent<FixedJoint>();
            joint.connectedBody = attachPoint;
        }

        [ClientRpc]
        private void RpcDestroyJoint(NetworkInstanceId netId)
        {
            // remove fixed joint before removing authority
            DestroyImmediate(joint);
            joint = null;
        }

        [Command]
        private void CmdSelect(NetworkInstanceId netId)
        {
            var netObject = NetworkServer.FindLocalObject(netId).GetComponent<NetworkIdentity>();
            // assign authority only if this object has no client authority
            if (netObject.clientAuthorityOwner == null)
            {
                netObject.AssignClientAuthority(networkIdentity.connectionToClient);
                RpcCreateJoint(netId);
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
                RpcDestroyJoint(netId);
                netObject.RemoveClientAuthority(networkIdentity.connectionToClient);
            }
            else
            {
                Debug.Log(name + " cannot deselect " + netObject.gameObject.name + " because it is not selected by this object.");
            }
        }
    }
}
