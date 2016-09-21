using UnityEngine;
using UnityEngine.Networking;
using System;

public class SelectorTrigger : MonoBehaviour, ITrigger
{
    public event Action<Selectable> OnTrigger;
    public event Action<Selectable> OnUntrigger;
    public event Action<Selectable> OnOver;
    public event Action<Selectable> OnOut;

    public virtual void Trigger(Selectable selectable)
    {
        if (OnTrigger != null)
        {
            OnTrigger(selectable);
        }
    }

    public virtual void Untrigger(Selectable selectable)
    {
        if (OnUntrigger != null)
        {
            OnUntrigger(selectable);
        }
    }

    public virtual void Over(Selectable selectable)
    {
        if (OnOver != null)
        {
            OnOver(selectable);
        }
    }

    public virtual void Out(Selectable selectable)
    {
        if (OnOut != null)
        {
            OnOut(selectable);
        }
    }
}
