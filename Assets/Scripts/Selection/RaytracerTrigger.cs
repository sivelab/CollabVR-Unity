using UnityEngine;
using System.Collections;
using VRStandardAssets.Utils;
using System.Collections.Generic;

/**
 * Place this on the gameobject to want to select with, as it will use its transform.
 */
public class RaytracerTrigger : SelectorTrigger
{
    [SerializeField]
    [Tooltip("Layers to exclude from the raycast.")]
    private LayerMask m_ExclusionLayers;

    [SerializeField]
    [Tooltip("The reticle, if applicable.")]
    private Reticle m_Reticle;

    [SerializeField]
    [Tooltip("Optionally show the debug ray.")]
    private bool m_ShowDebugRay;

    [SerializeField]
    private float m_DebugRayLength = 5f;

    [SerializeField]
    [Tooltip("How long the Debug ray will remain visible.")]
    private float m_DebugRayDuration = 1f;

    [SerializeField]
    [Tooltip("How far into the scene the ray is cast.")]
    private float m_RayLength = 500f;

    [SerializeField]
    [Tooltip("The transform we are casting the ray from.")]
    private Transform originTransform;

    private Selectable currentSelectable;
    public Selectable CurrentSelectabe
    {
        get { return currentSelectable; }
        //set { currentSelectable = value; }
    }

    //The current interactive item
    private Selectable previousSelectable;

    private void Update()
    {
        Raytrace();
    }

    private void Raytrace()
    {
        // Show the debug ray if required
        if (m_ShowDebugRay)
        {
            Debug.DrawRay(originTransform.position, originTransform.forward * m_DebugRayLength, Color.red, m_DebugRayDuration);
        }

        // Create a ray that points forwards from the camera.
        var ray = new Ray(originTransform.position, originTransform.forward);
        var hit = new RaycastHit();

        // Do the raycast forweards to see if we hit an interactive item
        if (Physics.Raycast(ray, out hit, m_RayLength, ~m_ExclusionLayers))
        {
            var interactible = hit.collider.GetComponent<Selectable>(); //attempt to get the VRInteractiveItem on the hit object
            currentSelectable = interactible;

            // If we hit an interactive item and it's not the same as the last interactive item, then call Over
            if (interactible && interactible != previousSelectable)
            {
                Over(interactible);
            }

            // Deactivate the last interactive item
            // takes care of null interactable as well
            if (interactible != previousSelectable)
            {
                DeactiveLastInteractible();
            }

            previousSelectable = currentSelectable;

            // Something was hit, set at the hit position.
            m_Reticle.SetPosition(hit);
        }
        else
        {
            // Nothing was hit, deactive the last interactive item.
            DeactiveLastInteractible();
            currentSelectable = null;

            // Position the reticle at default distance.
            m_Reticle.SetPosition();
        }
    }

    private void DeactiveLastInteractible()
    {
        if (previousSelectable == null)
        {
            return;
        }

        Out(previousSelectable);
        previousSelectable = null;
    }
}
