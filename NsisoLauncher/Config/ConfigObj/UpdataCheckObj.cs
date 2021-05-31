namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 升级检查
    /// </summary>
    public record UpdataCheckObj
    {
        /// <summary>
        /// 启用资源检查
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 资源更新检查地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 整合包名字
        /// </summary>
        public string Packname { get; set; }
        /// <summary>
        /// 整合包版本
        /// </summary>
        public string Version { get; set; }
    }
}
