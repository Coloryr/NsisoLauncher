using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.Windows;
using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NsisoLauncher.Controls
{
    /// <summary>
    /// MainPanelControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainPanelControl : UserControl
    {
        public bool isRes = false;
        private bool isUse = false;

        public event Action<object, LaunchEventArgs> Launch;

        private ObservableCollection<KeyValuePair<string, UserNode>> UserList { get; set; } = new ObservableCollection<KeyValuePair<string, UserNode>>();
        private ObservableCollection<KeyValuePair<string, AuthenticationNode>> AuthNodeList { get; set; } = new ObservableCollection<KeyValuePair<string, AuthenticationNode>>();
        private ObservableCollection<MCVersion> VersionList { get; set; } = new ObservableCollection<MCVersion>();

        public MainPanelControl()
        {
            InitializeComponent();
            FirstBinding();
            Refresh();
            APP_Color();
            DataContext = this;
        }

        private void FirstBinding()
        {
            authTypeCombobox.ItemsSource = AuthNodeList;
            userComboBox.ItemsSource = UserList;
            launchVersionCombobox.ItemsSource = VersionList;
        }

        private UserNode GetSelectedAuthNode()
        {
            string userID = (string)userComboBox.SelectedValue;
            if ((userID != null) && App.Config.MainConfig.User.UserDatabase.ContainsKey(userID))
                return App.Config.MainConfig.User.UserDatabase[userID];
            return null;
        }
        private AuthenticationNode GetSelectedAuthenticationNode()
        {
            if (authTypeCombobox.SelectedItem != null)
            {
                KeyValuePair<string, AuthenticationNode> node = (KeyValuePair<string, AuthenticationNode>)authTypeCombobox.SelectedItem;
                return node.Value;
            }
            return null;
        }
        public async void Refresh()
        {
            try
            {
                //更新验证模型列表
                AuthNodeList.Clear();
                if (App.Config.MainConfig.User.AuthenticationDic != null)
                    foreach (var item in App.Config.MainConfig.User.AuthenticationDic)
                    {
                        switch (item.Key)
                        {
                            case "offline":
                                item.Value.Name = App.GetResourceString("String.MainPanelControl.Offline");
                                break;
                            case "online":
                                item.Value.Name = App.GetResourceString("String.MainPanelControl.Online");
                                break;
                        }
                        AuthNodeList.Add(item);
                    }

                //更新用户列表
                UserList.Clear();
                if (App.Config.MainConfig.User.UserDatabase != null)
                    foreach (var item in App.Config.MainConfig.User.UserDatabase)
                    {
                        UserList.Add(item);
                    }

                //更新版本列表
                List<MCVersion> versions = await App.Handler.GetVersionsAsync();
                VersionList.Clear();
                foreach (var item in versions)
                {
                    VersionList.Add(item);
                }

                launchVersionCombobox.Text = App.Config.MainConfig.History.LastLaunchVersion;
                if ((App.Config.MainConfig.History.SelectedUserNodeID != null) &&
                    (App.Config.MainConfig.User.UserDatabase.ContainsKey(App.Config.MainConfig.History.SelectedUserNodeID)))
                {
                    userComboBox.SelectedValue = App.Config.MainConfig.History.SelectedUserNodeID;
                    UserComboBox_SelectionChanged(null, null);
                }

                //锁定验证模型处理
                if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.User.LockAuthName))
                {
                    if (App.Config.MainConfig.User.AuthenticationDic.ContainsKey(App.Config.MainConfig.User.LockAuthName))
                    {
                        isRes = true;
                        authTypeCombobox.SelectedValue = App.Config.MainConfig.User.LockAuthName;
                        authTypeCombobox.IsEnabled = false;
                        isRes = false;
                        AuthenticationNode node = GetSelectedAuthenticationNode();
                        if (node.AuthType != AuthenticationType.OFFLINE)
                            PasswordBox.Visibility = Visibility.Visible;
                        UserNode node1 = GetSelectedAuthNode();
                        if (node1 != null && node1.AuthModule == "offline")
                            userComboBox.SelectedItem = null;
                        isUse = true;
                    }
                }
                else
                {
                    authTypeCombobox.IsEnabled = true;
                    isUse = false;
                }

                if (userComboBox.SelectedValue != null && string.IsNullOrWhiteSpace(App.Config.MainConfig.User.LockAuthName))
                {
                    UserNode node = GetSelectedAuthNode();
                    authTypeCombobox.SelectedValue = node.AuthModule;
                }
                if (isRes == true)
                {
                    UserNode node = GetSelectedAuthNode();
                    if (node != null)
                        authTypeCombobox.SelectedValue = node.AuthModule;
                    isRes = false;
                }
                if (string.IsNullOrWhiteSpace(App.Config.MainConfig.User.LockAuthName) == false)
                {
                    downloadButton.Content = App.GetResourceString("String.Base.Register");
                    addauth.Visibility = Visibility.Hidden;
                    authTypeCombobox.Margin = new Thickness(10, 151, 10, 0);
                }
                else
                {
                    downloadButton.Content = App.GetResourceString("String.Base.Download");
                    addauth.Visibility = Visibility.Visible;
                    authTypeCombobox.Margin = new Thickness(10, 151, 50, 0);
                }
                await RefreshIcon();
                App.LogHandler.AppendDebug("启动器主窗体数据重载完毕");
            }
            catch (Exception e)
            {
                App.CatchAggregateException(this, new AggregateExceptionArgs()
                {
                    AggregateException = new AggregateException(
                        App.GetResourceString("String.MainPanelControl.Error"), e)
                });
            }
        }

        public async Task RefreshIcon()
        {
            //头像自定义显示皮肤
            var node = GetSelectedAuthNode();
            var node1 = GetSelectedAuthenticationNode();
            if (node == null || node1 == null)
                return;
            bool isNeedRefreshIcon = !string.IsNullOrWhiteSpace(node.SelectProfileUUID) &&
                (node.AuthModule == "mojang" || node1.AuthType == AuthenticationType.NIDE8);
            if (isNeedRefreshIcon)
            {
                if (node.AuthModule == "mojang")
                    await headScul.RefreshIcon_online(node.SelectProfileUUID);
                else if (node1.AuthType == AuthenticationType.NIDE8)
                    await headScul.RefreshIcon_nide8(node.SelectProfileUUID, node1);
            }
        }

        //启动游戏按钮点击
        public void launchButton_Click(object sender, RoutedEventArgs e)
        {
            Lock(false);
            //获取启动版本
            NsisoLauncherCore.Modules.MCVersion launchVersion = null;
            if (launchVersionCombobox.SelectedItem != null)
            {
                launchVersion = (MCVersion)launchVersionCombobox.SelectedItem;
            }

            //获取验证方式
            AuthenticationNode authNode = null;
            string authNodeName = null;
            if (authTypeCombobox.SelectedItem != null)
            {
                KeyValuePair<string, AuthenticationNode> node = (KeyValuePair<string, AuthenticationNode>)authTypeCombobox.SelectedItem;
                authNode = node.Value;
                authNodeName = node.Key;
            }

            //获取用户信息
            string userName = userComboBox.Text;
            string selectedUserUUID = (string)userComboBox.SelectedValue;
            bool isNewUser = string.IsNullOrWhiteSpace(selectedUserUUID);
            UserNode userNode;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                userNode = isNewUser ? new UserNode() { AuthModule = authNodeName, UserName = userName } : ((KeyValuePair<string, UserNode>)userComboBox.SelectedItem).Value;
            }
            else
            {
                userNode = null;
            }

            Launch?.Invoke(this, new LaunchEventArgs() { AuthNode = authNode, UserNode = userNode, LaunchVersion = launchVersion, IsNewUser = isNewUser });
        }

        //下载按钮点击
        private async void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.Config.MainConfig.User.Nide8ServerDependence)
            {
                AuthenticationNode node = GetSelectedAuthenticationNode();
                if (node == null)
                {
                    await DialogManager.ShowMessageAsync(App.MainWindow_,
                        App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID"),
                        App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID2"));
                    return;
                }
                new Register(string.Format("https://login2.nide8.com:233/{0}/loginreg", node.Property["nide8ID"])).ShowDialog();
            }
            else if (string.IsNullOrWhiteSpace(App.Config.MainConfig.User.LockAuthName) == false)
            {
                AuthenticationNode node = GetSelectedAuthenticationNode();
                if (string.IsNullOrWhiteSpace(node.RegisteAddress) == true)
                {
                    await DialogManager.ShowMessageAsync(App.MainWindow_, App.GetResourceString("String.Mainwindow.Auth.Registe.NoID"),
                                App.GetResourceString("String.Mainwindow.Auth.Registe.NoID_t"));
                    return;
                }
                if (node.UseSelfBrowser)
                    new Register(node.RegisteAddress).ShowDialog();
                else
                    Process.Start(node.RegisteAddress);
            }
            else
            {
                if (App.DownloadWindow_ == null)
                {
                    App.DownloadWindow_ = new DownloadWindow(true);
                    App.DownloadWindow_.Show();
                }
                else
                {
                    App.DownloadWindow_.Activate();
                }
            }
        }

        //配置按钮点击
        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow().ShowDialog();
            Refresh();
            ((MainWindow)Window.GetWindow(this)).CustomizeRefresh();
            App.Lauguage();
            APP_Color();
        }

        private void addAuthButton_Click(object sender, RoutedEventArgs e)
        {
            var a = new SettingWindow();
            a.ShowAddAuthModule();
            a.ShowDialog();
            Refresh();
        }
        private async void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserNode node = GetSelectedAuthNode();
            AuthenticationNode node1 = GetSelectedAuthenticationNode();
            if (isRes == true)
            {
                return;
            }
            if ((App.Config.MainConfig.History.SelectedUserNodeID != null) &&
                    (App.Config.MainConfig.User.UserDatabase.ContainsKey(App.Config.MainConfig.History.SelectedUserNodeID)))
            {
                if (node1 != null && node1.AuthType != AuthenticationType.OFFLINE && node != null && node.AuthModule == "offline")
                {
                    userComboBox.SelectedItem = null;
                    return;
                }
            }
            if (userComboBox.SelectedValue == null)
                PasswordBox.Password = null;
            if (node == null)
                return;
            if (node.AuthModule != "offline" && node.SelectProfileUUID != null && string.IsNullOrWhiteSpace(PasswordBox.Password) == false)
                PasswordBox.Password = "11111111111";
            else
            {
                if (node.AuthModule != "offline")
                    PasswordBox.Password = "11111111111";
                authTypeCombobox.SelectedValue = node.AuthModule;
            }
            await RefreshIcon();
        }
        public void APP_Color()
        {
            BrushColor get = new BrushColor();
            Brush b = get.GetBursh();
            if (b != null)
            {
                authTypeCombobox.Background = b;
                launchVersionCombobox.Background = b;
                userComboBox.Background = b;
                addauth.Background = b;
                configButton.Background = b;
                downloadButton.Background = b;
                launchButton.Background = b;
                PasswordBox.Background = b;
            }
        }
        private void AuthTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isRes == true)
                return;
            AuthenticationNode node = GetSelectedAuthenticationNode();
            UserNode node1 = GetSelectedAuthNode();
            if (node == null)
            {
                PasswordBox.Visibility = Visibility.Hidden;
                PasswordBox.Password = null;
                return;
            }
            if (node1 == null)
                switch (node.AuthType)
                {
                    case AuthenticationType.OFFLINE: //离线模式
                        PasswordBox.Visibility = Visibility.Hidden;
                        PasswordBox.Password = null;
                        break;
                    default: //非离线模式
                        PasswordBox.Visibility = Visibility.Visible;
                        PasswordBox.Password = null;
                        break;
                }
            else
                switch (node.AuthType)
                {
                    case AuthenticationType.OFFLINE: //离线模式
                        PasswordBox.Visibility = Visibility.Hidden;
                        PasswordBox.Password = null;
                        break;
                    default: //非离线模式
                        PasswordBox.Visibility = Visibility.Visible;
                        if (node1 != null && node1.AuthModule == "offline")
                            PasswordBox.Password = null;
                        else if (node1.SelectProfileUUID != null && string.IsNullOrWhiteSpace(PasswordBox.Password) == true)
                            PasswordBox.Password = "11111111111";
                        else if (string.IsNullOrWhiteSpace(PasswordBox.Password) == false && userComboBox.SelectedValue == null)
                            PasswordBox.Password = null;
                        break;
                }
        }
        public void Lock(bool use)
        {
            launchVersionCombobox.IsEnabled = userComboBox.IsEnabled =
                configButton.IsEnabled = downloadButton.IsEnabled =
                launchButton.IsEnabled = PasswordBox.IsEnabled = use;
            if (isUse == false)
            {
                authTypeCombobox.IsEnabled = use;
                addauth.IsEnabled = use;
            }
        }
    }

    public class LaunchEventArgs : EventArgs
    {
        public MCVersion LaunchVersion { get; set; }
        public AuthenticationNode AuthNode { get; set; }
        public UserNode UserNode { get; set; }
        public bool IsNewUser { get; set; }
    }
}
