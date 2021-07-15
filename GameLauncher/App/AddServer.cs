﻿using Newtonsoft.Json;
using System;
using System.Net;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.String;
using GameLauncher.App.Classes.LauncherCore.Visuals;
using GameLauncher.App.Classes.LauncherCore.Lists.JSON;
using GameLauncher.App.Classes.SystemPlatform.Linux;
using GameLauncher.App.Classes.Hash;
using GameLauncher.App.Classes.LauncherCore.Global;
using GameLauncher.App.Classes.Logger;

namespace GameLauncher.App
{
    public partial class AddServer : Form
    {
        public AddServer()
        {
            InitializeComponent();
            SetVisuals();
        }

        public void DrawErrorAroundTextBox(TextBox x)
        {
            x.BorderStyle = BorderStyle.Fixed3D;
            Pen p = new Pen(Color.Red);
            Graphics g = this.CreateGraphics();
            int variance = 1;
            g.DrawRectangle(p, new Rectangle(x.Location.X - variance, x.Location.Y - variance, x.Width + variance, x.Height + variance));
        }

        private void SetVisuals()
        {
            /*******************************/
            /* Set Window Name              /
            /*******************************/

            Text = "Add Server - SBRW Launcher: v" + Application.ProductVersion;

            /*******************************/
            /* Set Hardcoded Text           /
            /*******************************/

            Version.Text = "Version : v" + Application.ProductVersion;

            /*******************************/
            /* Set Font                     /
            /*******************************/

            FontFamily DejaVuSans = FontWrapper.Instance.GetFontFamily("DejaVuSans.ttf");
            FontFamily DejaVuSansBold = FontWrapper.Instance.GetFontFamily("DejaVuSans-Bold.ttf");

            var MainFontSize = 9f * 100f / CreateGraphics().DpiY;

            if (DetectLinux.LinuxDetected())
            {
                MainFontSize = 9f;
            }

            Font = new Font(DejaVuSansBold, MainFontSize, FontStyle.Bold);
            OkBTN.Font = new Font(DejaVuSansBold, MainFontSize, FontStyle.Bold);
            CancelBTN.Font = new Font(DejaVuSansBold, MainFontSize, FontStyle.Bold);
            ServerNameLabel.Font = new Font(DejaVuSansBold, MainFontSize, FontStyle.Bold);
            ServerName.Font = new Font(DejaVuSans, MainFontSize, FontStyle.Regular);
            ServerAddress.Font = new Font(DejaVuSans, MainFontSize, FontStyle.Regular);
            ServerAddressLabel.Font = new Font(DejaVuSansBold, MainFontSize, FontStyle.Bold);
            Error.Font = new Font(DejaVuSansBold, MainFontSize, FontStyle.Bold);
            Version.Font= new Font(DejaVuSans, MainFontSize, FontStyle.Regular);

            /********************************/
            /* Set Theme Colors              /
            /********************************/

            BackColor = Theming.WinFormTBGForeColor;
            ForeColor = Theming.WinFormTextForeColor;

            ServerName.BackColor = Theming.WinFormTBGDarkerForeColor;
            ServerName.ForeColor = Theming.WinFormSecondaryTextForeColor;

            ServerAddress.BackColor = Theming.WinFormTBGDarkerForeColor;
            ServerAddress.ForeColor = Theming.WinFormSecondaryTextForeColor;

            Error.ForeColor = Theming.WinFormWarningTextForeColor;

            ServerNameLabel.ForeColor = Theming.WinFormTextForeColor;
            ServerAddressLabel.ForeColor = Theming.WinFormTextForeColor;
            Version.ForeColor = Theming.WinFormTextForeColor;

            CancelBTN.ForeColor = Theming.BlueForeColorButton;
            CancelBTN.BackColor = Theming.BlueBackColorButton;
            CancelBTN.FlatAppearance.BorderColor = Theming.BlueBorderColorButton;
            CancelBTN.FlatAppearance.MouseOverBackColor = Theming.BlueMouseOverBackColorButton;

            OkBTN.ForeColor = Theming.BlueForeColorButton;
            OkBTN.BackColor = Theming.BlueBackColorButton;
            OkBTN.FlatAppearance.BorderColor = Theming.BlueBorderColorButton;
            OkBTN.FlatAppearance.MouseOverBackColor = Theming.BlueMouseOverBackColorButton;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (!File.Exists("servers.json"))
            {
                File.Create("servers.json");
            }

            bool success = true;
            Error.Visible = false;
            this.Refresh();

            String wellFormattedURL = "";

            if (IsNullOrWhiteSpace(ServerAddress.Text))
            {
                DrawErrorAroundTextBox(ServerAddress);
                success = false;
            }

            if (IsNullOrWhiteSpace(ServerName.Text))
            {
                DrawErrorAroundTextBox(ServerName);
                success = false;
            }

            bool result = Uri.TryCreate(ServerAddress.Text, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!result)
            {
                DrawErrorAroundTextBox(ServerAddress);
                success = false;
            }
            else
            {
                wellFormattedURL = uriResult.ToString();
            }

            CancelBTN.Enabled = false;
            OkBTN.Enabled = false;
            ServerAddress.Enabled = false;
            ServerName.Enabled = false;

            try
            {
                FunctionStatus.TLS();
                Uri StringToUri = new Uri(wellFormattedURL + "/GetServerInformation");
                ServicePointManager.FindServicePoint(StringToUri).ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                WebClient client = new WebClient();
                
                var serverLoginResponse = client.DownloadString(StringToUri);

                GetServerInformation json = JsonConvert.DeserializeObject<GetServerInformation>(serverLoginResponse);

                if (IsNullOrWhiteSpace(json.serverName))
                {
                    DrawErrorAroundTextBox(ServerAddress);
                    success = false;
                }
            }
            catch
            {
                DrawErrorAroundTextBox(ServerAddress);
                success = false;
            }

            CancelBTN.Enabled = true;
            OkBTN.Enabled = true;
            ServerAddress.Enabled = true;
            ServerName.Enabled = true;

            if (success == true)
            {
                try
                {
                    StreamReader sr = new StreamReader("servers.json");
                    String oldcontent = sr.ReadToEnd();
                    sr.Close();

                    if (IsNullOrWhiteSpace(oldcontent))
                    {
                        oldcontent = "[]";
                    }

                    var servers = JsonConvert.DeserializeObject<List<ServerList>>(oldcontent);

                    servers.Add(new ServerList
                    {
                        Name = ServerName.Text,
                        IpAddress = wellFormattedURL,
                        IsSpecial = false,
                        Id = SHA.Hashes(uriResult.Host)
                    });

                    File.WriteAllText("servers.json", JsonConvert.SerializeObject(servers));

                    MessageBox.Show(null, "New server will be added on next start of launcher.", "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception Error)
                {
                    Log.Error("ADD SERVER: " + Error.Message);
                    Log.ErrorIC("ADD SERVER: " + Error.HResult);
                    Log.ErrorFR("ADD SERVER: " + Error.ToString());

                    MessageBox.Show(null, "Failed to add new server.\n" + Error.Message, "GameLauncher", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                }

                CancelButton_Click(sender, e);
            }
            else
            {
                Error.Visible = true;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
