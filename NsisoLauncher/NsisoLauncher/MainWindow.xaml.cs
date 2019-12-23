﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using NsisoLauncher.Color_yr;
using NsisoLauncher.Color_yr.updata;
using NsisoLauncher.Config;
using NsisoLauncher.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NsisoLauncher
{
    public class AuthTypeItem
    {
        public Config.AuthenticationType Type { get; set; }
        public string Name { get; set; }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //Color_yr Add Start
        public string[] pic_file;
        public int filesnow = 0;
        public bool have_mp4 = false;
        public bool pic_go = false;
        public string[] mp3_file;
        public string[] mp4_file;
        public BitmapImage now_img;
        int now = 0;
        //Color_yr Add Stop

        //TODO:增加取消启动按钮
        public MainWindow()
        {
            InitializeComponent();
            App.logHandler.AppendDebug("启动器主窗体已载入");
            mainPanel.Launch += MainPanel_Launch;
            App.handler.GameExit += Handler_GameExit;
            //Color_yr Add
            BG.Width = 720;
            BG.Height = 405;
            App.MainWindow_ = this;
            CustomizeRefresh();
        }
        //Color_yr Add Start
        public void Pic_cyclic()
        {
            if (App.config.MainConfig.Customize.CustomBackGroundPicture_Cyclic && pic_go == false
                && pic_file.Length > 1)
            {
                Task.Factory.StartNew(() =>
                {
                    pic_go = true;
                    while (true)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            BG.Source = null;
                            now_img = new BitmapImage(new Uri(pic_file[filesnow]));
                            BG.Source = now_img;
                            now_img = null;
                        }));
                        GC.Collect();
                        filesnow++;
                        if (filesnow >= pic_file.Length)
                            filesnow = 0;
                        Thread.Sleep(App.config.MainConfig.Customize.CustomBackGroundPicture_Cyclic_time);
                    }
                });
            }
            else
                Dispatcher.Invoke(new Action(() =>
                {
                    now_img = new BitmapImage(new Uri(pic_file[filesnow]));
                    BG.Source = now_img;
                    now_img = null;
                }));
        }
        public void Mp4_cyclic()
        {
            try
            {
                mediaElement.Stop();
                now = 0;
                mediaElement.Source = new Uri(mp4_file[now]);
                volumeButton.Visibility = Visibility.Visible;
                mediaElement.Visibility = Visibility.Visible;
                mediaElement.Play();
                mediaElement.Volume = (double) App.config.MainConfig.Customize.CustomBackGroundSound / 100;
            }
            catch (Exception) { }
        }
        public void Mp3_cyclic()
        {
            try
            {
                mediaElement.Stop();
                now = 0;
                mediaElement.Source = new Uri(mp3_file[now]);
                volumeButton.Visibility = Visibility.Visible;
                mediaElement.Visibility = Visibility.Visible;
                mediaElement.Play();
                mediaElement.Volume = (double)App.config.MainConfig.Customize.CustomBackGroundSound / 100;
            }
            catch (Exception) { }
        }
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            if (have_mp4 == true && App.config.MainConfig.Customize.CustomBackGroundViode_Cyclic == true)
            {
                now++;
                if (now >= mp4_file.Length)
                    now = 0;
                mediaElement.Source = new Uri(mp4_file[now]);
            }
            else if(App.config.MainConfig.Customize.CustomBackGroundMusic_Cyclic == true)
            {
                now++;
                if (now >= mp3_file.Length)
                    now = 0;
                mediaElement.Source = new Uri(mp3_file[now]);
            }
            mediaElement.Volume = (double)App.config.MainConfig.Customize.CustomBackGroundSound / 100;
            mediaElement.Play();
        }
        //Color_yr Add Stop
        private async void MainPanel_Launch(object sender, Controls.LaunchEventArgs obj)
        {
            await LaunchGameFromArgs(obj);
        }

        #region 启动核心事件处理
        public void Handler_GameExit(object sender, GameExitArg arg)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.WindowState = WindowState.Normal;
                if (!arg.IsNormalExit())
                {
                    this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.GameExit.Error"),
                        string.Format(App.GetResourceString("String.Mainwindow.GameExit.Error_t"),
                        arg.ExitCode, arg.Duration));
                }
            }));
        }
        #endregion

        //Color_yr Add Start
        #region 自定义
        public async void CustomizeRefresh()
        {
            if (!string.IsNullOrWhiteSpace(App.config.MainConfig.Customize.LauncherTitle))
            {
                this.Title = App.config.MainConfig.Customize.LauncherTitle;
            }
            if (App.config.MainConfig.Customize.CustomBackGroundViode)
            {
                mp4_file = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "*.mp4");
                if (mp4_file.Length != 0)
                {
                    have_mp4 = true;
                    Mp4_cyclic();
                }
                else
                {
                    have_mp4 = false;
                    volumeButton.Visibility = Visibility.Hidden;
                    mediaElement.Visibility = Visibility.Hidden;
                    mediaElement.Stop();
                }
            }
            if (App.config.MainConfig.Customize.CustomBackGroundPicture && have_mp4 == false)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "*.png");
                string[] files1 = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "*.jpg");
                string[] icon = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "icon.ico");
                if (icon.Length != 0)
                {
                    this.Icon = new BitmapImage(new Uri(icon[0]));
                }
                if (files.Count() + files1.Count() != 0)
                {
                    pic_file = new string[files.Length + files1.Length];
                    int i = 0;
                    foreach (string a in files)
                    {
                        pic_file[i] = a;
                        i++;
                    }
                    foreach (string a in files1)
                    {
                        pic_file[i] = a;
                        i++;
                    }
                    Pic_cyclic();
                }
            }
            if (App.config.MainConfig.Customize.CustomBackGroundMusic && have_mp4 == false)
            {
                mp3_file = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "*.mp3");
                if (mp3_file.Length != 0)
                    Mp3_cyclic();
                else
                {
                    volumeButton.Visibility = Visibility.Hidden;
                    mediaElement.Visibility = Visibility.Hidden;
                    mediaElement.Stop();
                }
            }
            if (App.config.MainConfig.User.Nide8ServerDependence)
            {
                try
                {
                    var lockAuthNode = App.config.MainConfig.User.GetLockAuthNode();
                    if ((lockAuthNode != null) &&
                        (lockAuthNode.AuthType == AuthenticationType.NIDE8))
                    {
                        Config.Server nide8Server = new Config.Server() { ShowServerInfo = true };
                        var nide8ReturnResult = await (new NsisoLauncherCore.Net.Nide8API.APIHandler(lockAuthNode.Property["nide8ID"])).GetInfoAsync();
                        if (!string.IsNullOrWhiteSpace(nide8ReturnResult.Meta.ServerIP))
                        {
                            string[] serverIp = nide8ReturnResult.Meta.ServerIP.Split(':');
                            if (serverIp.Length == 2)
                            {
                                nide8Server.Address = serverIp[0];
                                nide8Server.Port = ushort.Parse(serverIp[1]);
                            }
                            else
                            {
                                nide8Server.Address = nide8ReturnResult.Meta.ServerIP;
                                nide8Server.Port = 25565;
                            }
                            nide8Server.ServerName = nide8ReturnResult.Meta.ServerName;
                            serverInfoControl.SetServerInfo(nide8Server);
                        }
                    }
                }
                catch (Exception)
                { }
            }
            else if (App.config.MainConfig.Server != null)
            {
                serverInfoControl.SetServerInfo(App.config.MainConfig.Server);
            }
            APP_Color();
        }

        public void APP_Color()
        {
            Brush_Color get = new Brush_Color();
            Brush b = get.get_Bursh();
            if (b != null)
            {
                cancelLaunchButton.Background = b;
                launchInfoBlock.Background = b;
                volumeButton.BorderBrush = volumeButton.Foreground =
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.config.MainConfig.Customize.AccentColor));
            }
        }

        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.IsMuted)
            {
                this.mediaElement.Play();
                this.mediaElement.IsMuted = false;
                volumeButtonIcon.Kind = PackIconFontAwesomeKind.PauseSolid;
            }
            else
            {
                this.mediaElement.Pause();
                this.mediaElement.IsMuted = true;
                volumeButtonIcon.Kind = PackIconFontAwesomeKind.PlaySolid;
            }
        }
        #endregion

        private async Task LaunchGameFromArgs(Controls.LaunchEventArgs args)
        {
            try
            {
                #region 检查有效数据
                if (args.LaunchVersion == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.EmptyLaunchVersion"),
                        App.GetResourceString("String.Message.EmptyLaunchVersion2"));
                    return;
                }
                if (args.UserNode == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.EmptyUsername"),
                        App.GetResourceString("String.Message.EmptyUsername2"));
                    return;
                }
                if (args.AuthNode == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.EmptyAuthType"),
                        App.GetResourceString("String.Message.EmptyAuthType2"));
                    return;
                }
                if (App.handler.Java == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"), App.GetResourceString("String.Message.NoJava2"));
                    return;
                }
                #endregion


                #region 保存启动数据
                App.config.MainConfig.History.LastLaunchVersion = args.LaunchVersion.ID;
                App.config.MainConfig.History.LastLaunchTime = DateTime.Now;
                #endregion

                LaunchSetting launchSetting = new LaunchSetting()
                {
                    Version = args.LaunchVersion
                };

                loadingRing.Visibility = Visibility.Visible;
                launchInfoBlock.Visibility = Visibility.Visible;
                loadingRing.IsActive = true;

                #region 验证

                #region 设置ClientToken
                if (string.IsNullOrWhiteSpace(App.config.MainConfig.User.ClientToken))
                {
                    App.config.MainConfig.User.ClientToken = Guid.NewGuid().ToString("N");
                }
                else
                {
                    Requester.ClientToken = App.config.MainConfig.User.ClientToken;
                }
                #endregion

                //主验证器接口
                IAuthenticator authenticator = null;
                bool shouldRemember = false;

                bool isRemember = (!string.IsNullOrWhiteSpace(args.UserNode.AccessToken)) && (args.UserNode.SelectProfileUUID != null);
                mainPanel.launchButton.Content = App.GetResourceString("String.Mainwindow.Loging");
                switch (args.AuthNode.AuthType)
                {
                    #region 离线验证
                    case AuthenticationType.OFFLINE:
                        if (args.IsNewUser)
                        {
                            authenticator = new OfflineAuthenticator(args.UserNode.UserName);
                        }
                        else
                        {
                            authenticator = new OfflineAuthenticator(args.UserNode.UserName,
                                args.UserNode.UserData,
                                args.UserNode.SelectProfileUUID);
                        }
                        break;
                    #endregion

                    #region MOJANG验证
                    case AuthenticationType.MOJANG:
                        if (isRemember)
                        {
                            var mYggTokenAuthenticator = new YggdrasilTokenAuthenticator(args.UserNode.AccessToken,
                                args.UserNode.GetSelectProfileUUID(),
                                args.UserNode.UserData);
                            mYggTokenAuthenticator.ProxyAuthServerAddress = "https://authserver.mojang.com";
                            authenticator = mYggTokenAuthenticator;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(mainPanel.PasswordBox.Password) || string.IsNullOrWhiteSpace(args.UserNode.UserName))
                            {
                                await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Password"),
                                    App.GetResourceString("String.Mainwindow.Auth.Error.Check_Login"));
                                return;
                            }
                            else
                            {
                                var mYggAuthenticator = new YggdrasilAuthenticator(new Credentials()
                                {
                                    Username = args.UserNode.UserName,
                                    Password = mainPanel.PasswordBox.Password
                                });
                                mYggAuthenticator.ProxyAuthServerAddress = "https://authserver.mojang.com";
                                authenticator = mYggAuthenticator;
                            }
                        }
                        break;
                    #endregion

                    #region NIDE8验证
                    case AuthenticationType.NIDE8:
                        string nide8ID = args.AuthNode.Property["nide8ID"];
                        if (string.IsNullOrWhiteSpace(nide8ID))
                        {
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID"),
                                App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID2"));
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(mainPanel.PasswordBox.Password) || string.IsNullOrWhiteSpace(args.UserNode.UserName))
                        {
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Password"),
                                    App.GetResourceString("String.Mainwindow.Auth.Error.Check_Login"));
                            return;
                        }
                        if (isRemember)
                        {
                            var nYggTokenCator = new Nide8TokenAuthenticator(nide8ID, args.UserNode.AccessToken,
                                args.UserNode.GetSelectProfileUUID(),
                                args.UserNode.UserData);
                            authenticator = nYggTokenCator;
                        }
                        else
                        {
                            var nYggCator = new Nide8Authenticator(
                                nide8ID,
                                new Credentials()
                                {
                                    Username = args.UserNode.UserName,
                                    Password = mainPanel.PasswordBox.Password
                                });
                            authenticator = nYggCator;
                        }
                        break;
                    #endregion

                    #region AUTHLIB验证
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        string aiRootAddr = args.AuthNode.Property["authserver"];
                        if (string.IsNullOrWhiteSpace(aiRootAddr))
                        {
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress"),
                                App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress2"));
                            return;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(mainPanel.PasswordBox.Password) || string.IsNullOrWhiteSpace(args.UserNode.UserName))
                            {
                                await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Password"),
                                    App.GetResourceString("String.Mainwindow.Auth.Error.Check_Login"));
                                return;
                            }
                            if (isRemember)
                            {
                                var cYggTokenCator = new AuthlibInjectorTokenAuthenticator(aiRootAddr,
                                    args.UserNode.AccessToken,
                                    args.UserNode.GetSelectProfileUUID(),
                                    args.UserNode.UserData);
                                authenticator = cYggTokenCator;
                            }
                            else
                            {
                                var cYggAuthenticator = new AuthlibInjectorAuthenticator(
                                        aiRootAddr,
                                        new Credentials()
                                        {
                                            Username = args.UserNode.UserName,
                                            Password = mainPanel.PasswordBox.Password
                                        });
                                authenticator = cYggAuthenticator;
                            }
                        }
                        break;
                    #endregion

                    #region 自定义验证
                    case AuthenticationType.CUSTOM_SERVER:
                        string customAuthServer = args.AuthNode.Property["authserver"];
                        if (string.IsNullOrWhiteSpace(customAuthServer))
                        {
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress"),
                                App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress2"));
                            return;
                        }
                        else
                        {
                            if (isRemember)
                            {
                                var cYggTokenCator = new YggdrasilTokenAuthenticator(args.UserNode.AccessToken,
                                args.UserNode.GetSelectProfileUUID(),
                                args.UserNode.UserData);
                                cYggTokenCator.ProxyAuthServerAddress = customAuthServer;
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(mainPanel.PasswordBox.Password) || string.IsNullOrWhiteSpace(args.UserNode.UserName))
                                {
                                    await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Password"),
                                    App.GetResourceString("String.Mainwindow.Auth.Error.Check_Login"));
                                    return;
                                }
                                var cYggAuthenticator = new YggdrasilAuthenticator(new Credentials()
                                {
                                    Username = args.UserNode.UserName,
                                    Password = mainPanel.PasswordBox.Password
                                });
                                cYggAuthenticator.ProxyAuthServerAddress = customAuthServer;
                                authenticator = cYggAuthenticator;
                            }
                        }
                        break;
                    #endregion

                    #region 意外情况
                    default:
                        if (args.IsNewUser)
                        {
                            authenticator = new OfflineAuthenticator(args.UserNode.UserName);
                        }
                        else
                        {
                            authenticator = new OfflineAuthenticator(args.UserNode.UserName,
                                args.UserNode.UserData,
                                args.UserNode.SelectProfileUUID);
                        }
                        break;
                        #endregion
                }

                //如果验证方式不是离线验证
                if (args.AuthNode.AuthType != AuthenticationType.OFFLINE)
                {
                    if(authenticator == null)
                    {
                        await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                            App.GetResourceString("String.Mainwindow.Auth.Error.Null"));
                        return;
                    }
                    var authResult = await authenticator.DoAuthenticateAsync();

                    if (authResult == null)
                    {
                        await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                            App.GetResourceString("String.Mainwindow.Auth.Error.Null"));
                        return;
                    }

                    switch (authResult.State)
                    {
                        case AuthState.SUCCESS:
                            args.UserNode.SelectProfileUUID = authResult.SelectedProfileUUID.Value;
                            args.UserNode.UserData = authResult.UserData;
                            if (authResult.Profiles != null)
                            {
                                args.UserNode.Profiles.Clear();
                                authResult.Profiles.ForEach(x => args.UserNode.Profiles.Add(x.Value, x));
                            }
                            if (shouldRemember)
                            {
                                args.UserNode.AccessToken = authResult.AccessToken;
                            }

                            args.UserNode.AccessToken = authResult.AccessToken;

                            launchSetting.AuthenticateResult = authResult;
                            break;
                        case AuthState.REQ_LOGIN:
                            args.UserNode.ClearAuthCache();
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                                App.GetResourceString("String.Mainwindow.Auth.Error.Login_Save_out"));
                            mainPanel.PasswordBox.Password = null;
                            return;
                        case AuthState.ERR_INVALID_CRDL:
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                                App.GetResourceString("String.Mainwindow.Auth.Error.Password_Error"));
                            mainPanel.PasswordBox.Password = null;
                            return;
                        case AuthState.ERR_NOTFOUND:
                            if (args.AuthNode.AuthType == AuthenticationType.CUSTOM_SERVER || args.AuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                            {
                                await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                                string.Format(App.GetResourceString("String.Mainwindow.Auth.Error.Auth_Error_notfound"),
                                authResult.Error.ErrorMessage));
                            }
                            else
                            {
                                await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                                string.Format(App.GetResourceString("String.Mainwindow.Auth.Error.Player_notfound"),
                                authResult.Error.ErrorMessage));
                            }
                            return;
                        case AuthState.ERR_OTHER:
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                                string.Format(App.GetResourceString("String.Mainwindow.Auth.Error.Other"),
                                authResult.Error.ErrorMessage));
                            return;
                        case AuthState.ERR_INSIDE:
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                                string.Format(App.GetResourceString("String.Mainwindow.Auth.Error.Core_Error"),
                                authResult.Error.ErrorMessage));
                            return;
                        default:
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Error.Login_Error"),
                                App.GetResourceString("String.Mainwindow.Auth.Error.Error"));
                            return;
                    }
                }
                else
                {
                    var authResult = await authenticator.DoAuthenticateAsync();
                    launchSetting.AuthenticateResult = authResult;
                    args.UserNode.UserData = authResult.UserData;
                    args.UserNode.SelectProfileUUID = authResult.SelectedProfileUUID.Value;
                }

                App.config.MainConfig.History.SelectedUserNodeID = args.UserNode.UserData.Uuid;
                if (!App.config.MainConfig.User.UserDatabase.ContainsKey(args.UserNode.UserData.Uuid))
                {
                    App.config.MainConfig.User.UserDatabase.Add(args.UserNode.UserData.Uuid, args.UserNode);
                }
                #endregion

                #region 检查游戏完整
                List<DownloadTask> losts = new List<DownloadTask>();

                App.logHandler.AppendInfo("检查丢失的依赖库文件中...");
                var lostDepend = await FileHelper.GetLostDependDownloadTaskAsync(
                    App.config.MainConfig.Download.DownloadSource,
                    App.handler,
                    launchSetting.Version);

                if (args.AuthNode.AuthType == AuthenticationType.NIDE8)
                {
                    string nideJarPath = App.handler.GetNide8JarPath();
                    if (!File.Exists(nideJarPath))
                    {
                        lostDepend.Add(new DownloadTask("统一通行证核心", "https://login2.nide8.com:233/index/jar", nideJarPath));
                    }
                }
                else if (args.AuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                {
                    string aiJarPath = App.handler.GetAIJarPath();
                    if (!File.Exists(aiJarPath))
                    {
                        lostDepend.Add(await NsisoLauncherCore.Net.Tools.GetDownloadUrl.GetAICoreDownloadTask(App.config.MainConfig.Download.DownloadSource, aiJarPath));
                    }
                }

                if (App.config.MainConfig.Environment.DownloadLostDepend && lostDepend.Count != 0)
                {
                    MessageDialogResult downDependResult = await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadDepend"),
                        App.GetResourceString("String.Mainwindow.NeedDownloadDepend2"),
                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings()
                        {
                            AffirmativeButtonText = App.GetResourceString("String.Base.Download"),
                            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                            FirstAuxiliaryButtonText = App.GetResourceString("String.Base.Unremember"),
                            DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    switch (downDependResult)
                    {
                        case MessageDialogResult.Affirmative:
                            losts.AddRange(lostDepend);
                            break;
                        case MessageDialogResult.FirstAuxiliary:
                            App.config.MainConfig.Environment.DownloadLostDepend = false;
                            break;
                        default:
                            break;
                    }

                }

                App.logHandler.AppendInfo("检查丢失的资源文件中...");
                if (App.config.MainConfig.Environment.DownloadLostAssets && (await FileHelper.IsLostAssetsAsync(App.config.MainConfig.Download.DownloadSource,
                    App.handler, launchSetting.Version)))
                {
                    MessageDialogResult downDependResult = await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadAssets"),
                        App.GetResourceString("String.Mainwindow.NeedDownloadAssets2"),
                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings()
                        {
                            AffirmativeButtonText = App.GetResourceString("String.Base.Download"),
                            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                            FirstAuxiliaryButtonText = App.GetResourceString("String.Base.Unremember"),
                            DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    switch (downDependResult)
                    {
                        case MessageDialogResult.Affirmative:
                            var lostAssets = await FileHelper.GetLostAssetsDownloadTaskAsync(
                                App.config.MainConfig.Download.DownloadSource,
                                App.handler, launchSetting.Version);
                            losts.AddRange(lostAssets);
                            break;
                        case MessageDialogResult.FirstAuxiliary:
                            App.config.MainConfig.Environment.DownloadLostAssets = false;
                            break;
                        default:
                            break;
                    }
                }

                updata_pack pack = null;
                bool isupdata = false;
                string packname = null;
                string vision = null;
                if (App.config.MainConfig.Server.Mods_Check == null)
                {
                    App.config.MainConfig.Server.Mods_Check = new Mods_Check()
                    {
                        Enable = false,
                        Address = "",
                        packname = "modpack",
                        Vision = "0.0.0"
                    };
                }
                if (App.config.MainConfig.Server.Mods_Check.Enable)
                {
                    mainPanel.launchButton.Content = App.GetResourceString("String.Mainwindow.Check.mods");
                    pack = new updata_pack();

                    var lost_mod = await new updata_check().check();
                    if (lost_mod != null)
                    {
                        if (await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Check.new"),
                            App.GetResourceString("String.Mainwindow.Check.new.ask"),
                            MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                            {
                                AffirmativeButtonText = App.GetResourceString("String.Base.Download"),
                                NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                                DefaultButtonFocus = MessageDialogResult.Affirmative
                            }) == MessageDialogResult.Affirmative)
                        {
                            losts.AddRange(await lost_mod.setupdata(pack));
                            packname = lost_mod.getpackname();
                            vision = lost_mod.getvision();
                            isupdata = true;
                        }
                    }
                }

                if (losts.Count != 0)
                {
                    if (!App.downloader.IsBusy)
                    {
                        App.downloader.SetDownloadTasks(losts);
                        App.downloader.StartDownload();
                        var downloadResult = await new DownloadWindow().ShowWhenDownloading();
                        if (downloadResult?.ErrorList?.Count != 0)
                        {
                            foreach (KeyValuePair<DownloadTask, Exception> task in downloadResult?.ErrorList)
                            {
                                string filename = task.Key.To + ".downloadtask";
                                if (File.Exists(filename))
                                    File.Delete(filename);
                            }
                            await this.ShowMessageAsync(string.Format("有{0}个文件下载补全失败", downloadResult.ErrorList.Count),
                                "这可能是因为本地网络问题或下载源问题，您可以尝试检查网络环境或在设置中切换首选下载源。");
                            return;
                        }
                        else
                        {
                            if (pack != null)
                            {
                                isupdata = await pack.pack();
                            }
                            if (isupdata)
                            {
                                App.config.MainConfig.Server.Mods_Check.packname = packname;
                                App.config.MainConfig.Server.Mods_Check.Vision = vision;
                            }
                        }
                    }
                    else
                    {
                        await this.ShowMessageAsync("无法下载补全：当前有正在下载中的任务", "请等待其下载完毕或取消下载。");
                        return;
                    }
                }

                #endregion

                #region 根据配置文件设置
                launchSetting.AdvencedGameArguments += App.config.MainConfig.Environment.AdvencedGameArguments;
                launchSetting.AdvencedJvmArguments += App.config.MainConfig.Environment.AdvencedJvmArguments;
                launchSetting.GCArgument += App.config.MainConfig.Environment.GCArgument;
                launchSetting.GCEnabled = App.config.MainConfig.Environment.GCEnabled;
                launchSetting.GCType = App.config.MainConfig.Environment.GCType;
                launchSetting.JavaAgent += App.config.MainConfig.Environment.JavaAgent;
                if (args.AuthNode.AuthType == AuthenticationType.NIDE8)
                {
                    launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.handler.GetNide8JarPath(), args.AuthNode.Property["nide8ID"]);
                }
                else if (args.AuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                {
                    launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.handler.GetAIJarPath(), args.AuthNode.Property["authserver"]);
                }

                //直连服务器设置
                var lockAuthNode = App.config.MainConfig.User.GetLockAuthNode();
                if (App.config.MainConfig.User.Nide8ServerDependence &&
                    (lockAuthNode != null) && App.config.MainConfig.Server.LaunchToServer &&
                        (lockAuthNode.AuthType == AuthenticationType.NIDE8))
                {
                    var nide8ReturnResult = await (new NsisoLauncherCore.Net.Nide8API.APIHandler(lockAuthNode.Property["nide8ID"])).GetInfoAsync();
                    if (!string.IsNullOrWhiteSpace(nide8ReturnResult.Meta.ServerIP))
                    {
                        NsisoLauncherCore.Modules.Server server = new NsisoLauncherCore.Modules.Server();
                        string[] serverIp = nide8ReturnResult.Meta.ServerIP.Split(':');
                        if (serverIp.Length == 2)
                        {
                            server.Address = serverIp[0];
                            server.Port = ushort.Parse(serverIp[1]);
                        }
                        else
                        {
                            server.Address = nide8ReturnResult.Meta.ServerIP;
                            server.Port = 25565;
                        }
                        launchSetting.LaunchToServer = server;
                    }
                }
                else if (App.config.MainConfig.Server.LaunchToServer)
                {
                    launchSetting.LaunchToServer = new NsisoLauncherCore.Modules.Server() 
                    { 
                        Address = App.config.MainConfig.Server.Address, 
                        Port = App.config.MainConfig.Server.Port 
                    };
                }

                //自动内存设置
                if (App.config.MainConfig.Environment.AutoMemory)
                {
                    var m = SystemTools.GetBestMemory(App.handler.Java);
                    App.config.MainConfig.Environment.MaxMemory = m;
                    launchSetting.MaxMemory = m;
                }
                else
                {
                    launchSetting.MaxMemory = App.config.MainConfig.Environment.MaxMemory;
                }
                launchSetting.VersionType = App.config.MainConfig.Customize.VersionInfo;
                launchSetting.WindowSize = App.config.MainConfig.Environment.WindowSize;
                #endregion

                #region 配置文件处理
                App.config.Save();
                #endregion

                #region 启动
                cancelLaunchButton.Visibility = Visibility.Visible;
                mainPanel.launchButton.Content = App.GetResourceString("String.Mainwindow.Staring");
                App.logHandler.OnLog += (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b.Message; }); };
                var result = await App.handler.LaunchAsync(launchSetting);
                App.logHandler.OnLog -= (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b.Message; }); };

                //程序猿是找不到女朋友的了 :) 
                if (!result.IsSuccess)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.LaunchError") + result.LaunchException.Title, result.LaunchException.Message);
                    App.logHandler.AppendError(result.LaunchException);
                }
                else
                {
                    cancelLaunchButton.Click += (x, y) => { CancelLaunching(result); };

                    #region 等待游戏响应
                    try
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            result.Process.WaitForInputIdle();
                        });
                    }
                    catch (Exception ex)
                    {
                        App.logHandler.AppendFatal(ex);
                        return;
                    }
                    #endregion

                    cancelLaunchButton.Click -= (x, y) => { CancelLaunching(result); };

                    #region 数据反馈
                    //API使用次数计数器+1
                    await App.nsisoAPIHandler.RefreshUsingTimesCounter();
                    #endregion

                    App.config.MainConfig.History.LastLaunchUsingMs = result.LaunchUsingMs;
                    if (App.config.MainConfig.Environment.ExitAfterLaunch)
                    {
                        Application.Current.Shutdown();
                    }
                    this.WindowState = WindowState.Minimized;

                    mainPanel.is_re = true;
                    mainPanel.Refresh();

                    //自定义处理
                    if (!string.IsNullOrWhiteSpace(App.config.MainConfig.Customize.GameWindowTitle))
                    {
                        GameHelper.SetGameTitle(result, App.config.MainConfig.Customize.GameWindowTitle);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                App.logHandler.AppendFatal(ex);
            }
            finally
            {
                //Color_yr Add Start
                mainPanel.use_switch(true);
                mainPanel.launchButton.Content = App.GetResourceString("String.Base.Launch");
                loadingRing.Visibility = Visibility.Hidden;
                launchInfoBlock.Visibility = Visibility.Hidden;
                cancelLaunchButton.Visibility = Visibility.Hidden;
                //Color_yr Add Stop
                this.loadingRing.IsActive = false;
            }
        }

        #region MainWindow event

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            #region 无JAVA提示
            if (App.handler.Java == null)
            {
                var result = await this.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"),
                    App.GetResourceString("String.Message.NoJava2"),
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                        NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                        DefaultButtonFocus = MessageDialogResult.Affirmative
                    });
                if (result == MessageDialogResult.Affirmative)
                {
                    var arch = SystemTools.GetSystemArch();
                    switch (arch)
                    {
                        case ArchEnum.x32:
                            App.downloader.SetDownloadTasks(new DownloadTask("32位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x86.exe", "jre_x86.exe"));
                            App.downloader.StartDownload();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x86.exe");
                            break;
                        case ArchEnum.x64:
                            App.downloader.SetDownloadTasks(new DownloadTask("64位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x64.exe", "jre_x64.exe"));
                            App.downloader.StartDownload();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x64.exe");
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion
            #region 检查更新
            if (App.config.MainConfig.Launcher.CheckUpdate)
            {
                //await CheckUpdate();
            }
            #endregion
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.downloader.IsBusy)
            {
                var choose = this.ShowModalMessageExternal(App.GetResourceString("String.Downloadwindow.BackDown"),
                    App.GetResourceString("String.Downloadwindow.BackDown_S"), MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings()
                {
                    AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                    NegativeButtonText = App.GetResourceString("String.Base.Cancel")
                });
                if (choose == MessageDialogResult.Affirmative)
                {
                    App.downloader.RequestStop();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
        #endregion

        #region Tools
        private bool IsValidateLoginData(LoginDialogData data)
        {
            if (data == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(data.Username))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(data.Password))
            {
                return false;
            }
            return true;
        }

        private void CancelLaunching(LaunchResult result)
        {
            if (!result.Process.HasExited)
            {
                result.Process.Kill();
            }
        }
        #endregion
    }
}
