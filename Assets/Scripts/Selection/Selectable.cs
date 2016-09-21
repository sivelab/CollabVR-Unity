using System;
using VRStandardAssets.Utils;

public class Selectable : VRInteractiveItem
{
    public event Action OnSelect;

    protected virtual void Awake() { }

    public virtual void Select()
    {
        if (IsOver && OnSelect != null)
        {
            OnSelect();
        }
    }
}
