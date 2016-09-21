using UnityEngine;
using System.Collections;
using VRStandardAssets.Utils;

public class RadialSelector : RigidbodySelector
{
    [SerializeField]
    private SelectionRadial radial;

    public override void HandleTrigger(Selectable selectable)
    {
        radial.Hide();
        radial.Show();
        base.HandleTrigger(selectable);
    }

    public override void HandleUntrigger(Selectable selectable)
    {
        radial.Hide();
        radial.Show();
        base.HandleUntrigger(selectable);
    }

    public override void HandleOver(Selectable selectable)
    {
        radial.Show();
        base.HandleOver(selectable);
    }

    public override void HandleOut(Selectable selectable)
    {
        radial.Hide();
        base.HandleOut(selectable);
    }
}
