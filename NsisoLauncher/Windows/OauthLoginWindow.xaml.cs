using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Net.MicrosoftLogin;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Collections.Generic;
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
            wb.Source = OAuthFlower.GetAuthorizeUri();
            ShowLoading();
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

        private async void wb_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Uri uri = e.Uri;
            Uri = e.Uri.ToString();
            await NavigatingUriHandle(uri);
        }

        private void wb_Navigated(object sender, NavigationEventArgs e)
        {
            Uri uri = e.Uri;
            if (uri == OAuthFlower.GetAuthorizeUri())
            {
                HideLoading();
            }
        }

        private async Task NavigatingUriHandle(Uri uri)
        {
            if (uri.Host == OAuthFlower.RedirectUri.Host && uri.AbsolutePath == OAuthFlower.RedirectUri.AbsolutePath)
            {
                ShowLoading();
                string code = OAuthFlower.RedirectUrlToAuthCode(uri);
                await Authenticate(code);
            }
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
                    await this.ShowMessageAsync("您的微软账号没有拥有Minecraft正版", "请确认您微软账号中购买了Minecraft正版，并拥有完整游戏权限");
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

        private void ShowLoading()
        {
            wb.Visibility = Visibility.Hidden;
            progress.IsActive = true;
            loadingPanel.Visibility = Visibility.Visible;
        }

        private void HideLoading()
        {
            wb.Visibility = Visibility.Visible;
            progress.IsActive = false;
            loadingPanel.Visibility = Visibility.Collapsed;
        }
    }
}
