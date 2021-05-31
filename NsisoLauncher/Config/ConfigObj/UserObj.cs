using System.Collections.Generic;

namespace NsisoLauncher.Config.ConfigObj
{
    /// <summary>
    /// 用户基本设置
    /// </summary>
    public class UserObj
    {
        /// <summary>
        /// 用户数据库
        /// </summary>
        public Dictionary<string, UserNodeObj> UserDatabase { get; set; }

        /// <summary>
        /// 验证节点
        /// </summary>
        public Dictionary<string, AuthenticationNodeObj> AuthenticationDic { get; set; }

        /// <summary>
        /// 用户端Token
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// 锁定全局验证
        /// </summary>
        public string LockAuthName { get; set; }

        /// <summary>
        /// 全局是否对NIDE8服务器依赖
        /// </summary>
        public bool Nide8ServerDependence { get; set; }

        /// <summary>
        /// 获取锁定验证模型，若不存在返回NULL
        /// </summary>
        /// <returns>锁定的验证模型</returns>
        public AuthenticationNodeObj GetLockAuthNode()
        {
            if ((!string.IsNullOrWhiteSpace(LockAuthName)) && AuthenticationDic.ContainsKey(LockAuthName))
            {
                return AuthenticationDic[LockAuthName];
            }
            else
            {
                return null;
            }
        }
    }
}
