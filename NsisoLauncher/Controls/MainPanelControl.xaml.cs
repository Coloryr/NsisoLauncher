﻿using NsisoLauncher.Color_yr;
using NsisoLauncher.Config;
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
        public event Action<object, LaunchEventArgs> Launch;

        private ObservableCollection<KeyValuePair<string, UserNode>> userList = new ObservableCollection<KeyValuePair<string, UserNode>>();
        private ObservableCollection<KeyValuePair<string, AuthenticationNode>> authNodeList = new ObservableCollection<KeyValuePair<string, AuthenticationNode>>();
        private ObservableCollection<NsisoLauncherCore.Modules.Version> versionList = new ObservableCollection<NsisoLauncherCore.Modules.Version>();

        public MainPanelControl()
        {
            InitializeComponent();
            FirstBinding();
            Refresh();
            //Color_yr add
            APP_Color();
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
            {
                return App.config.MainConfig.User.UserDatabase[userID];
            }
            else
            {
                return null;
            }
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
                //更新用户列表
                userList.Clear();
                foreach (var item in App.config.MainConfig.User.UserDatabase)
                {
                    userList.Add(item);
                }

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
                foreach (var item in App.config.MainConfig.User.AuthenticationDic)
                {
                    authNodeList.Add(item);
                }

                //更新版本列表
                List<NsisoLauncherCore.Modules.Version> versions = await App.handler.GetVersionsAsync();
                versionList.Clear();
                foreach (var item in versions)
                {
                    versionList.Add(item);
                }

                this.launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;
                if ((App.config.MainConfig.History.SelectedUserNodeID != null) && 
                    (App.config.MainConfig.User.UserDatabase.ContainsKey(App.config.MainConfig.History.SelectedUserNodeID)))
                {
                    this.userComboBox.SelectedValue = App.config.MainConfig.History.SelectedUserNodeID;
                    await RefreshIcon();
                }

                //锁定验证模型处理
                if (!string.IsNullOrWhiteSpace(App.config.MainConfig.User.LockAuthName))
                {
                    if (App.config.MainConfig.User.AuthenticationDic.ContainsKey(App.config.MainConfig.User.LockAuthName))
                    {
                        authTypeCombobox.SelectedValue = App.config.MainConfig.User.LockAuthName;
                        authTypeCombobox.IsEnabled = false;
                    }
                }
                else
                {
                    authTypeCombobox.IsEnabled = true;
                }

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
            /*
            UserNode node = GetSelectedAuthNode();
            if (node == null)
            {
                return;
            }
            bool isNeedRefreshIcon = (!string.IsNullOrWhiteSpace(node.SelectProfileUUID)) &&
                node.AuthModule == "mojang";
            if (isNeedRefreshIcon)
            {
                await headScul.RefreshIcon(node.SelectProfileUUID);
            }
            */
            UserNode node = GetSelectedAuthNode();
            AuthenticationNode node1 = GetSelectedAuthenticationNode();
            if (node == null || node1 == null)
            {
                return;
            }
            bool isNeedRefreshIcon = !string.IsNullOrWhiteSpace(node.SelectProfileUUID) &&
                (node.AuthModule == "mojang" || node1.AuthType == AuthenticationType.NIDE8);
            if (isNeedRefreshIcon)
            {
                if (node.AuthModule == "mojang")
                    await headScul.RefreshIcon(node.SelectProfileUUID);
                else if (node1.AuthType == AuthenticationType.NIDE8)
                    await headScul.RefreshIcon_nide8(node.SelectProfileUUID, node1);
            }
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
            UserNode userNode = null;
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


            this.Launch?.Invoke(this, new LaunchEventArgs() { AuthNode = authNode, UserNode = userNode, LaunchVersion = launchVersion, IsNewUser = isNewUser });
        }

        //下载按钮点击
        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            new DownloadWindow().ShowDialog();
            Refresh();
        }

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

        private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshIcon();
        }
        //Color_yr Add Start
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
