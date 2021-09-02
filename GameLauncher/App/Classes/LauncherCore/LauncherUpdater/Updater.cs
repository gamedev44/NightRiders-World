﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using GameLauncher.App.Classes.LauncherCore.FileReadWrite;
using GameLauncher.App.Classes.LauncherCore.Visuals;
using GameLauncher.App.Classes.LauncherCore.Global;
using System.Net;
using GameLauncher.App.Classes.InsiderKit;
using GameLauncher.App.Classes.LauncherCore.RPC;
using GameLauncher.App.Classes.LauncherCore.APICheckers;
using System.Text;
using GameLauncher.App.Classes.LauncherCore.Logger;
using GameLauncher.App.Classes.LauncherCore.Validator.JSON;
using GameLauncher.App.Classes.LauncherCore.Client.Web;
using GameLauncher.App.Classes.LauncherCore.Support;

namespace GameLauncher.App.Classes.LauncherCore.LauncherUpdater
{
    class LauncherUpdateCheck
    {
        public PictureBox status;
        public Label text;
        public Label description;

        public static string CurrentLauncherBuild = Application.ProductVersion;
        private static string LatestLauncherBuild;
        public static bool UpgradeAvailable = false;
        private static string VersionJSON;
        private static bool ValidJSONDownload = false;

        public LauncherUpdateCheck(PictureBox statusImage, Label statusText, Label statusDescription)
        {
            status = statusImage;
            text = statusText;
            description = statusDescription;
        }

        public static void Latest()
        {
            Log.Checking("LAUNCHER UPDATE: Is Version Up to Date or not");
            DiscordLauncherPresence.Status("Start Up", "Checking Latest Launcher Release Information");

            if (VisualsAPIChecker.UnitedAPI())
            {
                try
                {
                    bool IsAPIOnline = false;
                    FunctionStatus.TLS();
                    Uri URLCall = new Uri(URLs.Main + "/update.php?version=" + Application.ProductVersion);
                    ServicePointManager.FindServicePoint(URLCall).ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                    var Client = new WebClient
                    {
                        Encoding = Encoding.UTF8
                    };

                    if (!WebCalls.Alternative()) { Client = new WebClientWithTimeout { Encoding = Encoding.UTF8 }; }
                    else
                    {
                        Client.Headers.Add("user-agent", "SBRW Launcher " +
                        Application.ProductVersion + " (+https://github.com/SoapBoxRaceWorld/GameLauncher_NFSW)");
                    }

                    try
                    {
                        VersionJSON = Client.DownloadString(URLCall);
                        IsAPIOnline = true;
                    }
                    catch (WebException Error)
                    {
                        APIChecker.StatusCodes(URLCall.GetComponents(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped),
                            Error, (HttpWebResponse)Error.Response);
                    }
                    catch (Exception Error)
                    {
                        LogToFileAddons.OpenLog("LAUNCHER UPDATE", null, Error, null, true);
                    }
                    finally
                    {
                        if (Client != null)
                        {
                            Client.Dispose();
                        }
                    }

                    if (IsJSONValid.ValidJson(VersionJSON) && IsAPIOnline)
                    {
                        UpdateCheckResponse MAPI = JsonConvert.DeserializeObject<UpdateCheckResponse>(VersionJSON);

                        if (MAPI.Payload.LatestVersion != null)
                        {
                            LatestLauncherBuild = MAPI.Payload.LatestVersion;
                            Log.Info("LAUNCHER UPDATE: Latest Version -> " + LatestLauncherBuild);
                            ValidJSONDownload = true;
                        }
                    }
                    else
                    {
                        Log.Warning("LAUNCHER UPDATE: Received Invalid JSON Data");
                    }
                }
                catch (Exception Error)
                {
                    LogToFileAddons.OpenLog("LAUNCHER UPDATE", null, Error, null, true);
                }
                finally
                {
                    if (VersionJSON != null)
                    {
                        VersionJSON = null;
                    }
                }
            }

            if (!VisualsAPIChecker.UnitedAPI() || !ValidJSONDownload)
            {
                try
                {
                    FunctionStatus.TLS();
                    Uri URLCall = new Uri(URLs.GitHub_Launcher);
                    ServicePointManager.FindServicePoint(URLCall).ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                    var Client = new WebClient
                    {
                        Encoding = Encoding.UTF8
                    };

                    if (!WebCalls.Alternative()) { Client = new WebClientWithTimeout { Encoding = Encoding.UTF8 }; }
                    else
                    {
                        Client.Headers.Add("user-agent", "SBRW Launcher " +
                        Application.ProductVersion + " (+https://github.com/SoapBoxRaceWorld/GameLauncher_NFSW)");
                    }

                    try
                    {
                        VersionJSON = Client.DownloadString(URLCall);
                        VisualsAPIChecker.GitHubAPI = true;
                    }
                    catch (WebException Error)
                    {
                        APIChecker.StatusCodes(URLCall.GetComponents(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped),
                            Error, (HttpWebResponse)Error.Response);
                    }
                    catch (Exception Error)
                    {
                        LogToFileAddons.OpenLog("LAUNCHER UPDATE [GITHUB]", null, Error, null, true);
                    }
                    finally
                    {
                        if (Client != null)
                        {
                            Client.Dispose();
                        }
                    }

                    if (IsJSONValid.ValidJson(VersionJSON) && VisualsAPIChecker.GitHubAPI)
                    {
                        GitHubRelease GHAPI = JsonConvert.DeserializeObject<GitHubRelease>(VersionJSON);

                        if (GHAPI.TagName != null)
                        {
                            LatestLauncherBuild = GHAPI.TagName;
                            Log.Info("LAUNCHER UPDATE: GitHub Latest Version -> " + LatestLauncherBuild);
                            ValidJSONDownload = true;
                        }

                        if (GHAPI != null)
                        {
                            GHAPI = null;
                        }
                    }
                    else
                    {
                        ValidJSONDownload = false;
                    }
                }
                catch (Exception Error)
                {
                    LogToFileAddons.OpenLog("LAUNCHER UPDATE [GITHUB]", null, Error, null, true);
                }
                finally
                {
                    if (VersionJSON != null)
                    {
                        VersionJSON = null;
                    }
                }

                if (!VisualsAPIChecker.GitHubAPI)
                {
                    Log.Error("LAUNCHER UPDATE: Failed to retrive Latest Build information from two APIs ");
                }
            }
            Log.Completed("LAUNCHER UPDATE: Done");
            Log.Info("FIRST TIME RUN: Moved to Function");
            /* Do First Time Run Checks */
            FunctionStatus.FirstTimeRun();
        }

