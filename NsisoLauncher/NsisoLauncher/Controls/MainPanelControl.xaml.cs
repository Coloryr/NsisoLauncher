using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.Windows;
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
        public bool is_re = false;
        private bool is_use = false;

        public event Action<object, LaunchEventArgs> Launch;

        private ObservableCollection<KeyValuePair<string, UserNode>> UserList { get; set; } = new ObservableCollection<KeyValuePair<string, UserNode>>();
        private ObservableCollection<KeyValuePair<string, AuthenticationNode>> AuthNodeList { get; set; } = new ObservableCollection<KeyValuePair<string, AuthenticationNode>>();
        private ObservableCollection<NsisoLauncherCore.Modules.Version> VersionList { get; set; } = new ObservableCollection<NsisoLauncherCore.Modules.Version>();

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
            if ((userID != null) && App.config.MainConfig.User.UserDatabase.ContainsKey(userID))
                return App.config.MainConfig.User.UserDatabase[userID];
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
                AuthNodeList.Add(new KeyValuePair<string, AuthenticationNode>("offline", new AuthenticationNode()
                {
                    AuthType = AuthenticationType.OFFLINE,
                    Name = App.GetResourceString("String.Mainwindow.Auth.Offline")
                }));
                if (App.config.MainConfig.User.AuthenticationDic != null)
                    foreach (var item in App.config.MainConfig.User.AuthenticationDic)
                    {
                        AuthNodeList.Add(item);
                    }

                //更新用户列表
                UserList.Clear();
                if (App.config.MainConfig.User.UserDatabase != null)
                    foreach (var item in App.config.MainConfig.User.UserDatabase)
                    {
                        UserList.Add(item);
                    }

                //更新版本列表
                List<NsisoLauncherCore.Modules.Version> versions = await App.handler.GetVersionsAsync();
                VersionList.Clear();
                foreach (var item in versions)
                {
                    VersionList.Add(item);
                }

                launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;
                if ((App.config.MainConfig.History.SelectedUserNodeID != null) &&
                    (App.config.MainConfig.User.UserDatabase.ContainsKey(App.config.MainConfig.History.SelectedUserNodeID)))
                {
                    userComboBox.SelectedValue = App.config.MainConfig.History.SelectedUserNodeID;
                    UserComboBox_SelectionChanged(null, null);
                }

                //锁定验证模型处理
                if (!string.IsNullOrWhiteSpace(App.config.MainConfig.User.LockAuthName))
                {
                    if (App.config.MainConfig.User.AuthenticationDic.ContainsKey(App.config.MainConfig.User.LockAuthName))
                    {
                        is_re = true;
                        authTypeCombobox.SelectedValue = App.config.MainConfig.User.LockAuthName;
                        authTypeCombobox.IsEnabled = false;
                        is_re = false;
                        AuthenticationNode node = GetSelectedAuthenticationNode();
                        if (node.AuthType != AuthenticationType.OFFLINE)
                            PasswordBox.Visibility = Visibility.Visible;
                        UserNode node1 = GetSelectedAuthNode();
                        if (node1 != null && node1.AuthModule == "offline")
                            userComboBox.SelectedItem = null;
                        is_use = true;
                    }
                }
                else
                {
                    authTypeCombobox.IsEnabled = true;
                    is_use = false;
                }

                if (userComboBox.SelectedValue != null && string.IsNullOrWhiteSpace(App.config.MainConfig.User.LockAuthName))
                {
                    UserNode node = GetSelectedAuthNode();
                    authTypeCombobox.SelectedValue = node.AuthModule;
                }
                if (is_re == true)
                {
                    if (authTypeCombobox.SelectedItem != null)
                    {

                    }
                    UserNode node = GetSelectedAuthNode();
                    if (node != null)
                        authTypeCombobox.SelectedValue = node.AuthModule;
                    is_re = false;
                }
                if (string.IsNullOrWhiteSpace(App.config.MainConfig.User.LockAuthName) == false)
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
                App.logHandler.AppendDebug("启动器主窗体数据重载完毕");
            }
            catch (Exception e)
            {
                App.CatchAggregateException(this, new AggregateExceptionArgs()
                {
                    AggregateException = new AggregateException("启动器致命错误", e)
                });
            }
        }

        public async Task RefreshIcon()
        {
            //头像自定义显示皮肤
            UserNode node = GetSelectedAuthNode();
            AuthenticationNode node1 = GetSelectedAuthenticationNode();
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
        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            Lock(false);
            //获取启动版本
            NsisoLauncherCore.Modules.Version launchVersion = null;
            if (launchVersionCombobox.SelectedItem != null)
            {
                launchVersion = (NsisoLauncherCore.Modules.Version)launchVersionCombobox.SelectedItem;
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
            if (App.config.MainConfig.User.Nide8ServerDependence)
            {
                AuthenticationNode node = GetSelectedAuthenticationNode();
                if (node == null)
                {
                    await DialogManager.ShowMessageAsync(App.MainWindow_, App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID"),
                                App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID2"));
                    return;
                }
                new Register(string.Format("https://login2.nide8.com:233/{0}/loginreg", node.Property["nide8ID"])).ShowDialog();
            }
            else if (string.IsNullOrWhiteSpace(App.config.MainConfig.User.LockAuthName) == false)
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
                new DownloadWindow(true).ShowDialog();
                Refresh();
            }
        }

        //配置按钮点击
        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow().ShowDialog();
            Refresh();
            ((MainWindow)Window.GetWindow(this)).CustomizeRefresh();
            App.lan();
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
            if (is_re == true)
            {
                return;
            }
            if ((App.config.MainConfig.History.SelectedUserNodeID != null) &&
                    (App.config.MainConfig.User.UserDatabase.ContainsKey(App.config.MainConfig.History.SelectedUserNodeID)))
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
            if (is_re == true)
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
            if (is_use == false)
            {
                authTypeCombobox.IsEnabled = use;
                addauth.IsEnabled = use;
            }
        }
    }

    public class LaunchEventArgs : EventArgs
    {
        public NsisoLauncherCore.Modules.Version LaunchVersion { get; set; }
        public AuthenticationNode AuthNode { get; set; }
        public UserNode UserNode { get; set; }
        public bool IsNewUser { get; set; }
    }

    public enum LaunchType
    {
        NORMAL,
        SAFE,
        CREATE_SHORT
    }
}
