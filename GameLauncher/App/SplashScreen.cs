﻿using System.Windows.Forms;
using GameLauncher.App.Classes.LauncherCore.Visuals;
using GameLauncher.App.Classes.LauncherCore.Global;
using System;
using GameLauncher.App.Classes.LauncherCore.Logger;
using System.Threading;

namespace GameLauncher.App.Classes
{
    public partial class SplashScreen : Form
    {
        /* Global Thread for Splash Screen */
        private static Thread SplashScreenThread;
        private static bool IsSplashScreenLive = false;

        private static void StartSplashScreen()
        {
            if (!IsSplashScreenLive)
            {
                new SplashScreen().ShowDialog();
                IsSplashScreenLive = true;
            }            
        }

        public static void ThreadStatus(string ContinueThread)
        {
            try
            {
                if (ContinueThread == "Start" && !IsSplashScreenLive)
                {
                    Log.Info("SPLASH SCREEN: Attempting to Start Thread");
                    SplashScreenThread = new Thread(new ThreadStart(StartSplashScreen));
                    SplashScreenThread.Start();
                    Log.Info("SPLASH SCREEN: Thread has now Started");
                }
                else if (ContinueThread == "Stop" && IsSplashScreenLive)
                {
                    Log.Info("SPLASH SCREEN: Attempting to Stop Thread");
                    SplashScreenThread.Abort();
                    Log.Completed("SPLASH SCREEN: Thread is now Stopped");
                }
                else
                {
                    Log.Info("SPLASH SCREEN: Thread has already been Stopped");
                }
            }
            catch (Exception Error)
            {
                LogToFileAddons.OpenLog("SPLASH SCREEN", null, Error, null, true);
            }
        }

        public SplashScreen()
        {
            InitializeComponent();

            /********************************/
            /* Set Theme Colors & Images     /
            /********************************/

            BackColor = Theming.SplashScreenTransparencyKey;
            TransparencyKey = Theming.SplashScreenTransparencyKey;
            BackgroundImage = Theming.LogoSplash;

            /*******************************/
            /* Set Window Name              /
            /*******************************/

            Text = "Splash Screen - SBRW Launcher: v" + Application.ProductVersion;

            this.Closing += (x, y) =>
            {
                IsSplashScreenLive = false;
            };
            Shown += (x, y) =>
            {
                IsSplashScreenLive = true;
            };
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            FunctionStatus.CenterScreen(this);
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            if (FunctionStatus.LoadingComplete || FunctionStatus.LauncherForceClose)
            {
                Clock.Start();

                try
                {
                    Application.OpenForms["SplashScreen"].Close();
                }
                catch (Exception Error)
                {
                    LogToFileAddons.OpenLog("SPLASH SCREEN", null, Error, null, true);
                    Close();
                }
            }
        }
    }
}
