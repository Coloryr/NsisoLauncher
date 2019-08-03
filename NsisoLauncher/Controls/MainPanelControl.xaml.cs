using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Color_yr;
using NsisoLauncher.Config;
using NsisoLauncher;
using NsisoLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        //Color_yr Add
        public bool is_re = false;

        public event Action<object, LaunchEventArgs> Launch;

        private ObservableCollection<KeyValuePair<string, UserNode>> userList = new ObservableCollection<KeyValuePair<string, UserNode>>();
        private ObservableCollection<KeyValuePair<string, AuthenticationNode>> authNodeList = new ObservableCollection<KeyValuePair<string, AuthenticationNode>>();
        private ObservableCollection<NsisoLauncherCore.Modules.Version> versionList = new ObservableCollection<NsisoLauncherCore.Modules.Version>();

        public MainPanelControl()
        {
            InitializeComponent();
            FirstBinding();
            Refresh();
            //Color_yr Add Start
            APP_Color();
            //Color_yr Add Stop
        }

        private void FirstBinding()
        {
            authTypeCombobox.ItemsSource = authNodeList;
            userComboBox.ItemsSource = userList;
            launchVersionCombobox.ItemsSource = versionList;
        }

        private UserNode GetSelectedAuthNode()
        {
            string userID = (string)userComboBox.SelectedValue;
            if ((userID != null) && App.config.MainConfig.User.UserDatabase.ContainsKey(userID))
                return App.config.MainConfig.User.UserDatabase[userID];
            return null;
        }
        //Color_yr Add Start
        private AuthenticationNode GetSelectedAuthenticationNode()
        {
            if (authTypeCombobox.SelectedItem != null)
            {
                KeyValuePair<string, AuthenticationNode> node = (KeyValuePair<string, AuthenticationNode>)authTypeCombobox.SelectedItem;
                return node.Value;
            }
            return null;
        }
        //Color_yr Add Stop
        public async void Refresh()
        {
            try
            {
                //更新验证模型列表
                authNodeList.Clear();
                authNodeList.Add(new KeyValuePair<string, AuthenticationNode>("offline", new AuthenticationNode()
                {
                    AuthType = AuthenticationType.OFFLINE,
                    Name = App.GetResourceString("String.MainWindow.Auth.Offline")
                }));
                authNodeList.Add(new KeyValuePair<string, AuthenticationNode>("mojang", new AuthenticationNode()
                {
                    AuthType = AuthenticationType.MOJANG,
                    Name = App.GetResourceString("String.MainWindow.Auth.Mojang")
                }));
                if (App.config.MainConfig.User.AuthenticationDic != null)
                    foreach (var item in App.config.MainConfig.User.AuthenticationDic)
                    {
                        authNodeList.Add(item);
                    }

                //更新用户列表
                userList.Clear();
                if (App.config.MainConfig.User.UserDatabase != null)
                    foreach (var item in App.config.MainConfig.User.UserDatabase)
                    {
                        userList.Add(item);
                    }

                //更新版本列表
                List<NsisoLauncherCore.Modules.Version> versions = await App.handler.GetVersionsAsync();
                versionList.Clear();
                foreach (var item in versions)
                {
                    versionList.Add(item);
                }

                launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;
                if ((App.config.MainConfig.History.SelectedUserNodeID != null) &&
                    (App.config.MainConfig.User.UserDatabase.ContainsKey(App.config.MainConfig.History.SelectedUserNodeID)))
                {
                    userComboBox.SelectedValue = App.config.MainConfig.History.SelectedUserNodeID;
                    UserNode node = GetSelectedAuthNode();
                    if (node.AuthModule == "offline")
                        userComboBox.SelectedItem = null;
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
                    }
                }
                else
                    authTypeCombobox.IsEnabled = true;

                //Color_yr Add Start
                if (is_re == true)
                {
                    UserNode node = GetSelectedAuthNode();
                    if (node != null)
                        authTypeCombobox.SelectedValue = node.AuthModule;
                    is_re = false;
                }
                if (App.config.MainConfig.User.Nide8ServerDependence)
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
                //Color_yr Add Stop
                App.logHandler.AppendDebug("启动器主窗体数据重载完毕");
            }
            catch (Exception e)
            {
                App.CatchAggregateException(this, new AggregateExceptionArgs() { AggregateException = new AggregateException("启动器致命错误", e) });
            }
        }

        public async Task RefreshIcon()
        {
            //头像自定义显示皮肤
            //Color_yr Add Start
            UserNode node = GetSelectedAuthNode();
            AuthenticationNode node1 = GetSelectedAuthenticationNode();
            if (node == null || node1 == null)
                return;
            bool isNeedRefreshIcon = !string.IsNullOrWhiteSpace(node.SelectProfileUUID) &&
                (node.AuthModule == "mojang" || node1.AuthType == AuthenticationType.NIDE8);
            if (isNeedRefreshIcon)
            {
                if (node.AuthModule == "mojang")
                    await headScul.RefreshIcon(node.SelectProfileUUID);
                else if (node1.AuthType == AuthenticationType.NIDE8)
                    await headScul.RefreshIcon_nide8(node.SelectProfileUUID, node1);
            }
            //Color_yr Add Stop
        }

        #region button click event

        //启动游戏按钮点击
        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
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
                if (!isNewUser)
                {
                    userNode = ((KeyValuePair<string, UserNode>)userComboBox.SelectedItem).Value;
                }
                else
                {
                    userNode = new UserNode() { AuthModule = authNodeName, UserName = userName };
                }
            }
            else
            {
                userNode = null;
            }


            Launch?.Invoke(this, new LaunchEventArgs() { AuthNode = authNode, UserNode = userNode, LaunchVersion = launchVersion, IsNewUser = isNewUser });
        }

        //下载按钮点击
        //Color_yr Add Start
        private async void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.config.MainConfig.User.Nide8ServerDependence)
            {
                AuthenticationNode node = GetSelectedAuthenticationNode();
                if (node == null)
                {
                    await DialogManager.ShowMessageAsync(null, App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID"),
                                App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID2"));
                    return;
                }
                System.Diagnostics.Process.Start(string.Format("https://login2.nide8.com:233/{0}/loginreg", node.Property["nide8ID"]));
            }
            else
            {
                new DownloadWindow().ShowDialog();
                Refresh();
            }
        }
        //Color_yr Add Stop

        //配置按钮点击
        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow().ShowDialog();
            Refresh();
            ((MainWindow)Window.GetWindow(this)).CustomizeRefresh();
            //Color_yr Add
            APP_Color();
        }
        #endregion

        private void addAuthButton_Click(object sender, RoutedEventArgs e)
        {
            var a = new SettingWindow();
            a.ShowAddAuthModule();
            a.ShowDialog();
            Refresh();
        }
        //Color_yr Add Start
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
            Brush_Color get = new Brush_Color();
            Brush b = get.get_Bursh();
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
        //Color_yr Add Stop
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
