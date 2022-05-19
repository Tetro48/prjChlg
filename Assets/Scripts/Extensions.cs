using System;
using System.Collections.Generic;

public static class Extensions
{
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
