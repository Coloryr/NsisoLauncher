namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 服务器设置
    /// </summary>
    public record ServerObj
    {
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 是否在主界面显示服务器信息
        /// </summary>
        public bool ShowServerInfo { get; set; }

        /// <summary>
        /// 是否在启动后直接进入服务器
        /// </summary>
        public bool LaunchToServer { get; set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// 检测更新
        /// </summary>
        public UpdataCheckObj UpdataCheck { get; set; }
    }
}
