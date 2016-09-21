using UnityEngine;
using VRStandardAssets.Utils;
using System.Collections;

public class RadialTrigger : RaytracerTrigger
{
    [SerializeField]
    private SelectionRadial radial;

	private void Start () {
        radial.OnSelectionComplete += HandleRadialSelectionComplete;
	}
	
    private void HandleRadialSelectionComplete()
    {
        if (CurrentSelectabe != null)
        {
            Trigger(CurrentSelectabe);
        }
    }
}
