using System;
using UnityEngine;

public class CircularBuffer <T>
{ 
    T[] dataArray;
    int bufferCurrentPosition = -1;
    int bufferCapacity;
    float howManyRecordsPerSecond;
    int lastAccessedIndex = -1;


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
    /// Try read last value that was written to buffer
    /// </summary>
    /// <returns></returns>
    public bool TryReadLastValue(out T value)
    {
        if (bufferCurrentPosition != -1)
        {
            value = dataArray[bufferCurrentPosition];
            return true;
        }
        else
        {
            value = default;
            return false;
        }
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
    /// <summary>
    /// Read specified value from circular buffer
    /// </summary>
    /// <param name="seconds">Variable defining how many seconds into the past should be read (eg. seconds=5 then function will return the values that tracked object had exactly 5 seconds ago)</param>
    /// <param name="wasLastAccessedIndexSame">To save performance, for certain rewinds we can check if the last accessed index was the same current and choose to ignore the update</param>
    /// <returns></returns>
    public T ReadFromBuffer(float seconds, out bool wasLastAccessedIndexSame)
    {
        int index = CalculateIndex(seconds);

        wasLastAccessedIndexSame = index == lastAccessedIndex;
        lastAccessedIndex = index;
        return dataArray[index];
    }
    private void MoveLastBufferPosition(float seconds)
    {
        bufferCurrentPosition= CalculateIndex(seconds);    
    }
    private int CalculateIndex(float seconds)
    {
        int howManyBeforeLast = (int)(howManyRecordsPerSecond * (seconds - 0.001));
        int moveBy = bufferCurrentPosition - howManyBeforeLast;
       
        if (moveBy < 0)
        {
            return bufferCapacity + moveBy;
        }
        else
        {
            return bufferCurrentPosition - howManyBeforeLast;
        }
    }
}
