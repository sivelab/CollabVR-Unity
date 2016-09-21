using UnityEngine;

public class VRUI : Selectable
{
    protected override void Awake()
    {
        base.Awake();
        // add a collider if we don't have one already
        if (GetComponent<Collider>() == null)
        {
            var rect = GetComponent<RectTransform>().rect;
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(rect.width, rect.height, 0.01f);
        }
    }
}