using UnityEngine;

// If you already have the lecture's Ease.cs in your project, DON'T add this one.
public static class Ease
{
    public static float OutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        t = Mathf.Clamp01(t) - 1f;
        return 1f + c3 * t * t * t + c1 * t * t;
    }
}
