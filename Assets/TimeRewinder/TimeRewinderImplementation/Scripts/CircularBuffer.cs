using System;
using UnityEngine;

public class CircularBuffer <T>
{ 
    T[] dataArray;
    int bufferCurrentPosition = -1;
    int bufferCapacity;
    float howManyRecordsPerSecond;

    /// <summary>
    /// Use circular buffer structure for time rewinding
    /// </summary>
    public CircularBuffer()
    {
        try
        {
            howManyRecordsPerSecond = 1 / Time.fixedDeltaTime;
            bufferCapacity = (int)(RewindManager.Instance.HowManySecondsToTrack *howManyRecordsPerSecond);
            dataArray = new T[bufferCapacity];
            RewindManager.BuffersRestore += MoveLastBufferPosition;
        }
        catch
        {
            Debug.LogError("Circular buffer cannot use field initialization (Time.fixedDeltaTime is unknown yet). Initialize Circular buffer in Start() method!");
        }        
    }
    
    /// <summary>
    /// Write value to the last position of the buffer if Tracking is enabled
    /// </summary>
    /// <param name="val"></param>
    public void WriteLastValue(T val)
    {
        if (RewindManager.Instance.TrackingEnabled)
        {
            bufferCurrentPosition++;
            if (bufferCurrentPosition >= bufferCapacity)
            {
                bufferCurrentPosition = 0;
                dataArray[bufferCurrentPosition] = val;
            }
            else
            {
                dataArray[bufferCurrentPosition] = val;
            }
        }
    }
    /// <summary>
    /// Read last value that was written to buffer
    /// </summary>
    /// <returns></returns>
    public T ReadLastValue()
    {
        return dataArray[bufferCurrentPosition];
    }

    /// <summary>
    /// Read specified value from circular buffer
    /// </summary>
    /// <param name="seconds">Variable defining how many seconds into the past should be read (eg. seconds=5 then function will return the values that tracked object had exactly 5 seconds ago)</param>
    /// <returns></returns>
    public T ReadFromBuffer(float seconds)
    {
        return dataArray[CalculateIndex(seconds)];
    }
    private void MoveLastBufferPosition(float seconds)
    {
        bufferCurrentPosition= CalculateIndex(seconds);    
    }
    private int CalculateIndex(float seconds)
    {
        double secondsRound = Math.Round(seconds, 2);
        int howManyBeforeLast = (int)(howManyRecordsPerSecond * secondsRound);

        int moveBy = bufferCurrentPosition - howManyBeforeLast;
        if (moveBy < 0)
        {
            return bufferCapacity + moveBy;
        }
        else
        {
            return bufferCurrentPosition- howManyBeforeLast;
        }
    }
}
