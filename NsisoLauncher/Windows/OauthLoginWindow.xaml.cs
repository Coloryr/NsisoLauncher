using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using NsisoLauncherCore.Net.MicrosoftLogin;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// OauthLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OauthLoginWindow : MetroWindow
    {
        /// <summary>
        /// The uri of the web view
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// The progress of the auth
        /// </summary>
        public string Progress { get; set; } = "初始化";

        public Visibility WebBrowserVisibility { get; set; }

        public OAuthFlow OAuthFlower { get; set; }
        public XboxliveAuth XboxliveAuther { get; set; }
        public MinecraftServices McServices { get; set; }

        public CancellationToken CancelToken { get; set; } = default;

        public static MicrosoftUser LoggedInUser { get; set; }

        public OauthLoginWindow()
        {
            InitializeComponent();
            DataContext = this;

            OAuthFlower = new OAuthFlow();
            XboxliveAuther = new XboxliveAuth();
            McServices = new MinecraftServices();

        }

        public void ShowLogin()
        {
            var temp = OAuthFlower.GetAuthorizeUri().OriginalString;
            //从注册表中读取默认浏览器可执行文件路径
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
            string s = key.GetValue("").ToString();

            //s就是你的默认浏览器，不过后面带了参数，把它截去，不过需要注意的是：不同的浏览器后面的参数不一样！
            //"D:\Program Files (x86)\Google\Chrome\Application\chrome.exe" -- "%1"
            System.Diagnostics.Process.Start(s.Substring(0, s.Length - 8), temp);
            ShowDialog();
        }

        public async Task<MinecraftToken> RefreshMinecraftToken(MicrosoftToken token)
        {
            Progress = "MicrosoftCodeToAccessToken";
            var result = await OAuthFlower.RefreshMicrosoftAccessToken(token, CancelToken);

            Progress = "XboxliveAuther.Authenticate";
            var xbox_result = await XboxliveAuther.Authenticate(result, CancelToken);

            Progress = "McServices.Authenticate";
            var mc_result = await McServices.Authenticate(xbox_result, CancelToken);

            return mc_result;

        }

        private async Task Authenticate(string code)
        {
            try
            {
                Progress = "MicrosoftCodeToAccessToken";
                var result = await OAuthFlower.MicrosoftCodeToAccessToken(code, CancelToken);

                Progress = "XboxliveAuther.Authenticate";
                var xbox_result = await XboxliveAuther.Authenticate(result, CancelToken);

                Progress = "McServices.Authenticate";
                var mc_result = await McServices.Authenticate(xbox_result, CancelToken);

                Progress = "McServices.CheckHaveGameOwnership";
                var owner_result = await McServices.CheckHaveGameOwnership(mc_result, CancelToken);

                if (owner_result)
                {
                    Progress = "McServices.GetProfile";
                    MicrosoftUser microsoftUser = await McServices.GetProfile(result, mc_result, CancelToken);
                    LoggedInUser = microsoftUser;
                    Close();
                }
                else
                {

                    LoggedInUser = null;
                    await this.ShowMessageAsync("您的微软账号没有拥有Minecraft正版", "请确认您微软账号中购买了Minecraft正版，并拥有完整游戏权限");
                    Close();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("登录发生异常", ex.ToString());
            }
        }

        private async Task<MinecraftToken> Refresh(MicrosoftToken token)
        {
            Progress = "MicrosoftCodeToAccessToken";
            var result = await OAuthFlower.RefreshMicrosoftAccessToken(token, CancelToken);

            Progress = "XboxliveAuther.Authenticate";
            var xbox_result = await XboxliveAuther.Authenticate(result, CancelToken);

            Progress = "McServices.Authenticate";
            var mc_result = await McServices.Authenticate(xbox_result, CancelToken);

            return mc_result;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string code = OAuthFlower.RedirectUrlToAuthCode(new(url.Text));
            await Authenticate(code);
        }
    }
}
