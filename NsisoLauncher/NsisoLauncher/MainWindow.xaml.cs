﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using NsisoLauncher.Config;
using NsisoLauncher.Updata;
using NsisoLauncher.Utils;
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

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public string[] PicFiles;
        public string[] Mp3Files;
        public string[] Mp4Files;

        private Timer timer;
        private bool cancalrun = false;

        int PicNow = 0;
        int MediaNow = 0;

        //TODO:增加取消启动按钮
        public MainWindow()
        {
            InitializeComponent();
            App.LogHandler.AppendDebug("启动器主窗体已载入");
            mainPanel.Launch += MainPanel_Launch;
            App.Handler.GameExit += Handler_GameExit;
            App.MainWindow_ = this;
            CustomizeRefresh();
            if (App.Config.MainConfig.Launcher.AutoRun)
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    Application.Current.Dispatcher.Invoke(() =>
                   {
                       mainPanel.launchButton_Click(null, null);
                   });
                });
            }
        }

        private void ChangeBackPic(Uri uri)
        {
            Dispatcher.Invoke(() =>
            {
                BG.Source = new BitmapImage(uri);
            });
            GC.Collect();
        }
        private void PicShow(object sender)
        {
            PicNow = PicNow >= PicFiles.Length ? 0 : +1;
            ChangeBackPic(new Uri(PicFiles[PicNow]));
        }
        public void Pic_cyclic()
        {
            if (App.Config.MainConfig.Customize.CustomBackGroundPictureCyclic && PicFiles.Length > 1)
            {
                if (timer != null)
                    timer.Dispose();
                timer = new Timer(new TimerCallback(PicShow), null, TimeSpan.Zero, 
                    TimeSpan.FromMilliseconds(App.Config.MainConfig.Customize.CustomBackGroundPictureCyclicTime));
            }
            else
                ChangeBackPic(new Uri(PicFiles[0]));
        }
        public void Mp4Play()
        {
            try
            {
                mediaElement.Stop();
                mediaElement.Source = new Uri(Mp4Files[App.Config.MainConfig.Customize.CustomBackGroundVedioRandom ? new Random().Next(Mp4Files.Length) : MediaNow]);
                mediaElement.Volume = (double)App.Config.MainConfig.Customize.CustomBackGroundSound / 100;
                mediaElement.Play();
                MediaNow = MediaNow >= Mp4Files.Length ? 0 : +1;
            }
            catch (Exception) { }
        }
        public void Mp3Play()
        {
            try
            {
                mediaElement.Stop();
                mediaElement.Source = new Uri(Mp3Files[App.Config.MainConfig.Customize.CustomBackGroundMusicRandom ? new Random().Next(Mp3Files.Length) : MediaNow]);
                mediaElement.Volume = (double)App.Config.MainConfig.Customize.CustomBackGroundSound / 100;
                mediaElement.Play();
                MediaNow = MediaNow >= Mp3Files.Length ? 0 : +1;
            }
            catch (Exception) { }
        }
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (Mp4Files?.Length != 0 && App.Config.MainConfig.Customize.CustomBackGroundVedioCyclic)
                Mp4Play();
            else if (App.Config.MainConfig.Customize.CustomBackGroundMusicCyclic)
                Mp3Play();
        }
        private async void MainPanel_Launch(object sender, Controls.LaunchEventArgs obj)
        {
            await LaunchGameFromArgs(obj);
        }

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

        public void Refresh()
        {
            mainPanel.isRes = true;
            mainPanel.Refresh();
        }

        public async void CustomizeRefresh()
        {
            if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Customize.LauncherTitle))
            {
                this.Title = App.Config.MainConfig.Customize.LauncherTitle;
            }
            if (App.Config.MainConfig.Customize.CustomBackGroundVedio)
            {
                Mp4Files = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "*.mp4");
                if (Mp4Files.Length != 0)
                {
                    volumeButton.Visibility = Visibility.Visible;
                    mediaElement.Visibility = Visibility.Visible;
                    Mp4Play();
                    MediaNow = 0;
                }
                else
                {
                    volumeButton.Visibility = Visibility.Hidden;
                    mediaElement.Visibility = Visibility.Hidden;
                    mediaElement.Stop();
                }
            }
            else
            {
                if (App.Config.MainConfig.Customize.CustomBackGroundMusic)
                {
                    Mp3Files = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "*.mp3");
                    if (Mp3Files.Length != 0)
                    {
                        volumeButton.Visibility = Visibility.Visible;
                        mediaElement.Visibility = Visibility.Visible;
                        Mp3Play();
                        MediaNow = 0;
                    }
                    else
                    {
                        volumeButton.Visibility = Visibility.Hidden;
                        mediaElement.Visibility = Visibility.Hidden;
                        mediaElement.Stop();
                    }
                }
                if (App.Config.MainConfig.Customize.CustomBackGroundPicture)
                {
                    string[] files = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "*.png");
                    string[] files1 = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "*.jpg");
                    string[] icon = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "icon.ico");
                    if (icon.Length != 0)
                    {
                        this.Icon = new BitmapImage(new Uri(icon[0]));
                        ShowIconOnTitleBar = false;
                    }
                    if (files.Count() + files1.Count() != 0)
                    {
                        PicFiles = new string[files.Length + files1.Length];
                        int i = 0;
                        foreach (string a in files)
                        {
                            PicFiles[i] = a;
                            i++;
                        }
                        foreach (string a in files1)
                        {
                            PicFiles[i] = a;
                            i++;
                        }
                        Pic_cyclic();
                    }
                }
            }
            if (App.Config.MainConfig.User.Nide8ServerDependence)
            {
                try
                {
                    var lockAuthNode = App.Config.MainConfig.User.GetLockAuthNode();
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
            else if (App.Config.MainConfig.Server != null)
            {
                serverInfoControl.SetServerInfo(App.Config.MainConfig.Server);
            }
            APP_Color();
        }

        public void APP_Color()
        {
            BrushColor get = new BrushColor();
            Brush b = get.GetBursh();
            if (b != null)
            {
                cancelLaunchButton.Background = b;
                launchInfoBlock.Background = b;
                Side.Background = b;
                volumeButton.BorderBrush = volumeButton.Foreground =
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Config.MainConfig.Customize.AccentColor));
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

        private async Task LaunchGameFromArgs(Controls.LaunchEventArgs args)
        {
            try
            {
                App.LogHandler.OnLog += (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b.Message; }); };
                Side_E.IsExpanded = false;
                App.LogHandler.AppendInfo("检查有效数据...");

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
                if (App.Handler.Java == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"), App.GetResourceString("String.Message.NoJava2"));
                    return;
                }

                App.Config.MainConfig.History.LastLaunchVersion = args.LaunchVersion.ID;
                App.Config.MainConfig.History.LastLaunchTime = DateTime.Now;

                LaunchSetting launchSetting = new LaunchSetting()
                {
                    Version = args.LaunchVersion
                };

                loadingRing.Visibility = Visibility.Visible;
                launchInfoBlock.Visibility = Visibility.Visible;
                loadingRing.IsActive = true;

                if (string.IsNullOrWhiteSpace(App.Config.MainConfig.User.ClientToken))
                {
                    App.Config.MainConfig.User.ClientToken = Guid.NewGuid().ToString("N");
                }
                else
                {
                    Requester.ClientToken = App.Config.MainConfig.User.ClientToken;
                }

                //主验证器接口
                App.LogHandler.AppendInfo("登陆中...");
                IAuthenticator authenticator = null;
                bool shouldRemember = false;

                bool isRemember = (!string.IsNullOrWhiteSpace(args.UserNode.AccessToken)) && (args.UserNode.SelectProfileUUID != null);
                mainPanel.launchButton.Content = App.GetResourceString("String.Mainwindow.Loging");
                switch (args.AuthNode.AuthType)
                {
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
                }

                //如果验证方式不是离线验证
                if (args.AuthNode.AuthType != AuthenticationType.OFFLINE)
                {
                    if (authenticator == null)
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
                            if (authResult.SelectedProfileUUID == null)
                            {
                                //没有游戏角色
                                if (authResult.Profiles == null || authResult.Profiles.Count == 0)
                                {
                                    await this.ShowMessageAsync(App.GetResourceString("String.Message.Auth.ErrorNO.Title"),
                                    App.GetResourceString("String.Message.Auth.ErrorNO.Text"));
                                    return;
                                }
                                await this.ShowMessageAsync(App.GetResourceString("String.Message.Auth.ErrorNO1.Title"),
                                    App.GetResourceString("String.Message.Auth.ErrorNO1.Text"));
                                return;
                            }
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

                App.Config.MainConfig.History.SelectedUserNodeID = args.UserNode.UserData.Uuid;
                if (!App.Config.MainConfig.User.UserDatabase.ContainsKey(args.UserNode.UserData.Uuid))
                {
                    App.Config.MainConfig.User.UserDatabase.Add(args.UserNode.UserData.Uuid, args.UserNode);
                }

                List<DownloadTask> losts = new List<DownloadTask>();

                App.LogHandler.AppendInfo("检查丢失的文件中...");
                var lostDepend = await FileHelper.GetLostDependDownloadTaskAsync(
                    App.Config.MainConfig.Download.DownloadSource,
                    App.Handler,
                    launchSetting.Version,
                    this);
                if (args.AuthNode.AuthType == AuthenticationType.NIDE8)
                {
                    string nideJarPath = App.Handler.GetNide8JarPath();
                    if (!File.Exists(nideJarPath))
                    {
                        lostDepend.Add(new DownloadTask("统一通行证核心", "https://login2.nide8.com:233/index/jar", nideJarPath));
                    }
                }
                else if (args.AuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                {
                    string aiJarPath = App.Handler.GetAIJarPath();
                    if (!File.Exists(aiJarPath))
                    {
                        lostDepend.Add(await NsisoLauncherCore.Net.Tools.GetDownloadUrl.GetAICoreDownloadTask(App.Config.MainConfig.Download.DownloadSource, aiJarPath));
                    }
                }

                if (App.Config.MainConfig.Environment.DownloadLostDepend && lostDepend.Count != 0)
                {
                    MessageDialogResult downDependResult = await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadDepend"),
                        App.GetResourceString("String.Mainwindow.NeedDownloadDepend2"),
                        MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                        {
                            AffirmativeButtonText = App.GetResourceString("String.Base.Download"),
                            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                            DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    switch (downDependResult)
                    {
                        case MessageDialogResult.Affirmative:
                            losts.AddRange(lostDepend);
                            break;
                        case MessageDialogResult.FirstAuxiliary:
                            App.Config.MainConfig.Environment.DownloadLostDepend = false;
                            break;
                        default:
                            break;
                    }

                }

                if (App.Config.MainConfig.Environment.DownloadLostAssets &&
                    (await FileHelper.IsLostAssetsAsync(App.Config.MainConfig.Download.DownloadSource,
                    App.Handler, launchSetting.Version)))
                {
                    MessageDialogResult downDependResult = await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadAssets"),
                        App.GetResourceString("String.Mainwindow.NeedDownloadAssets2"),
                        MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                        {
                            AffirmativeButtonText = App.GetResourceString("String.Base.Download"),
                            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                            DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    switch (downDependResult)
                    {
                        case MessageDialogResult.Affirmative:
                            var lostAssets = await FileHelper.GetLostAssetsDownloadTaskAsync(
                                App.Config.MainConfig.Download.DownloadSource,
                                App.Handler, launchSetting.Version);
                            losts.AddRange(lostAssets);
                            break;
                        case MessageDialogResult.FirstAuxiliary:
                            App.Config.MainConfig.Environment.DownloadLostAssets = false;
                            break;
                        default:
                            break;
                    }
                }

                OtherCheck pack = null;
                string packname = null;
                string vision = null;
                if (App.Config.MainConfig.Server.UpdataCheck == null)
                {
                    App.Config.MainConfig.Server.UpdataCheck = new Config.UpdataCheck()
                    {
                        Enable = false,
                        Address = "",
                        Packname = "modpack",
                        Vision = "0.0.0"
                    };
                }
                if (App.Config.MainConfig.Server.UpdataCheck.Enable)
                {
                    App.LogHandler.AppendInfo("检查客户端更新...");
                    mainPanel.launchButton.Content = App.GetResourceString("String.Mainwindow.Check.mods");
                    pack = new OtherCheck();

                    var lostmod = await new Updata.UpdataCheck().Check();
                    if (lostmod != null)
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
                            losts.AddRange(await lostmod.CheckUpdata(pack));
                            packname = lostmod.getpackname();
                            vision = lostmod.getvision();
                        }
                    }
                }

                if (losts.Count != 0)
                {
                    if (!App.Downloader.IsBusy)
                    {
                        App.Downloader.SetDownloadTasks(losts);
                        App.Downloader.StartDownloadAsync();
                        var downloadResult = await new DownloadWindow().ShowWhenDownloading();
                        if (downloadResult?.cancel == false)
                        {
                            if (downloadResult?.ErrorList?.Count != 0)
                            {
                                foreach (KeyValuePair<DownloadTask, Exception> task in downloadResult?.ErrorList)
                                {
                                    string filename = task.Key.To + ".downloadtask";
                                    if (File.Exists(filename))
                                        File.Delete(filename);
                                }
                                await this.ShowMessageAsync(string.Format(App.GetResourceString("String.Mainwindow.Download.Errot.Title"), downloadResult.ErrorList.Count),
                                    App.GetResourceString("String.Mainwindow.Download.Errot.Text"));
                                return;
                            }
                            else
                            {
                                if (pack != null && await pack?.pack())
                                {
                                    App.Config.MainConfig.Server.UpdataCheck.Packname = packname;
                                    App.Config.MainConfig.Server.UpdataCheck.Vision = vision;
                                }
                            }
                        }
                    }
                    else
                    {
                        await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Download.Busy.Title"),
                            App.GetResourceString("String.Mainwindow.Download.Busy.Title"));
                        return;
                    }
                }

                App.LogHandler.AppendInfo("准备启动...");
                launchSetting.AdvencedGameArguments += App.Config.MainConfig.Environment.AdvencedGameArguments;
                launchSetting.AdvencedJvmArguments += App.Config.MainConfig.Environment.AdvencedJvmArguments;
                launchSetting.GCArgument += App.Config.MainConfig.Environment.GCArgument;
                launchSetting.GCEnabled = App.Config.MainConfig.Environment.GCEnabled;
                launchSetting.GCType = App.Config.MainConfig.Environment.GCType;
                launchSetting.JavaAgent += App.Config.MainConfig.Environment.JavaAgent;
                if (args.AuthNode.AuthType == AuthenticationType.NIDE8)
                {
                    launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.Handler.GetNide8JarPath(), args.AuthNode.Property["nide8ID"]);
                }
                else if (args.AuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                {
                    launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.Handler.GetAIJarPath(), args.AuthNode.Property["authserver"]);
                }

                //直连服务器设置
                var lockAuthNode = App.Config.MainConfig.User.GetLockAuthNode();
                if (App.Config.MainConfig.User.Nide8ServerDependence &&
                    (lockAuthNode != null) && App.Config.MainConfig.Server.LaunchToServer &&
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
                else if (App.Config.MainConfig.Server.LaunchToServer)
                {
                    launchSetting.LaunchToServer = new NsisoLauncherCore.Modules.Server()
                    {
                        Address = App.Config.MainConfig.Server.Address,
                        Port = App.Config.MainConfig.Server.Port
                    };
                }

                //自动内存设置
                if (App.Config.MainConfig.Environment.AutoMemory)
                {
                    var m = SystemTools.GetBestMemory(App.Handler.Java);
                    App.Config.MainConfig.Environment.MaxMemory = m;
                    launchSetting.MaxMemory = m;
                }
                else
                {
                    launchSetting.MaxMemory = App.Config.MainConfig.Environment.MaxMemory;
                }
                launchSetting.VersionType = App.Config.MainConfig.Customize.VersionInfo;
                launchSetting.WindowSize = App.Config.MainConfig.Environment.WindowSize;

                App.Config.Save();

                App.LogHandler.AppendInfo("开始启动...");
                cancelLaunchButton.Visibility = Visibility.Visible;
                mainPanel.launchButton.Content = App.GetResourceString("String.Mainwindow.Staring");

                //启动游戏
                var result = await App.Handler.LaunchAsync(launchSetting);

                //程序猿是找不到女朋友的了 :) 
                if (!result.IsSuccess)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.LaunchError") + result.LaunchException.Title, result.LaunchException.Message);
                    App.LogHandler.AppendError(result.LaunchException);
                }
                else
                {
                    cancelLaunchButton.Click += (x, y) => { CancelLaunching(result); };

                    try
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            result.Process.WaitForInputIdle();
                        });
                    }
                    catch (Exception ex)
                    {
                        App.LogHandler.AppendFatal(ex);
                        return;
                    }

                    //API使用次数计数器+1
                    await App.nsisoAPIHandler.RefreshUsingTimesCounter();

                    App.Config.MainConfig.History.LastLaunchUsingMs = result.LaunchUsingMs;
                    if (App.Config.MainConfig.Environment.ExitAfterLaunch)
                    {
                        Application.Current.Shutdown();
                    }
                    if (!cancalrun)
                    {
                        WindowState = WindowState.Minimized;
                    }
                    else
                    {
                        Activate();
                        cancalrun = false;
                    }

                    mainPanel.isRes = true;
                    mainPanel.Refresh();

                    //自定义处理
                    if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Customize.GameWindowTitle))
                    {
                        GameHelper.SetGameTitle(result, App.Config.MainConfig.Customize.GameWindowTitle);
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogHandler.AppendFatal(ex);
            }
            finally
            {
                App.LogHandler.OnLog -= (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b.Message; }); };
                mainPanel.Lock(true);
                Side_E.IsExpanded = true;
                mainPanel.launchButton.Content = App.GetResourceString("String.Base.Launch");
                loadingRing.Visibility = Visibility.Hidden;
                launchInfoBlock.Visibility = Visibility.Hidden;
                cancelLaunchButton.Visibility = Visibility.Hidden;
                loadingRing.IsActive = false;
            }
        }

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Handler.Java == null)
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
                            App.Downloader.SetDownloadTasks(new DownloadTask("32位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x86.exe", "jre_x86.exe"));
                            App.Downloader.StartDownloadAsync();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x86.exe");
                            break;
                        case ArchEnum.x64:
                            App.Downloader.SetDownloadTasks(new DownloadTask("64位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x64.exe", "jre_x64.exe"));
                            App.Downloader.StartDownloadAsync();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x64.exe");
                            break;
                        default:
                            break;
                    }
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.Java.Finish.Title"),
                    App.GetResourceString("String.Message.Java.Finish.Text"));
                }
            }
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.Downloader.IsBusy)
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
                    Task.Run(() => App.Downloader.RequestStopAsync());
                    timer.Dispose();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void CancelLaunching(LaunchResult result)
        {
            if (!result.Process.HasExited)
            {
                result.Process.Kill();
                cancalrun = true;
            }
        }

        private void Side_Click(object sender, RoutedEventArgs e)
        {
            var now = Side_E.IsExpanded;
            Side_E.IsExpanded = !now;
        }
    }
}
