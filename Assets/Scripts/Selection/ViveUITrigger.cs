using UnityEngine;
using System.Collections;

public class ViveUITrigger : ViveGrabTrigger
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponent<Selectable>() is VRUI)
        {
            collisionObject = collider.gameObject.GetComponent<Selectable>();
            Over(collisionObject);
        }
    }

    protected override void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.GetComponent<Selectable>() is VRUI)
        {
            Out(collider.gameObject.GetComponent<Selectable>());
            collisionObject = null;
        }
    }
}
