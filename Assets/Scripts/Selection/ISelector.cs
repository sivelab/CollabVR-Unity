using UnityEngine;
using System.Collections;

public interface ISelector
{
    void HandleOver(Selectable selectable);
    void HandleOut(Selectable selectable);
    void HandleTrigger(Selectable selectable);
    void HandleUntrigger(Selectable selectable);

    void Select(Selectable selectable);
    void Deselect(Selectable selectable);
}
