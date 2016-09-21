using UnityEngine;
using System.Collections;

public class ViveGrabTrigger : SelectorTrigger
{
    [SerializeField]
    private SteamVR_TrackedController trackedController;

    private Selectable collisionObject;

    // Use this for initialization
    private void Start()
    {
        // add a collider if we don't have one
        if (gameObject.GetComponent<BoxCollider>() == null)
        {
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(0.2f, 0.2f, 0.2f);
        }

        // add the trigger button handlers
        trackedController.TriggerClicked += HandleTriggerClicked;
        trackedController.TriggerUnclicked += HandleTriggerUnclicked;
    }

    // Physics trigger with collider on this GameObject
    private void OnTriggerEnter(Collider collider)
    {
        // only trigger if it has a rigidbody,
        // we don't want to be grabbing and throwing things without it
        var selectable = collider.gameObject.GetComponent<Selectable>();
        if (collider.gameObject.GetComponent<Rigidbody>() != null && selectable != null)
        {
            collisionObject = selectable;
            Over(collisionObject);
        }
    }

    // Physics trigger
    private void OnTriggerExit(Collider collider)
    {
        var selectable = collider.gameObject.GetComponent<Selectable>();
        if (collider.gameObject.GetComponent<Rigidbody>() != null && selectable != null)
        {
            Out(selectable);
            collisionObject = null;
        }
    }

    // Steam_VR controller trigger
    private void HandleTriggerClicked(object sender, ClickedEventArgs e)
    {
        if (collisionObject != null)
        {
            Trigger(collisionObject);
        }
    }

    // Steam_VR controller trigger
    private void HandleTriggerUnclicked(object sender, ClickedEventArgs e)
    {
        if (collisionObject != null)
        {
            Untrigger(collisionObject);
        }
    }
}
