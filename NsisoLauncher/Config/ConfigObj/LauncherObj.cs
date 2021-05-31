namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 启动器设置
    /// </summary>
    public record LauncherObj
    {
        /// <summary>
        /// 是否开启DEBUG模式
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// 是否禁止数据追踪
        /// </summary>
        public bool NoTracking { get; set; }

        /// <summary>
        /// 自动启动游戏
        /// </summary>
        public bool AutoRun { get; set; }
    }
}
