using UnityEngine;

public class ViveGrabSelector : RigidbodySelector
{
    [SerializeField]
    private SteamVR_TrackedObject trackedObject;
    [SerializeField]
    private Color highlightColor;

    private Color originalColor;

    public override void HandleOver(Selectable selectable)
    {
        base.HandleOver(selectable);
        var material = selectable.GetComponent<MeshRenderer>().material;
        originalColor = material.color;
        material.color = highlightColor;
    }

    public override void HandleOut(Selectable selectable)
    {
        base.HandleOut(selectable);
        selectable.GetComponent<MeshRenderer>().material.color = originalColor;
    }

    public override void Deselect(Selectable selectable)
    {
        var rigidbody = selectable.GetComponent<Rigidbody>();

        // see SteamVR/Extras/SteamVR_TestThrow.cs
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        var origin = trackedObject.origin ? trackedObject.origin : trackedObject.transform.parent;
        if (origin != null)
        {
            rigidbody.velocity = origin.TransformVector(device.velocity);
            rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
        }
        else
        {
            rigidbody.velocity = device.velocity;
            rigidbody.angularVelocity = device.angularVelocity;
        }

        rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;

        base.Deselect(selectable);
    }
}
