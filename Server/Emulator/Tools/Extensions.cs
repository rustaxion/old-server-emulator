using System.Collections.Generic;

namespace Server.Emulator.Tools;


public static partial class Extensions
{
    public static void AddAll<K, V>(this IDictionary<K, V> target,
                                   IDictionary<K, V> source,
                                   bool overwrite = false)
    {
        foreach (var _ in source)
            if (overwrite || !target.ContainsKey(_.Key))
                target[_.Key] = _.Value;
    }
}
