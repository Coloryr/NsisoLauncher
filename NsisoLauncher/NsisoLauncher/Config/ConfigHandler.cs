﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace NsisoLauncher.Config
{
    public class ConfigHandler
    {
        /// <summary>
        /// 配置文件存放文件夹
        /// </summary>
        public DirectoryInfo Directory { get; set; }

        /// <summary>
        /// 首要配置文件路径
        /// </summary>
        public string MainConfigPath { get => Directory.FullName + @"\MainConfig.json"; }

        /// <summary>
        /// 官方用户配置文件路径
        /// </summary>
        public string LauncherProfilesConfigPath { get => Path.GetFullPath(@".minecraft\launcher_profiles.json"); }

        /// <summary>
        /// 首要配置文件内容
        /// </summary>
        public MainConfig MainConfig { get; set; }

        /// <summary>
        /// 官方用户配置文件内容
        /// </summary>
        public LauncherProfilesConfig LauncherProfilesConfig { get; set; }

        private object mainconfigLocker = new object();
        private object launcherProfilesLocker = new object();

        public ConfigHandler()
        {
            Directory = new DirectoryInfo("Config");

            if (!Directory.Exists)
            {
                Directory.Create();
            }

            if (!File.Exists(MainConfigPath))
            { NewConfig(); }
            else
            { Read(); }

            string profilesConfigDir = Path.GetDirectoryName(LauncherProfilesConfigPath);
            if (!System.IO.Directory.Exists(profilesConfigDir))
            {
                System.IO.Directory.CreateDirectory(profilesConfigDir);
            }
            if (!File.Exists(LauncherProfilesConfigPath))
            {
                NewProfilesConfig();
            }

        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void Save()
        {
            lock (mainconfigLocker)
            {
                try
                {
                    //Color_yr Change
                    File.WriteAllText(MainConfigPath, JsonConvert.SerializeObject(MainConfig, Formatting.Indented));
                }
                catch (UnauthorizedAccessException)
                {
                    var result = MessageBox.Show("启动器无法正常写入配置文件。\n" +
                        "这可能是由于您将启动器放置在系统敏感目录（如C盘，桌面等系统关键位置）\n" +
                        "而导致系统自我保护机制权限禁止写入文件。\n" +
                        "是否以管理员模式运行启动器？若拒绝则请自行移动到有权限的路径运行",
                        "启动器权限不足", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        App.Reboot(true);
                    }
                }
            }

        }

        /// <summary>
        /// 读配置文件
        /// </summary>
        public void Read()
        {
            lock (mainconfigLocker)
            {
                try
                {
                    MainConfig = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(MainConfigPath));
                }
                catch (UnauthorizedAccessException)
                {
                    var result = MessageBox.Show("启动器无法正常读取配置文件。\n" +
                        "这可能是由于您将启动器放置在系统敏感目录（如C盘，桌面等系统关键位置）\n" +
                        "而导致系统自我保护机制权限禁止写入文件。\n" +
                        "是否以管理员模式运行启动器？若拒绝则请自行移动到有权限的路径运行",
                        "启动器权限不足", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        App.Reboot(true);
                    }
                }
            }
        }

        /// <summary>
        /// 写配置文件
        /// </summary>
        public void SaveProfilesConfig()
        {
            lock (launcherProfilesLocker)
            {
                try
                {
                    File.WriteAllText(LauncherProfilesConfigPath, JsonConvert.SerializeObject(LauncherProfilesConfig));
                }
                catch (UnauthorizedAccessException)
                {
                    var result = MessageBox.Show("启动器无法正常写入配置文件。\n" +
                        "这可能是由于您将启动器放置在系统敏感目录（如C盘，桌面等系统关键位置）\n" +
                        "而导致系统自我保护机制权限禁止写入文件。\n" +
                        "是否以管理员模式运行启动器？若拒绝则请自行移动到有权限的路径运行",
                        "启动器权限不足", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        App.Reboot(true);
                    }
                }
            }
        }

        /// <summary>
        /// 新的官方用户配置文件
        /// </summary>
        private void NewProfilesConfig()
        {
            LauncherProfilesConfig = new LauncherProfilesConfig()
            {
                ClientToken = "88888888-8888-8888-8888-888888888888",
                SelectedProfile = "(Default)",
                Profiles = JObject.Parse("{\"(Default)\":{\"name\":\"(Default)\"}}")
            };
            SaveProfilesConfig();
        }

        /// <summary>
        /// 新建配置文件
        /// </summary>
        private void NewConfig()
        {
            MainConfig = new MainConfig()
            {
                User = new User()
                {
                    ClientToken = Guid.NewGuid().ToString("N"),
                    UserDatabase = new Dictionary<string, UserNode>(),
                    AuthenticationDic = new Dictionary<string, AuthenticationNode>()
                    {
                        {
                            "offline",
                            new AuthenticationNode()
                            {
                                AuthType = AuthenticationType.OFFLINE,
                                Name="离线验证"
                            }
                        },
                        {
                            "online",
                            new AuthenticationNode()
                            {
                                AuthType = AuthenticationType.MOJANG,
                                Name="正版验证"
                            }
                        }
                    }
                },
                History = new History(),
                Environment = new Environment()
                {
                    VersionIsolation = false,
                    AutoMemory = true,
                    GamePathType = GameDirEnum.ROOT,
                    DownloadLostAssets = true,
                    DownloadLostDepend = true,
                    GCEnabled = true,
                    GCType = NsisoLauncherCore.Modules.GCType.G1GC,
                    AutoJava = true,
                    WindowSize = new NsisoLauncherCore.Modules.WindowSize()
                    {
                        FullScreen = false
                    },
                    ExitAfterLaunch = false
                },
                Download = new Download()
                {
                    DownloadSource = DownloadSource.MCBBS,
                    DownloadThreadsSize = 5,
                    CheckDownloadFileHash = true
                },
                Launcher = new Launcher()
                {
                    Debug = false,
                    NoTracking = false
                },
                Server = new Server()
                {
                    ShowServerInfo = false,
                    LaunchToServer = false,
                    Port = 25565,
                    Updata_Check = new Updata_Check
                    {
                        Enable = false,
                        Vision = "0.0.0",
                        packname = "modpack"
                    }
                },
                Customize = new Customize()
                {
                    CustomBackGroundViode = false,
                    CustomBackGroundViode_Cyclic = false,
                    CustomBackGroundMusic = false,
                    CustomBackGroundMusic_Cyclic = false,
                    CustomBackGroundPicture = false,
                    CustomBackGroundPicture_Cyclic = false,
                    CustomBackGroundPicture_Cyclic_time = 10000,
                    AccentColor = "Blue",
                    AppThme = "BaseLight"
                },
                ConfigVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                Lauguage = "中文"
            };
            Save();
        }
    }
}
