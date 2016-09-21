using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class VRButton : VRUI
{
    private Button button;

    protected override void Awake()
    {
        base.Awake();
        button = GetComponent<Button>();
    }

    public override void Select()
    {
        base.Select();
        button.onClick.Invoke();
    }
}