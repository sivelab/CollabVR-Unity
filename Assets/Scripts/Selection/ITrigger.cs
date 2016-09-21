using System;

public interface ITrigger
{
    event Action<Selectable> OnTrigger;
    event Action<Selectable> OnUntrigger;
    event Action<Selectable> OnOver;
    event Action<Selectable> OnOut;

    void Trigger(Selectable selectable);
    void Untrigger(Selectable selectable);
    void Over(Selectable selectable);
    void Out(Selectable selectable);
}

