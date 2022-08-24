using System;
using System.IO;
using System.Linq;
using Server.Emulator;

namespace Server.NAudioWrapper;

public static class Mp3ToWav
{
    public static void Convert()
    {
        // TODO: Find a way to actually fucking do this.
        if (!Server.NAudio.HasLoaded) return;
        var Mp3FileReader = Server.NAudio.NAudioAssembly.GetType("NAudio.Wave.Mp3FileReader");
        var WaveFileWriter = Server.NAudio.NAudioAssembly.GetType("NAudio.Wave.WaveFileWriter");

        var Params = new object[] { Emulator.Tools.Path.Combine(BepInEx.Paths.GameRootPath, "osu!mania_beatmaps", "Audio!", "a.mp3") };
        using (var mp3 = new MemoryStream())
        {
            var bytes = File.ReadAllBytes(Params[0] as string);
            mp3.Write(bytes, 0, bytes.Length);
            
            var reader = Mp3FileReader.GetConstructors()[2].Invoke(new object[] { mp3 });
        }
        
        // .Select(constr => "(" +  string.Join(", ", constr.GetParameters().Select(par => par.Name).ToArray())  + ")");
    }
}