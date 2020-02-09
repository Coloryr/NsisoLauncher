using MahApps.Metro;
using NsisoLauncher.Config;
using NsisoLauncher.Core.Util;
using NsisoLauncher.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.PhalAPI;
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
        public static LaunchHandler Handler;
        public static ConfigHandler Config;
        public static MultiThreadDownloader Downloader;
        public static LogHandler LogHandler;
        public static event EventHandler<AggregateExceptionArgs> AggregateExceptionCatched;
        public static List<Java> JavaList;
        public static MainWindow MainWindow_;
        public static DownloadWindow DownloadWindow_;
        public static NewDownloadTaskWindow NewDownloadTaskWindow_;

        public static APIHandler nsisoAPIHandler;

        public static void CatchAggregateException(object sender, AggregateExceptionArgs arg)
        {
            AggregateExceptionCatched?.Invoke(sender, arg);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            LogHandler = new LogHandler(true);
            Config = new ConfigHandler();
            Config.Environment env = Config.MainConfig.Environment;
            JavaList = Java.GetJavaList();

            AggregateExceptionCatched += (a, b) => LogHandler.AppendFatal(b.AggregateException);

            if (Config.MainConfig.Launcher.Debug || e.Args.Contains("-debug"))
            {
                DebugWindow debugWindow = new DebugWindow();
                debugWindow.Show();
                LogHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }

            nsisoAPIHandler = new APIHandler(Config.MainConfig.Launcher.NoTracking);

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
            LogHandler.AppendInfo("核心初始化->游戏根目录(默认则为空):" + gameroot);

            //设置JAVA
            Java java;
            if (env.AutoJava)
            {
                java = Java.GetSuitableJava(JavaList);
            }
            else
            {
                java = JavaList.Find(x => x.Path == env.JavaPath);
                if (java == null)
                {
                    java = Java.GetJavaInfo(env.JavaPath);
                }

            }
            if (java != null)
            {
                env.JavaPath = java.Path;
                LogHandler.AppendInfo("核心初始化->Java路径:" + java.Path);
                LogHandler.AppendInfo("核心初始化->Java版本:" + java.Version);
                LogHandler.AppendInfo("核心初始化->Java位数:" + java.Arch);
            }
            else
            {
                LogHandler.AppendWarn("核心初始化失败，当前电脑未匹配到JAVA");
            }

            bool verIso = Config.MainConfig.Environment.VersionIsolation;

            Handler = new LaunchHandler(gameroot, java, verIso);
            Handler.GameLog += (s, log) => LogHandler.AppendLog(s, new Log() { LogLevel = LogLevel.GAME, Message = log });
            Handler.LaunchLog += (s, log) => LogHandler.AppendLog(s, log);

            ServicePointManager.DefaultConnectionLimit = 10;

            Downloader = new MultiThreadDownloader();

            Re();

            Downloader.ProcessorSize = Config.MainConfig.Download.DownloadThreadsSize;
            Downloader.CheckFileHash = Config.MainConfig.Download.CheckDownloadFileHash;
            Downloader.DownloadLog += (s, log) => LogHandler?.AppendLog(s, log);

            var custom = Config.MainConfig.Customize;
            if (!string.IsNullOrWhiteSpace(custom.AccentColor) && !string.IsNullOrWhiteSpace(custom.AppThme))
            {
                LogHandler.AppendInfo("自定义->更改主题颜色:" + custom.AccentColor);
                LogHandler.AppendInfo("自定义->更改主题:" + custom.AppThme);
                ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(custom.AccentColor), ThemeManager.GetAppTheme(custom.AppThme));
            }

            Lauguage();

        }

        public static void Re()
        {
            Download downloadCfg = Config.MainConfig.Download;
            if (!string.IsNullOrWhiteSpace(downloadCfg.DownloadProxyAddress))
            {
                WebProxy proxy = new WebProxy(downloadCfg.DownloadProxyAddress, downloadCfg.DownloadProxyPort);
                if (!string.IsNullOrWhiteSpace(downloadCfg.ProxyUserName))
                {
                    NetworkCredential credential = new NetworkCredential(downloadCfg.ProxyUserName, downloadCfg.ProxyUserPassword);
                    proxy.Credentials = credential;
                }
                HttpRequesterAPI.Proxy = proxy;
            }
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        public static void Lauguage()
        {
            string lang = Config.MainConfig.Lauguage;
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
            LogHandler.AppendFatal(e.Exception);
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
            Current.Shutdown();
        }
    }

    //定义异常参数处理
    public class AggregateExceptionArgs : EventArgs
    {
        public AggregateException AggregateException { get; set; }
    }
}
