using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RigidbodySelector : NetworkedSelector
{
    [SerializeField]
    private Rigidbody attachPoint;

    private FixedJoint joint;

    public override void Select(Selectable selectable)
    {
        base.Select(selectable);
        if (selectable.gameObject.GetComponent<FixedJoint>() == null)
        {
            Debug.Log("adding fixed joint");
            joint = selectable.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = attachPoint;
        }
    }

    public override void Deselect(Selectable selectable)
    {
        DestroyImmediate(joint);
        joint = null;
        base.Deselect(selectable);
    }
}
