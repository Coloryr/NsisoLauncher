namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 自定义设置
    /// </summary>
    public record CustomizeObj
    {
        /// <summary>
        /// 是否使用自定义背景视频
        /// </summary>
        public bool CustomBackGroundVedio { get; set; }

        /// <summary>
        /// 是否使用自定义背景视频循环播放
        /// </summary>
        public bool CustomBackGroundVedioCyclic { get; set; }

        /// <summary>
        /// 使用使用随机播放
        /// </summary>
        public bool CustomBackGroundVedioRandom { get; set; }

        /// <summary>
        /// 是否使用自定义背景音乐
        /// </summary>
        public bool CustomBackGroundMusic { get; set; }

        /// <summary>
        /// 是否使用自定义音乐循环播放
        /// </summary>
        public bool CustomBackGroundMusicCyclic { get; set; }

        /// <summary>
        /// 是否随机播放音乐
        /// </summary>
        public bool CustomBackGroundMusicRandom { get; set; }

        /// <summary>
        /// 是否使用自定义壁纸
        /// </summary>
        public bool CustomBackGroundPicture { get; set; }

        /// <summary>
        /// 是否使用自定义壁纸循环
        /// </summary>
        public bool CustomBackGroundPictureCyclic { get; set; }

        /// <summary>
        /// 背景高斯模糊
        /// </summary>
        public bool CustomBackGroundPictureBlurEffect { get; set; }

        /// <summary>
        /// 模糊程度
        /// </summary>
        public int CustomBackGroundPictureBlurEffectSize { get; set; }

        /// <summary>
        /// 是否使用自定义壁纸循环时间
        /// </summary>
        public int CustomBackGroundPictureCyclicTime { get; set; }

        /// <summary>
        /// 背景音乐大小
        /// </summary>
        public int CustomBackGroundSound { get; set; }

        /// <summary>
        /// 主题Thme
        /// </summary>
        public string AppThme { get; set; }

        /// <summary>
        /// 启动器标题
        /// </summary>
        public string LauncherTitle { get; set; }

        /// <summary>
        /// 游戏窗口标题
        /// </summary>
        public string GameWindowTitle { get; set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        public string VersionInfo { get; set; }
    }
}
