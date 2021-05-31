using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using NsisoLauncher.Utils;
using NsisoLauncherCore.Net.MicrosoftLogin;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// OauthLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OauthLoginWindow : MetroWindow
    {
        public OAuthFlow OAuthFlower { get; set; }
        public XboxliveAuth XboxliveAuther { get; set; }
        public MinecraftServices McServices { get; set; }

        public CancellationToken CancelToken { get; set; } = default;

        public static MicrosoftUser LoggedInUser { get; set; }

        private bool loginout;
        private bool islogin;

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

        private async Task Authenticate(string code)
        {
            try
            {
                TaskbarManager.SetProgressState(TaskbarProgressBarState.Indeterminate);

                var result = await OAuthFlower.MicrosoftCodeToAccessToken(code, CancelToken);
                var xbox_result = await XboxliveAuther.Authenticate(result, CancelToken);
                var mc_result = await McServices.Authenticate(xbox_result, CancelToken);
                var owner_result = await McServices.CheckHaveGameOwnership(mc_result, CancelToken);

                if (owner_result)
                {
                    MicrosoftUser microsoftUser = await McServices.GetProfile(result, mc_result, CancelToken);
                    LoggedInUser = microsoftUser;
                    islogin = true;
                    LoginChange();
                }
                else
                {
                    islogin = false;
                    LoggedInUser = null;
                    var res = await this.ShowMessageAsync(App.GetResourceString("String.OauthLogin.NoMinecraft"), 
                        App.GetResourceString("String.OauthLogin.NoMinecraft1"), 
                        MessageDialogStyle.AffirmativeAndNegative, 
                        new MetroDialogSettings()
                    {
                        AffirmativeButtonText = App.GetResourceString("String.OauthLogin.Back"),
                        NegativeButtonText = App.GetResourceString("String.OauthLogin.Change"),
                        DefaultButtonFocus = MessageDialogResult.Negative
                    });
                    if (res == MessageDialogResult.Negative)
                    {
                        LoginOut();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggedInUser = null;
                await this.ShowMessageAsync(App.GetResourceString("String.OauthLogin.LoginError"), ex.ToString());
            }
            finally
            {
                TaskbarManager.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
        }

        private void LoginOut()
        {
            wb.Source = new Uri("https://login.live.com/oauth20_logout.srf?client_id=ca634a9a-f102-4033-b081-3a4393a6f65d&redirect_uri=https%3A%2F%2Fsisu.xboxlive.com%2Fconnect%2Foauth%2FXboxLive%2Fsignout%3Fstate%3Dlogout%26ru%3Dhttps%253A%252F%252Fwww.minecraft.net%252Fzh-hans");
            loginout = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(url.Text))
            {
                await this.ShowMessageAsync(App.GetResourceString("String.OauthLogin.Error1.Title"),
                    App.GetResourceString("String.OauthLogin.Error1.Text"));
                return;
            }
            string code = OAuthFlower.RedirectUrlToAuthCode(new(url.Text));
            var loading = await this.ShowProgressAsync(App.GetResourceString("String.OauthLogin.Logining.Title"),
                    App.GetResourceString("String.OauthLogin.Logining.Text"));
            await Authenticate(code);
            await loading.CloseAsync();
        }

        private async void wb_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Uri uri = e.Uri;
            Uri.Text = e.Uri.ToString();
            if (loginout)
                return;
            await NavigatingUriHandle(uri);
        }

        private void wb_Navigated(object sender, NavigationEventArgs e)
        {
            Uri uri = e.Uri;
            if (uri == OAuthFlower.GetAuthorizeUri())
            {
                HideLoading();
            }
            else if (uri.ToString() == "https://www.minecraft.net/zh-hans")
            {
                wb.Source = OAuthFlower.GetAuthorizeUri();
                loginout = false;
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 || loginout)
                return;
            var item = e.AddedItems[0] as TabItem;
            if (item.Name is "Web")
            {
                var temp = OAuthFlower.GetAuthorizeUri().OriginalString;
                weburl.Text = temp;
                try
                {
                    Process.Start(temp);
                }
                catch
                {

                }
            }
            else if (item.Name is "Self")
            {
                wb.Source = OAuthFlower.GetAuthorizeUri();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LoggedInUser = null;
            islogin = false;
            UserName.Content = UserUUID.Content = "";
            LoginOut();
            LoginChange();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoginChange()
        {
            if (islogin)
            {
                Tabs.SelectedIndex = 2;
                Self.Visibility = Visibility.Collapsed;
                Web.Visibility = Visibility.Collapsed;
                UserInfo.Visibility = Visibility.Visible;
                UserName.Content = LoggedInUser.Name;
                UserUUID.Content = LoggedInUser.LaunchUuid;
            }
            else
            {
                Tabs.SelectedIndex = 0;
                Self.Visibility = Visibility.Visible;
                Web.Visibility = Visibility.Visible;
                UserInfo.Visibility = Visibility.Collapsed;
            }
        }
    }
}
