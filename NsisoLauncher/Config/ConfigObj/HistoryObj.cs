using System;

namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 历史记录设置
    /// </summary>
    public record HistoryObj
    {
        /// <summary>
        /// 选中的用户的ID
        /// </summary>
        public string SelectedUserNodeID { get; set; }

        /// <summary>
        /// 上一次启动版本
        /// </summary>
        public string LastLaunchVersion { get; set; }

        /// <summary>
        /// 上次启动时间
        /// </summary>
        public DateTime LastLaunchTime { get; set; }

        /// <summary>
        /// 上次启动使用的时间(Ms)
        /// </summary>
        public long LastLaunchUsingMs { get; set; }
    }
}
