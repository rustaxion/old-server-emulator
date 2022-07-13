using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Server.AutoUpdater;

public static class Update
{
    public static string ReleasesAPI = "https://api.github.com/repos/Invaxion-Server-Emulator/invaxion-server-emulator/releases";

    public static void CheckForUpdate(Tag currentVersion)
    {
        var headers = new Dictionary<string, string> { { "Accept", "application/vnd.github.v3+json" } };
        var checkUpdateReq = new Networking.Request(ReleasesAPI, headers);
        Server.Instance.startCoroutine(checkUpdateReq.Download(((request, success) =>
        {
            if (success)
            {
                GithubReleases.Release NewVersion = null;
                var releases = LitJson.JsonMapper.ToObject<GithubReleases.Release[]>(request._www.text);
                foreach (var release in releases)
                {
                    var releaseVersion = new Tag(release.tag_name);
                    if (releaseVersion > currentVersion)
                    {
                        NewVersion = release;
                    }
                }
                if (NewVersion != null)
                {
                    var releaseVersion = new Tag(NewVersion.tag_name);
                    Server.logger.LogInfo($"[ServerEmulator] [Updater] A new version is available on GitHub! (v{releaseVersion})");

                    void ShowUpdateDialog()
                    {
                        if (DiscordRichPresence.GameState.CurrentScene == "MenuScene")
                        {
                            Emulator.Tools.Run.After(1f, () =>
                            {
                                Aquatrax.TipHelper.Instance.InitTipMode(Aquatrax.TipHelper.TipsMode.two);
                                Aquatrax.TipHelper.Instance.InitText($"An update for ServerEmulator has been found ({currentVersion} -> {releaseVersion}), update now?");
                                Aquatrax.TipHelper.Instance.Commit = delegate ()
                                {
                                    Procceed(NewVersion);
                                };
                            });
                            DiscordRichPresence.GameEvents.switchScene -= ShowUpdateDialog;
                        }
                    }
                    DiscordRichPresence.GameEvents.switchScene += ShowUpdateDialog;
                }
            }
        })));
    }

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
        if (assetLink == "")
        {
            return;
        }

        var outFile = Emulator.Tools.Path.Combine(BepInEx.Paths.BepInExRootPath, "plugins", "ServerEmulator.dll");
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
