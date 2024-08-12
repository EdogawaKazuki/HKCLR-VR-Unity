using UnityEngine;

public class SmallChangeFilter
{
    private float threshold = 5;   // Threshold for small changes
    private float lastValue = 0;   // Last value

    public float Update(float measurement)
    {
        if (Mathf.Abs(measurement - lastValue) < threshold)
        {
            return lastValue;
        }
        else
        {
            lastValue = measurement;
            return measurement;
        }
    }
}