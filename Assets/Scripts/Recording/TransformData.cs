using System;
using UnityEngine;

[Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public double timeStamp;

    public TransformData(Transform transform, double timeStamp)
    {
        FromTransform(transform);
        this.timeStamp = timeStamp;
    }

    public void FromTransform(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
    }

    public void ToTransform(Transform transform)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
