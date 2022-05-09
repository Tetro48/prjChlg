using System;
using System.Collections.Generic;

public static class Extensions
{
    public static List<T> Shuffle<T>(this List<T> _list)
    {
        Random random = new Random();
        for (int i = 0; i < _list.Count; i++)
        {
            T temp = _list[i];
            int randomIndex = random.Next(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }

        return _list;
    }
    public static T[] Shuffle<T>(this T[] _array)
    {
        Random random = new Random();
        for (int i = 0; i < _array.Length; i++)
        {
            T temp = _array[i];
            int randomIndex = random.Next(i, _array.Length);
            _array[i] = _array[randomIndex];
            _array[randomIndex] = temp;
        }

        return _array;
    }
    public static IList<T> Shuffle<T>(this IList<T> _list)
    {
        Random random = new Random();
        for (int i = 0; i < _list.Count; i++)
        {
            T temp = _list[i];
            int randomIndex = random.Next(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }

        return _list;
    }
}
