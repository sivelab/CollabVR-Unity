using System;
using UnityEngine;

[Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public double timeStamp;

    public TransformData(Transform transform, double timeStamp)
    {
        FromTransform(transform);
        this.timeStamp = timeStamp;
    }

    /// <summary>
    /// Sets this transform data from a give transform.
    /// </summary>
    /// <param name="transform">Input transform.</param>
    public void FromTransform(Transform transform)
    {
        position = transform.localPosition;
        rotation = transform.localRotation;
        scale = transform.localScale;
    }

    /// <summary>
    /// Sets the given transform with this tranform data.
    /// </summary>
    /// <param name="transform">The transform to set.</param>
    public void ToTransform(Transform transform)
    {
        transform.localPosition = position;
        transform.localRotation = rotation;
        transform.localScale = scale;
    }
}
