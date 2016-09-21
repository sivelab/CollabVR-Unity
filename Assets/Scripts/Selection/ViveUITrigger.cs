using UnityEngine;
using System.Collections;

public class ViveUITrigger : RaytracerTrigger
{
    // some sort of pointing ray

    [SerializeField]
    private SteamVR_TrackedController controller;

    private void Start()
    {
        controller.TriggerClicked += HandleTriggerClicked;
        controller.TriggerUnclicked += HandleTriggerUnclicked;
    }

    private void HandleTriggerClicked(object sender, ClickedEventArgs e)
    {
        if (CurrentSelectabe is VRUI)
        {
            Trigger((VRUI)CurrentSelectabe);
        }
    }

    private void HandleTriggerUnclicked(object sender, ClickedEventArgs e)
    {
        if (CurrentSelectabe is VRUI)
        {
            Untrigger((VRUI)CurrentSelectabe);
        }
    }
}
