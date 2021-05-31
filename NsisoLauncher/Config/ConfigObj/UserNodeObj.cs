using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using System.Collections.Generic;

namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 用户验证节点设置
    /// </summary>
    public class UserNodeObj
    {
        /// <summary>
        /// 所使用的验证模型
        /// </summary>
        public string AuthModule { get; set; }

        /// <summary>
        /// 用户名/账号
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 验证令牌
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 选中的profile UUID
        /// </summary>
        public string SelectProfileUUID { get; set; }

        /// <summary>
        /// 玩家选择的角色UUID
        /// </summary>
        public Dictionary<string, Uuid> Profiles { get; set; } = new();

        /// <summary>
        /// 用户资料
        /// </summary>
        public AuthenticateResponse.UserData UserData { get; set; }

        public Uuid GetSelectProfileUUID()
        {
            return Profiles[SelectProfileUUID];
        }

        public void ClearAuthCache()
        {
            AccessToken = null;
            SelectProfileUUID = null;
        }
    }
}
