using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Server.AutoUpdater;

public static class Update
{
    public static string GetUpdateScript(string url, string path)
    {
        var commands = new string[] {
            "Wait-Process -Name 'INVAXION' -ErrorAction SilentlyContinue",
            $"Invoke-WebRequest -Uri '{url}' -OutFile '{path}'",
            "Remove-Item -Path $MyInvocation.MyCommand.Source"
        };
        return string.Join("\n", commands);
    }

    public static void Procceed(GithubReleases.Release release)
    {
        var assetLink = release.assets.Where(asset => asset.name == "ServerEmulator.dll").FirstOrDefault().browser_download_url;
        var outFile = Path.Combine(Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "BepInEx"), "plugins"), "ServerEmulator.dll");
        var script = GetUpdateScript(assetLink, outFile);

        var scriptName = $"ServerEmulator_{new Tag(release.tag_name)}_update.ps1";
        var scriptPath = Path.Combine(Path.GetTempPath(), scriptName);
        File.WriteAllText(scriptPath, script);

        var updateProcess = new Process()
        {
            StartInfo =
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy ByPass -File {scriptName}",
                    WorkingDirectory = Path.GetTempPath(),
                }
        };
        updateProcess.Start();
        Application.Quit();
    }
}
