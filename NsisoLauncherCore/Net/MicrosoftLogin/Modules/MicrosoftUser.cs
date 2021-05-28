namespace NsisoLauncherCore.Net.MicrosoftLogin.Modules
{
    public class MicrosoftUser
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 微软token
        /// </summary>
        public MicrosoftToken MicrosoftToken { get; set; }

        /// <summary>
        /// 游戏验证token
        /// </summary>
        public MinecraftToken MinecraftToken { get; set; }

        public string LaunchAccessToken => MinecraftToken.AccessToken;

        public string LaunchUuid => Id;

        public string LaunchPlayerName => Name;

        public string UserType => "msa";

        public string UserId => MicrosoftToken.User_id;
    }
}
