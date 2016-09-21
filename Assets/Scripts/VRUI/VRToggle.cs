using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class VRToggle : VRUI
{
    private Toggle toggle;

    protected override void Awake()
    {
        base.Awake();
        toggle = GetComponent<Toggle>();
    }

    public override void Select()
    {
        base.Select();
        toggle.isOn = !toggle.isOn;
    }
}