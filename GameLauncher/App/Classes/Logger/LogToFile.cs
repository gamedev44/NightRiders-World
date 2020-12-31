﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameLauncher.App.Classes.Logger
{
    class Log
    {
        private static ConcurrentQueue<string> buffer = new ConcurrentQueue<string>();
        private static String filename = String.Empty;

        public Log(String file = "launcher.log") => filename = file;
        public void StartLogging() => Task.Run(() => TaskKernel());
        private static void _toFile(string text, string errorname = "DEBUG") => buffer.Enqueue($"[{errorname}] {text}");

        public void Debug(string text) => _toFile(text, "DEBUG");
        public void Info(string text) => _toFile(text, " INFO");
        public void Warning(string text) => _toFile(text, " WARN");
        public void Error(string text) => _toFile(text, "ERROR");
        public void UrlCall(string text) => _toFile(text, "  URL");
        public void System(string text) => _toFile(text, "SYSTM");
        public void Build(string text) => _toFile(text, "BUILD");
        public void Visuals(string text) => _toFile(text, "VISUL");
        public void Api(string text) => _toFile(text, "  API");
        public void Core(string text) => _toFile(text, " CORE");

        private static async void TaskKernel()
        {
            while (true)
            {
                if (buffer.Count > 0 && buffer.TryDequeue(out string merged))
                {
                    try {
                        File.AppendAllText(filename, merged + Environment.NewLine);
                    } catch(Exception ex) {
                        Log launcherLog = new Log(filename);
                        launcherLog.Error(ex.Message);
                    }
                    Console.WriteLine(merged);
                } else
                {
                    await Task.Delay(30);
                }
            }
        }
    }
    class LogVerify
    {
        private static ConcurrentQueue<string> buffer = new ConcurrentQueue<string>();
        public static void StartVerifyLogging() => Task.Run(() => VerifyTaskKernel());
        private static void _toFile(string text, string errorname = "DEBUG") => buffer.Enqueue($"[{errorname}] {text}");
        public static void Valid(string text) => _toFile(text, "   VAILD");
        public static void Invalid(string text) => _toFile(text, " INVALID");
        public static void Deleted(string text) => _toFile(text, " DELETED");
        public static void Missing(string text) => _toFile(text, " MISSING");
        public static void Downloaded(string text) => _toFile(text, "REPLACED");
        private static async void VerifyTaskKernel()
        {
            while (true)
            {
                if (buffer.Count > 0 && buffer.TryDequeue(out string merged))
                {
                    File.AppendAllText("Verify.log", merged + Environment.NewLine);
                    Console.WriteLine(merged);
                }
                else
                {
                    await Task.Delay(30);
                }
            }
        }
    }
}
