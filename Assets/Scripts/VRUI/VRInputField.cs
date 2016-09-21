using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class VRInputField : VRUI
{
    private InputField inputField;

    protected override void Awake()
    {
        base.Awake();
        inputField = GetComponent<InputField>();
    }

    public override void Select()
    {
        base.Select();
        inputField.Select();
        inputField.ActivateInputField();
    }
}
