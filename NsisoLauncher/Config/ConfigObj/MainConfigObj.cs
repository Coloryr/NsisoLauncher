namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 主设置
    /// </summary>
    public record MainConfigObj
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public UserObj User { get; set; }

        /// <summary>
        /// 启动环境设置
        /// </summary>
        public EnvironmentObj Environment { get; set; }

        /// <summary>
        /// 历史数据
        /// </summary>
        public HistoryObj History { get; set; }

        /// <summary>
        /// 启动器设置
        /// </summary>
        public LauncherObj Launcher { get; set; }

        /// <summary>
        /// 下载设置
        /// </summary>
        public DownloadObj Download { get; set; }

        /// <summary>
        /// 服务器设置
        /// </summary>
        public ServerObj Server { get; set; }

        /// <summary>
        /// 自定义设置
        /// </summary>
        public CustomizeObj Customize { get; set; }

        /// <summary>
        /// 配置文件版本
        /// </summary>
        public string ConfigVersion { get; set; }

        /// <summary>
        /// 语言设置
        /// </summary>
        public string Lauguage { get; set; }
    }
}
