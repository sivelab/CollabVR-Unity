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

    /// <summary>
    /// Get the next data point in the recording.
    /// </summary>
    /// <param name="timestamp">The time in the playback that we are at.</param>
    /// <param name="forwards">Whether we are seeks forwards in time or not.</param>
    /// <returns></returns>
    public TransformData Next(double timestamp, bool forwards = true)
    {
        if (forwards
            && currentPosition + 1 < data.Count - 1
            && data[currentPosition + 1].timeStamp < timestamp)
        {
            currentPosition++;
        }
        else if (!forwards
            && currentPosition - 1 >= 0
            && data[currentPosition - 1].timeStamp >= timestamp)
        {
            currentPosition--;
        }
        return data[currentPosition];
    }

    public void Stop()
    {
        currentPosition = 0;
    }
}
