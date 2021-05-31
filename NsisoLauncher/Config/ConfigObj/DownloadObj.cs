using NsisoLauncherCore.Net;

namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 下载设置
    /// </summary>
    public record DownloadObj
    {
        /// <summary>
        /// 下载源设置
        /// </summary>
        public DownloadSource DownloadSource { get; set; }

        /// <summary>
        /// 线程数量
        /// </summary>
        public int DownloadThreadsSize { get; set; }

        /// <summary>
        /// 代理下载服务器地址
        /// </summary>
        public string DownloadProxyAddress { get; set; }

        /// <summary>
        /// 代理下载服务器端口
        /// </summary>
        public ushort DownloadProxyPort { get; set; }

        /// <summary>
        /// 代理服务器账号
        /// </summary>
        public string ProxyUserName { get; set; }

        /// <summary>
        /// 代理服务器密码
        /// </summary>
        public string ProxyUserPassword { get; set; }

        /// <summary>
        /// 下载后是否检查哈希值（前提为可用）
        /// </summary>
        public bool CheckDownloadFileHash { get; set; }
    }
}
