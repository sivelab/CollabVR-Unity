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

        OnOver += HandleOver;
        OnOut += HandleOver;
    }

    public override void Select()
    {
        base.Select();
        button.onClick.Invoke();
    }

    private void HandleOver()
    {
        var temp = button.colors.normalColor;

        var cb = button.colors;
        cb.normalColor = button.colors.highlightedColor;
        cb.highlightedColor = temp;
    }
}