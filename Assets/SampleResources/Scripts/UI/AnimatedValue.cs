/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;

public abstract class AnimatedValue<T>
{
    T startValue;
    T targetValue;

    double startTime;
    double targetTime;

    bool animating = false;
    
    public AnimatedValue(T value)
    {
        startValue = targetValue = value;
        startTime = targetTime = Time.time;
    }

    public T GetValue()
    {
        if (!animating)
        {
            return targetValue;
        }

        double now = Time.time;
        if (now >= targetTime)
        {
            animating = false;
            return targetValue;
        }

        var elapsed = (now - startTime) / (targetTime - startTime);
        return Interpolate(startValue, targetValue, (float)elapsed);
    }

    public void SetValue(T v)
    {
        targetValue = v;
        targetTime = Time.time;
    }

    public void AnimateToValue(T value, double duration)
    {
        startValue = GetValue();
        if (value.Equals(startValue))
        {
            SetValue(value);
            animating = false;
        }
        else
        {
            targetValue = value;

            startTime = Time.time;
            targetTime = startTime + duration;

            animating = true;
        }
    }

    public bool IsAnimating()
    {
        return animating;
    }

    protected abstract T Interpolate(T a, T b, float delta);
}

public class AnimatedFloat : AnimatedValue<float>
{
    public AnimatedFloat(float value) : base(value) { }

    protected override float Interpolate(float a, float b, float delta)
    {
        return Mathf.Lerp(a, b, delta);
    }
}
