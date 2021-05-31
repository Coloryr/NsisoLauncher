namespace NsisoLauncher.Config.ConfigObj
{
    public enum HeadType
    {
        /// <summary>
        /// 直接获取图片
        /// </summary>
        URL,

        /// <summary>
        /// 通过JSON解密图片
        /// </summary>
        JSON
    }
    public enum AuthenticationType
    {
        /// <summary>
        /// 离线验证
        /// </summary>
        OFFLINE,

        /// <summary> 
        /// 官方正版验证
        /// </summary>
        MOJANG,

        /// <summary>
        /// 统一验证
        /// </summary>
        NIDE8,

        /// <summary>
        /// authlib-injector验证
        /// </summary>
        AUTHLIB_INJECTOR,

        /// <summary>
        /// 自定义服务器验证
        /// </summary>
        CUSTOM_SERVER,

        /// <summary>
        /// 微软登录
        /// </summary>
        MICROSOFT
    }

    public enum GameDirEnum
    {
        /// <summary>
        /// 启动器根目录
        /// </summary>
        ROOT = 0,

        /// <summary>
        /// 系统AppData
        /// </summary>
        APPDATA = 1,

        /// <summary>
        /// 系统程序文件夹
        /// </summary>
        PROGRAMFILES = 2,

        /// <summary>
        /// 自定义
        /// </summary>
        CUSTOM = 3
    }
}
