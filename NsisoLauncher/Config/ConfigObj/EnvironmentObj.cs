using NsisoLauncherCore.Modules;

namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 启动环境基本设置
    /// </summary>
    public record EnvironmentObj
    {
        /// <summary>
        /// 版本隔离
        /// </summary>
        public bool VersionIsolation { get; set; }

        /// <summary>
        /// 游戏路径类型
        /// </summary>
        public GameDirEnum GamePathType { get; set; }

        /// <summary>
        /// 游戏根路径
        /// </summary>
        public string GamePath { get; set; }

        /// <summary>
        /// 是否启动垃圾回收，默认开启
        /// </summary>
        public bool GCEnabled { get; set; }

        /// <summary>
        /// 垃圾回收器种类(默认G1)
        /// </summary>
        public GCType GCType { get; set; }

        /// <summary>
        /// 垃圾回收器附加参数
        /// </summary>
        public string GCArgument { get; set; }

        /// <summary>
        /// 是否使用自动选择java
        /// </summary>
        public bool AutoJava { get; set; }

        /// <summary>
        /// 启动所使用JAVA路径
        /// </summary>
        public string JavaPath { get; set; }

        /// <summary>
        /// JavaAgent参数
        /// </summary>
        public string JavaAgent { get; set; }

        /// <summary>
        /// 游戏最大内存
        /// </summary>
        public int MaxMemory { get; set; }

        /// <summary>
        /// 游戏最小内存
        /// </summary>
        public int MinMemory { get; set; }

        /// <summary>
        /// 附加虚拟机启动参数
        /// </summary>
        public string AdvencedJvmArguments { get; set; }

        /// <summary>
        /// 附加游戏启动参数
        /// </summary>
        public string AdvencedGameArguments { get; set; }

        /// <summary>
        /// 游戏窗口大小
        /// </summary>
        public WindowSize WindowSize { get; set; }

        /// <summary>
        /// 是否下载丢失游戏依赖库
        /// </summary>
        public bool DownloadLostDepend { get; set; }

        /// <summary>
        /// 是否下载丢失游戏资源库
        /// </summary>
        public bool DownloadLostAssets { get; set; }

        /// <summary>
        /// 启动后退出启动器
        /// </summary>
        public bool ExitAfterLaunch { get; set; }
    }
}
