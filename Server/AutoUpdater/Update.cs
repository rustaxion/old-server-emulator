using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Server.AutoUpdater;

public static class Update
{
    public const string ReleasesAPI = "https://api.github.com/repos/Invaxion-Server-Emulator/invaxion-server-emulator/releases";

    public static void CheckForUpdate(Tag currentVersion)
    {
        var headers = new Dictionary<string, string> { { "Accept", "application/vnd.github.v3+json" } };
        var checkUpdateReq = new Networking.Request(ReleasesAPI, headers);
        Server.Instance.startCoroutine(checkUpdateReq.Download(((request, success) =>
        {
            if (success)
            {
                GithubReleases.Release newVersion = null;
                var newVersionTag = currentVersion;
                
                var releases = LitJson.JsonMapper.ToObject<GithubReleases.Release[]>(request._www.text);
                foreach (var release in releases)
                {
                    var releaseVersion = new Tag(release.tag_name);
                    if (releaseVersion > newVersionTag)
                    {
                        newVersion = release;
                        newVersionTag = releaseVersion;
                    }
                }

                if (newVersion == null) return;
                Server.logger.LogInfo($"[ServerEmulator] [Updater] A new version is available on GitHub! (v{newVersionTag})");

                void ShowUpdateDialog()
                {
                    if (DiscordRichPresence.GameState.CurrentScene == "MenuScene")
                    {
                        Emulator.Tools.Run.After(1f, () =>
                        {
                            Aquatrax.TipHelper.Instance.InitTipMode(Aquatrax.TipHelper.TipsMode.two);
                            Aquatrax.TipHelper.Instance.InitText($"An update for ServerEmulator has been found ({currentVersion} -> {newVersionTag}), update now?");
                            Aquatrax.TipHelper.Instance.Commit = delegate ()
                            {
                                Procceed(newVersion);
                            };
                        });
                        DiscordRichPresence.GameEvents.switchScene -= ShowUpdateDialog;
                    }
                }
                DiscordRichPresence.GameEvents.switchScene += ShowUpdateDialog;
            }
        })));
    }

    public static string GetUpdateScript(string url, string path)
    {
        var commands = new string[] {
            "Wait-Process -Name 'INVAXION' -ErrorAction SilentlyContinue",
            $"Invoke-WebRequest -Uri '{url}' -OutFile '{path}'",
            "Remove-Item -Path $MyInvocation.MyCommand.Source",
            "if ($Error)", "{", "Pause", "}"
        };
        return string.Join("\n", commands);
    }

    public static void Procceed(GithubReleases.Release release)
    {
        var assetLink = release.assets.FirstOrDefault(asset => asset.name == "ServerEmulator.dll");
        if (assetLink == null)
        {
            return;
        }

        var outFile = Emulator.Tools.Path.Combine(BepInEx.Paths.BepInExRootPath, "plugins", "ServerEmulator.dll");
        var script = GetUpdateScript(assetLink.browser_download_url, outFile);

        var scriptName = $"ServerEmulator_{new Tag(release.tag_name)}_update.ps1";
        var scriptPath = Path.Combine(BepInEx.Paths.BepInExRootPath, scriptName);
        File.WriteAllText(scriptPath, script, Encoding.UTF8);

        var updateProcess = new Process()
        {
            StartInfo =
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy ByPass -File {scriptName}",
                    WorkingDirectory = BepInEx.Paths.BepInExRootPath,
                }
        };
        updateProcess.Start();
        Application.Quit();
    }
}
