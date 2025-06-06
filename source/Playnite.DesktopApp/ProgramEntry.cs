﻿using CommandLine;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.DesktopApp
{
    public class ProgramEntry
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var cmdLine = new CmdLineOptions();
            var parsed = Parser.Default.ParseArguments<CmdLineOptions>(Environment.GetCommandLineArgs());
            if (parsed is Parsed<CmdLineOptions> options)
            {
                cmdLine = options.Value;
            }

            if (!cmdLine.UserDataDir.IsNullOrWhiteSpace())
            {
                try
                {
                    cmdLine.UserDataDir = cmdLine.UserDataDir.TrimEnd(new char[] { '/', '\\', '"' });
                    FileSystem.CreateDirectory(cmdLine.UserDataDir, false);
                    PlaynitePaths.UpdateUserDataDir(cmdLine.UserDataDir);
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        $"Failed to initialize in specified user data folder:\n{e.Message}",
                        "Startup Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }

            FileSystem.CreateDirectory(PlaynitePaths.JitProfilesPath);
            ProfileOptimization.SetProfileRoot(PlaynitePaths.JitProfilesPath);
            ProfileOptimization.StartProfile("desktop");

            if (Computer.WindowsVersion == WindowsVersion.Win7 || Computer.WindowsVersion == WindowsVersion.Win8)
            {
                MessageBox.Show(
                     "Windows 7 and Windows 8 are no longer supported. Please update your operating system or downgrade to older Playnite version.",
                     "Startup Error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);
                return;
            }

            if (PlaynitePaths.ProgramPath.Contains(@"temp\rar$", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Playnite is not allowed to run from temporary extracted archive.\rInstall/Extract application properly before starting it.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            else if (PlaynitePaths.ProgramPath.Contains("#"))
            {
                MessageBox.Show(
                    "Playnite is unable to run from current directory due to illegal character '#' in the path. Please use different directory.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            SplashScreen splash = null;
            var procCount = Process.GetProcesses().Where(a => a.ProcessName.StartsWith("Playnite.")).Count();
            if (cmdLine.Start.IsNullOrEmpty() && !cmdLine.HideSplashScreen && procCount == 1)
            {
                splash = new SplashScreen("SplashScreen.png");
                splash.Show(false);
            }

            PlayniteSettings.ConfigureLogger();
            LogManager.GetLogger().Info($"App arguments: '{string.Join(",", args)}'");
            var app = new DesktopApplication(() => new App(), splash, cmdLine);
            app.Run();
        }
    }
}