        public void ChangeVisualStatus()
        {
            if (!string.IsNullOrWhiteSpace(LatestLauncherBuild))
            {
                var Revisions = CurrentLauncherBuild.CompareTo(LatestLauncherBuild);

                if (Revisions > 0)
                {
                    string WhatBuildAmI;
                    if (EnableInsiderDeveloper.Allowed())
                    {
                        WhatBuildAmI = "Developer";
                    }
                    else if (EnableInsiderBetaTester.Allowed())
                    {
                        WhatBuildAmI = "Beta";
                    }
                    else
                    {
                        WhatBuildAmI = "Unofficial";
                    }

                    text.Text = "Launcher Status:\n - " + WhatBuildAmI + " Build";
                    status.BackgroundImage = Theming.UpdateIconWarning;
                    text.ForeColor = Theming.Warning;
                    description.Text = "Stable: v" + LatestLauncherBuild + "\nCurrent: v" + Application.ProductVersion;

                    if (!string.IsNullOrWhiteSpace(FileSettingsSave.IgnoreVersion))
                    {
                        FileSettingsSave.IgnoreVersion = String.Empty;
                        FileSettingsSave.SaveSettings();
                        Log.Info("IGNOREUPDATEVERSION: Cleared OLD IgnoreUpdateVersion Build Number. " +
                            "You are currenly using a " + WhatBuildAmI + " Build!");
                    }
                }
                else if (Revisions == 0)
                {
                    text.Text = "Launcher Status:\n - Current Version";
                    status.BackgroundImage = Theming.UpdateIconSuccess;
                    text.ForeColor = Theming.Sucess;
                    description.Text = "Version: " + Application.ProductVersion;

                    if (FileSettingsSave.IgnoreVersion == Application.ProductVersion)
                    {
                        FileSettingsSave.IgnoreVersion = String.Empty;
                        FileSettingsSave.SaveSettings();
                        Log.Info("IGNOREUPDATEVERSION: Cleared OLD IgnoreUpdateVersion Build Number. You're now on the Latest Game Launcher!");
                    }
                }
                else
                {
                    text.Text = "Launcher Status:\n - Update Available";
                    status.BackgroundImage = Theming.UpdateIconWarning;
                    text.ForeColor = Theming.Warning;
                    description.Text = "New: v" + LatestLauncherBuild + "\nCurrent: v" + Application.ProductVersion;
                    UpgradeAvailable = true;

                    if (FileSettingsSave.IgnoreVersion == LatestLauncherBuild)
                    {
                        /* No Update Popup
                           Blame DavidCarbon if this Breaks (to some degree), not Zacam...*/
                    }
                    else
                    {
                        DialogResult updateConfirm = new UpdatePopup(LatestLauncherBuild).ShowDialog();

                        if (updateConfirm == DialogResult.OK)
                        {
                            string UpdaterPath = Strings.Encode(Path.Combine(Locations.LauncherFolder, Locations.NameUpdater));
                            if (File.Exists(UpdaterPath))
                            {
                                Process.Start(UpdaterPath, Process.GetCurrentProcess().Id.ToString());
                            }
                            else
                            {
                                Process.Start(@"https://github.com/SoapboxRaceWorld/GameLauncher_NFSW/releases/latest");
                                MessageBox.Show(null, "A Web Browser has been opened to the Game Launcher's GitHub Latest Release Page " +
                                    "to Download the New Version", "GameLauncher", MessageBoxButtons.OK);
                            }
                        };

                        /* Check if User clicked Ignore so it doesn't update "IgnoreUpdateVersion" */
                        if (updateConfirm == DialogResult.Cancel)
                        {
                            FileSettingsSave.IgnoreVersion = String.Empty;
                        };

                        /* Write to Settings.ini to Skip Update */
                        if (updateConfirm == DialogResult.Ignore)
                        {
                            FileSettingsSave.IgnoreVersion = LatestLauncherBuild;
                        };
                    }
                    FileSettingsSave.SaveSettings();
                }
            }
            else if (VisualsAPIChecker.GitHubAPI && !ValidJSONDownload)
            {
                text.Text = "Launcher Status:\n - Invalid JSON";
                status.BackgroundImage = Theming.UpdateIconError;
                text.ForeColor = Theming.Error;
                description.Text = "Version: v" + Application.ProductVersion;
            }
            else
            {
                text.Text = "Launcher Status:\n - API Version Error";
                status.BackgroundImage = Theming.UpdateIconUnknown;
                text.ForeColor = Theming.ThirdTextForeColor;
                description.Text = "Version: v" + Application.ProductVersion;
            }
        }
    }
}
