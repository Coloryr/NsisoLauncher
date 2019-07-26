using NsisoLauncher.Config;
using NsisoLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Color_custom();
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
            if (App.config.MainConfig.User.UserDatabase.ContainsKey(userID))
            {
                return App.config.MainConfig.User.UserDatabase[userID];
            }
            else
            {
                return null;
            }
        }

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
                /*
                //全局统一验证设置
                bool isAllUsingNide8 = (App.nide8Handler != null) && App.config.MainConfig.User.AllUsingNide8;
                if (isAllUsingNide8)
                {
                    downloadButton.Content = App.GetResourceString("String.Base.Register");
                }
                else
                {
                    downloadButton.Content = App.GetResourceString("String.Base.Download");
                }

                launchVersionCombobox.ItemsSource = await App.handler.GetVersionsAsync();
                this.launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;
                */
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
                this.userComboBox.SelectedValue = App.config.MainConfig.History.SelectedUserNodeID;
                if ((App.config.MainConfig.History.SelectedUserNodeID != null) &&
                    (App.config.MainConfig.User.UserDatabase.ContainsKey(App.config.MainConfig.History.SelectedUserNodeID)))
                {
                    this.userComboBox.SelectedValue = App.config.MainConfig.History.SelectedUserNodeID;
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

        public async void RefreshIcon()
        {
            /*
            //头像自定义显示皮肤
            AuthenticationNode node = GetSelectedAuthNode();
            if (node == null)
            {
                return;
            }
            bool isNeedRefreshIcon = (!string.IsNullOrWhiteSpace(node.SelectProfileUUID)) &&
                (node.AuthenticationType == AuthenticationType.MOJANG
                || node.AuthenticationType == AuthenticationType.NIDE8);
            if (isNeedRefreshIcon)
            {
                switch (node.AuthenticationType)
                {
                    case AuthenticationType.MOJANG:
                        await headScul.RefreshIcon(node.SelectProfileUUID);
                        break;
                    case AuthenticationType.NIDE8:
                        await headScul.RefreshIcon_nide8(node.SelectProfileUUID);
                        break;
                }
            }
            */
            //头像自定义显示皮肤
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
        }

        private void Color_custom()
        {
            /*
            switch (App.config.MainConfig.User.AuthenticationType)
            {
                case Config.AuthenticationType.OFFLINE:
                    playerPassTextBox.Visibility = Visibility.Hidden;
                    break;
                default:
                    playerPassTextBox.Visibility = Visibility.Visible;
                    break;
            }
            */
            Brush b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AF3F5966"));
            switch (App.config.MainConfig.Customize.AccentColor)
            {
                case "Red":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CBE1707"));
                    break;
                case "Green":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C548E19"));
                    break;
                case "Blue":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C1585B5"));
                    break;
                case "Purple":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C574EB9"));
                    break;
                case "Orange":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CCF5A07"));
                    break;
                case "Lime":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C8AA407"));
                    break;
                case "Emerald":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C077507"));
                    break;
                case "Teal":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C07908E"));
                    break;
                case "Cyan":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C1D88BC"));
                    break;
                case "Cobalt":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C0747C6"));
                    break;
                case "Indigo":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C5C07D3"));
                    break;
                case "Violet":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C8F07D3"));
                    break;
                case "Pink":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CCA62AD"));
                    break;
                case "Magenta":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CB40763"));
                    break;
                case "Crimson":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C890725"));
                    break;
                case "Amber":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CC7890F"));
                    break;
                case "Yellow":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CD2B90C"));
                    break;
                case "Browns":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C6F4F2A"));
                    break;
                case "Olive":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C5E7357"));
                    break;
                case "Steel":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C576573"));
                    break;
                case "mauve":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C655475"));
                    break;
                case "Taupe":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C736845"));
                    break;
                case "Sienna":
                    b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C87492B"));
                    break;
            }
            if (b != null)
            {
                authTypeCombobox.Background = b;
                launchVersionCombobox.Background = b;
                userComboBox.Background = b;
                //playerPassTextBox.Background = b;
                configButton.Background = b;
                downloadButton.Background = b;
                launchButton.Background = b;
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

        //下载按钮点击
        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (fastlogin == true)
            {
                System.Diagnostics.Process.Start(string.Format("https://login2.nide8.com:233/{0}/register", args.AuthNode.Property["nide8ID"]);
                return;
            }
            */
            //else
            //{
            new DownloadWindow().ShowDialog();
            Refresh();
            //}
        }

        //配置按钮点击
        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow().ShowDialog();
            Refresh();
            ((MainWindow)Window.GetWindow(this)).CustomizeRefresh();
            Color_custom();
        }
        #endregion
        private void addAuthButton_Click(object sender, RoutedEventArgs e)
        {
            var a = new SettingWindow();
            a.ShowAddAuthModule();
            a.ShowDialog();
            Refresh();
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