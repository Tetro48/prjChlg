using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class Extensions
{
    public static IList<T> Shuffle<T>(this IList<T> _list)
    {
        System.Random random = new System.Random();
        for (int i = 0; i < _list.Count; i++)
        {
            T temp = _list[i];
            int randomIndex = random.Next(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }

        return _list;
    }
    public static Vector2 Rotate(this Vector2 v, float radians) {
        return new Vector2(
            v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
            v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
        );
    }
    public static float2 Rotate(this float2 v, float radians) {
        return new float2(
            v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
            v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
        );
    }
}
