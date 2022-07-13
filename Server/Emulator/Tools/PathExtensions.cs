using System.Linq;

namespace Server.Emulator.Tools;

public static class Path
{
    public static string Combine(params string[] paths)
    {
        // the fact that this is not how Path.Combine works in .net framework 3.5 is disappointing
        return paths.ToArray().Aggregate((pathA, pathB) => System.IO.Path.Combine(pathA, pathB));
    }
}