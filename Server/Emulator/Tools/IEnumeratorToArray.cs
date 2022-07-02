using System.Collections.Generic;

namespace Server.Emulator.Tools;

public static class IEnumeratorToArray
{
    public static T[] ToArray<T>(this IEnumerator<T> enumerator)
    {
        var list = new List<T>();
        
        while(enumerator.MoveNext())
            list.Add(enumerator.Current);
        
        return list.ToArray();
    }
}