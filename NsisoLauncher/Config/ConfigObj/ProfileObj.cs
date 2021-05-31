namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// profile文件
    /// </summary>
    public record ProfileObj
    {
        /// <summary>
        /// 显示的玩家昵称.
        /// </summary>
        public string DisplayName { get; internal set; }
    }
}
