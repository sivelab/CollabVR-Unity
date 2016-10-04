using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[Serializable]
public struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public float timeStamp;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="timeStamp"></param>
    public TransformData(Transform transform, float timeStamp)
    {
        position = transform.localPosition;
        rotation = transform.localRotation;
        scale = transform.localScale;
        this.timeStamp = timeStamp;
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
