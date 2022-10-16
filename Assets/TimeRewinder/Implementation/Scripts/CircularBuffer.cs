using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularBuffer <T>
{
    T[] dataArray;
    int bufferCurrentPosition = -1;
    int bufferCapacity;
    /// <summary>
    /// Use circular buffer structure for time rewinding
    /// </summary>
    /// <param name="bufferSize">This parameter is defining how big should circular buffer be (For time rewinding purposes, the variable howManyItemsFit from RewindAbstract class is correct option)</param>
    public CircularBuffer(int bufferSize)
    {
        bufferCapacity=bufferSize;
        dataArray = new T[bufferSize];
    }
    public void WriteLastValue(T val)
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
    /// <summary>
    /// Read last value that was written to buffer
    /// </summary>
    /// <returns></returns>
    public T ReadLastValue()
    {
        return dataArray[bufferCurrentPosition];
    }
    /// <summary>
    /// Read specified value from circullar buffer
    /// </summary>
    /// <param name="howManyBeforeLast">Variable defining what is the position of the value to be read in relation to the last written value to the buffer (eg. howManyBeforeLast=5 and lastPositionInBuffer=300 then function will return the value on position 295 value)</param>
    /// <returns></returns>
    public T ReadFromBuffer(float howManyBeforeLast)
    {
        if((bufferCurrentPosition- (int)howManyBeforeLast) <0)
        {
            int showingIndex = bufferCapacity - ((int)howManyBeforeLast - bufferCurrentPosition);
            return dataArray[showingIndex];
        }
        else
        {
            return dataArray[bufferCurrentPosition - (int)howManyBeforeLast];
        }
    }
    /// <summary>
    /// Delete specified value from circullar buffer
    /// </summary>
    /// <param name="howManyBeforeLast">Variable defining what is the position that the circular buffer should move to to prepare for next write (eg. howManyBeforeLast=5 and lastPositionInBuffer=300 then function will move buffer position to 295. Which means that in next write, value that is now on position 295 will be overwritten)</param>
    public void MoveLastBufferPosition(float howManyBeforeLast)
    {
        if ((bufferCurrentPosition - (int)howManyBeforeLast) < 0)
        {
            bufferCurrentPosition = bufferCapacity - ((int)howManyBeforeLast - bufferCurrentPosition);
        }
        else
        {
            bufferCurrentPosition -= (int)howManyBeforeLast;
        }     
    }
}
