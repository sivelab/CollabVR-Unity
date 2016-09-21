using UnityEngine;

public class SteamVR_Grab : MonoBehaviour
{
    [SerializeField]
    private Rigidbody attachPoint;
    [SerializeField]
    private SteamVR_TrackedObject trackedObj;
    [SerializeField]
    private SteamVR_TrackedController trackedController;
    [SerializeField]
    private GameObject collisionObject;

    private FixedJoint joint;

    private void OnEnable()
    {
        // add the gripped handlers
        trackedController.Gripped += DoGrip;
        trackedController.Ungripped += DoUngrip;
    }

    private void OnDisable()
    {
        trackedController.Gripped -= DoGrip;
        trackedController.Ungripped -= DoUngrip;
    }

    private void Start()
    {
        // add a collider
        if (GetComponent<BoxCollider>() == null)
        {
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(0.2f, 0.2f, 0.2f);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // only trigger if it has a rigidbody,
        // we don't want to be grabbing and throwing things without it
        if (collider.gameObject.GetComponent<Rigidbody>() != null)
        {
            collisionObject = collider.gameObject;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject == collisionObject)
        {
            collisionObject = null;
        }
    }

    private void DoGrip(object sender, ClickedEventArgs e)
    {
        if (collisionObject != null)
        {
            collisionObject.transform.position = attachPoint.transform.position;

            joint = collisionObject.AddComponent<FixedJoint>();
            joint.connectedBody = attachPoint;
        }
    }

    private void DoUngrip(object sender, ClickedEventArgs e)
    {
        if (collisionObject != null)
        {
            var rigidbody = collisionObject.GetComponent<Rigidbody>();
            DestroyImmediate(joint);
            joint = null;

            // see SteamVR/Extras/SteamVR_TestThrow.cs
            var device = SteamVR_Controller.Input((int)trackedObj.index);
            var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
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
        }
    }
}
