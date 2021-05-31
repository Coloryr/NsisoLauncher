using System.Collections.Generic;

namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 验证节点设置
    /// </summary>
    public record AuthenticationNodeObj
    {
        public string Name { get; set; }

        public AuthenticationType AuthType { get; set; }

        /// <summary>
        /// authserver:验证服务器地址
        /// nide8ID:NIDE8的验证ID
        /// </summary>
        public Dictionary<string, string> Property { get; set; } = new();

        /// <summary>
        /// 注册的地址
        /// </summary>
        public string RegisteAddress { get; set; }

        /// <summary>
        /// 皮肤网址
        /// </summary>
        public string SkinUrl { get; set; }

        /// <summary>
        /// 使用内部浏览器
        /// </summary>
        public bool UseSelfBrowser { get; set; }

        /// <summary>
        /// 获取头像的方式
        /// </summary>
        public HeadType HeadType { get; set; } = HeadType.JSON;
    }
}
