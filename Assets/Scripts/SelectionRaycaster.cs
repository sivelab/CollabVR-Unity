using UnityEngine;
using System;
using VRStandardAssets.Utils;

namespace MMADLab
{
    public class SelectionRaycaster : MonoBehaviour
    {
        public event Action<RaycastHit> OnRaycasthit;                   // This event is called every frame that the user's gaze is over a collider.
        public event Action OnOverInteractiveItem;
        public event Action OnOutInteractiveItem;
        public event Action<Selectable> OnSelection;
        public event Action<Selectable> OnDeselection;

        [SerializeField]
        private LayerMask m_ExclusionLayers;           // Layers to exclude from the raycast.
        [SerializeField]
        private Reticle m_Reticle;                     // The reticle, if applicable.
        [SerializeField]
        private VRInput m_VrInput;                     // Used to call input based events on the current VRInteractiveItem.
        [SerializeField]
        private bool m_ShowDebugRay;                   // Optionally show the debug ray.
        [SerializeField]
        private float m_DebugRayLength = 5f;           // Debug ray length.
        [SerializeField]
        private float m_DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
        [SerializeField]
        private float m_RayLength = 500f;              // How far into the scene the ray is cast.
		[SerializeField]
        private Transform cameraTransform;
		[SerializeField]
        private SelectionRadial radial;

        private Selectable currentSelectable;                //The current interactive item
        private Selectable previousSelectable;                   //The last interactive item
        private float selectedDistance;
        private Selectable currentSelected;
        private RaycastHit hit;

        // Utility for other classes to get the current interactive item
        public Selectable CurrentSelectable
        {
            get { return currentSelected; }
        }

        public float SelectedDistance
        {
            get { return selectedDistance; }
        }

        private void OnEnable()
        {
            radial.OnSelectionComplete += HandleSelectionComplete;
        }

        private void OnDisable()
        {
            radial.OnSelectionComplete -= HandleSelectionComplete;
        }

        private void Update()
        {
            EyeRaycast();
        }

        private void EyeRaycast()
        {
            // Show the debug ray if required
            if (m_ShowDebugRay)
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * m_DebugRayLength, Color.blue, m_DebugRayDuration);
            }

            // Create a ray that points forwards from the camera.
            var ray = new Ray(cameraTransform.position, cameraTransform.forward);

            // Do the raycast forweards to see if we hit an interactive item
            if (Physics.Raycast(ray, out hit, m_RayLength, ~m_ExclusionLayers))
            {
                var interactible = hit.collider.GetComponent<Selectable>(); //attempt to get the VRInteractiveItem on the hit object
                currentSelectable = interactible;

                // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                if (interactible && interactible != previousSelectable)
                {
                    interactible.Over();
                    OverInteractiveItem();
                }

                // Deactive the last interactive item 
				if (interactible != previousSelectable)
				{
					DeactiveLastInteractible();
				}

                previousSelectable = interactible;

                // Something was hit, set at the hit position.
				if (m_Reticle)
				{
					m_Reticle.SetPosition(hit);
				}

                if (OnRaycasthit != null)
                {
                    OnRaycasthit(hit);
                }
            }
            else
            {
                // Nothing was hit, deactive the last interactive item.
                DeactiveLastInteractible();
                currentSelectable = null;

                // Position the reticle at default distance.
				if (m_Reticle)
				{
					m_Reticle.SetPosition();
				}
            }
        }


        private void HandleSelectionComplete()
        {
            // check for current item
            if (currentSelected != null)
            {
                // we are already selecting, so release
                Deselect();
            }
            else
            {
                currentSelected = currentSelectable;
                selectedDistance = hit.distance;
                selectedDistance += Vector3.Distance(currentSelected.gameObject.GetComponent<Collider>().bounds.center, hit.point);
                currentSelected.Select();
                Select();
            }
        }

        private void Select()
        {
            Debug.Log(transform.root.gameObject.name + " selects " + currentSelected.name);
            radial.Hide();
            radial.Show();
            if (OnSelection != null)
            {
                OnSelection(currentSelected);
            }
        }

        private void Deselect()
        {
            Debug.Log(transform.root.gameObject.name + " deselects " + currentSelected.name);
            var selected = currentSelected;
            currentSelected = null;
            selectedDistance = 0;
            radial.Hide();
            radial.Show();
            if (OnDeselection != null)
            {
                OnDeselection(selected);
            }
        }

        private void OverInteractiveItem()
        {
            if (currentSelected == null)
            {
                radial.Show();
            }

            if (OnOverInteractiveItem != null)
            {
                OnOverInteractiveItem();
            }
        }

        private void OutInteractiveItem()
        {
            if (currentSelected == null)
            {
                radial.Hide();
            }
            else
            {
                Deselect();
            }

            if (OnOutInteractiveItem != null)
            {
                OnOutInteractiveItem();
            }
        }

        private void DeactiveLastInteractible()
        {
			if (previousSelectable == null)
			{
				return;
			}

            previousSelectable.Out();
            OutInteractiveItem();
            previousSelectable = null;
        }
    }
}
