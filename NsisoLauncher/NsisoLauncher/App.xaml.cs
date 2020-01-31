﻿using MahApps.Metro;
using NsisoLauncher.Config;
using NsisoLauncher.Core.Util;
using NsisoLauncherCore;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace NsisoLauncher
{

    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static LaunchHandler handler;
        public static ConfigHandler config;
        public static MultiThreadDownloader downloader;
        public static LogHandler logHandler;
        public static event EventHandler<AggregateExceptionArgs> AggregateExceptionCatched;
        public static List<Java> javaList;
        public static MainWindow MainWindow_;

        public static NsisoLauncherCore.Net.PhalAPI.APIHandler nsisoAPIHandler;

        public static void CatchAggregateException(object sender, AggregateExceptionArgs arg)
        {
            AggregateExceptionCatched?.Invoke(sender, arg);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            #region DEBUG初始化
            //debug
            logHandler = new LogHandler(true);
            AggregateExceptionCatched += (a, b) => logHandler.AppendFatal(b.AggregateException);
            if (e.Args.Contains("-debug"))
            {
                Windows.DebugWindow debugWindow = new Windows.DebugWindow();
                debugWindow.Show();
                logHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }
            #endregion

            config = new ConfigHandler();
            
            #region DEBUG初始化（基于配置文件）
            if (config.MainConfig.Launcher.Debug && !e.Args.Contains("-debug"))
            {
                Windows.DebugWindow debugWindow = new Windows.DebugWindow();
                debugWindow.Show();
                logHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }
            #endregion

            #region Nsiso反馈API初始化

#if DEBUG
            nsisoAPIHandler = new NsisoLauncherCore.Net.PhalAPI.APIHandler(true);
#else
            nsisoAPIHandler = new NsisoLauncherCore.Net.PhalAPI.APIHandler(config.MainConfig.Launcher.NoTracking);
#endif

            #endregion

            #region 数据初始化
            Config.Environment env = config.MainConfig.Environment;

            javaList = Java.GetJavaList();

            //设置版本路径
            string gameroot = null;
            switch (env.GamePathType)
            {
                case GameDirEnum.ROOT:
                    gameroot = Path.GetFullPath(".minecraft");
                    break;
                case GameDirEnum.APPDATA:
                    gameroot = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                    break;
                case GameDirEnum.PROGRAMFILES:
                    gameroot = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\.minecraft";
                    break;
                case GameDirEnum.CUSTOM:
                    gameroot = env.GamePath + "\\.minecraft";
                    break;
                default:
                    throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
            }
            logHandler.AppendInfo("核心初始化->游戏根目录(默认则为空):" + gameroot);

            //设置JAVA
            Java java = null;
            if (env.AutoJava)
            {
                java = Java.GetSuitableJava(javaList);
            }
            else
            {
                java = javaList.Find(x => x.Path == env.JavaPath);
                if (java == null)
                {
                    java = Java.GetJavaInfo(env.JavaPath);
                }

            }
            if (java != null)
            {
                env.JavaPath = java.Path;
                logHandler.AppendInfo("核心初始化->Java路径:" + java.Path);
                logHandler.AppendInfo("核心初始化->Java版本:" + java.Version);
                logHandler.AppendInfo("核心初始化->Java位数:" + java.Arch);
            }
            else
            {
                logHandler.AppendWarn("核心初始化失败，当前电脑未匹配到JAVA");
            }

            //设置版本独立
            bool verIso = config.MainConfig.Environment.VersionIsolation;
            #endregion

            #region 启动核心初始化
            handler = new LaunchHandler(gameroot, java, verIso);
            handler.GameLog += (s, log) => logHandler.AppendLog(s, new Log() { LogLevel = LogLevel.GAME, Message = log });
            handler.LaunchLog += (s, log) => logHandler.AppendLog(s, log);
            #endregion

            #region 下载核心初始化
            ServicePointManager.DefaultConnectionLimit = 10;

            Download downloadCfg = config.MainConfig.Download;
            downloader = new MultiThreadDownloader();
            if (!string.IsNullOrWhiteSpace(downloadCfg.DownloadProxyAddress))
            {
                WebProxy proxy = new WebProxy(downloadCfg.DownloadProxyAddress, downloadCfg.DownloadProxyPort);
                if (!string.IsNullOrWhiteSpace(downloadCfg.ProxyUserName))
                {
                    NetworkCredential credential = new NetworkCredential(downloadCfg.ProxyUserName, downloadCfg.ProxyUserPassword);
                    proxy.Credentials = credential;
                }
                downloader.Proxy = proxy;
            }
            downloader.ProcessorSize = App.config.MainConfig.Download.DownloadThreadsSize;
            downloader.CheckFileHash = App.config.MainConfig.Download.CheckDownloadFileHash;
            downloader.DownloadLog += (s, log) => logHandler?.AppendLog(s, log);
            #endregion

            #region 自定义主题初始化
            var custom = config.MainConfig.Customize;
            if (!string.IsNullOrWhiteSpace(custom.AccentColor) && !string.IsNullOrWhiteSpace(custom.AppThme))
            {
                logHandler.AppendInfo("自定义->更改主题颜色:" + custom.AccentColor);
                logHandler.AppendInfo("自定义->更改主题:" + custom.AppThme);
                ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(custom.AccentColor), ThemeManager.GetAppTheme(custom.AppThme));
            }
            #endregion

            lan();

            //debug
        }

        public static void lan()
        {
            string lang = config.MainConfig.Lauguage;
            if (lang == "中文")
            {
                lang = "zh_CN.xaml";
            }
            else if (lang == "English")
            {
                lang = "en_US.xaml";
            }
            else if (lang == "日本語")
            {
                lang = "ja_JP.xaml";
            }
            try
            {
                Current.Resources.MergedDictionaries.Add(LoadComponent(new Uri("/NsisoLauncher;component/Resource/Language/" + lang, UriKind.Relative)) as ResourceDictionary);
            }
            catch
            {
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            App.logHandler.AppendFatal(e.Exception);
            e.Handled = true;
        }

        public static string GetResourceString(string key)
        {
            return (string)Current.FindResource(key);
        }

        /// <summary>
        /// 重启启动器
        /// </summary>
        /// <param name="admin">是否用管理员模式重启</param>
        public static void Reboot(bool admin)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var args = System.Environment.GetCommandLineArgs();
            foreach (var item in args)
            {
                info.Arguments += (item + ' ');
            }
            if (admin)
            {
                info.Verb = "runas";
            }
            info.Arguments += "-reboot";
            System.Diagnostics.Process.Start(info);
            App.Current.Shutdown();
        }
    }

    //定义异常参数处理
    public class AggregateExceptionArgs : EventArgs
    {
        public AggregateException AggregateException { get; set; }
    }
}
