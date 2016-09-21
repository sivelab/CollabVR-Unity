using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Recording
{
    public string gameObjectName;
    public List<TransformData> data;

    private int currentPosition;        // position in the recording for playback

    public Recording(string gameObjectName)
    {
        this.gameObjectName = gameObjectName;
        data = new List<TransformData>();
        currentPosition = 0;
    }

    public TransformData Next(double timestamp)
    {
        if (currentPosition + 1 < data.Count - 1 && data[currentPosition + 1].timeStamp < timestamp)
        {
            currentPosition++;
            return data[currentPosition];
        }
        else
        {
            return data[currentPosition];
        }
    }

    public void Stop()
    {
        currentPosition = 0;
    }
}
