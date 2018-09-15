﻿using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            //else
            //{
            //    ReadProfilesConfig();
            //}

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
                    File.WriteAllText(MainConfigPath, JsonConvert.SerializeObject(MainConfig));
                }
                catch (UnauthorizedAccessException)
                {
                    var result = MessageBox.Show("启动器无法正常写入配置文件。\n" +
                        "这可能是由于您将启动器放置在系统敏感目录（如C盘，桌面等系统关键位置）\n" +
                        "而导致系统自我保护机制权限禁止写入文件。\n" +
                        "是否以管理员模式运行启动器？若拒绝则请自行移动到有权限的路径运行",
                        "启动器权限不足", MessageBoxButton.YesNo);
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
                        "启动器权限不足", MessageBoxButton.YesNo);
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
                    var result = MessageBox.Show("启动器无法正常读取配置文件。\n" +
                        "这可能是由于您将启动器放置在系统敏感目录（如C盘，桌面等系统关键位置）\n" +
                        "而导致系统自我保护机制权限禁止写入文件。\n" +
                        "是否以管理员模式运行启动器？若拒绝则请自行移动到有权限的路径运行",
                        "启动器权限不足", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        App.Reboot(true);
                    }
                }
            }
        }

        //public void ReadProfilesConfig()
        //{
        //    lock (launcherProfilesLocker)
        //    {
        //        LauncherProfilesConfig = JsonConvert.DeserializeObject<LauncherProfilesConfig>(File.ReadAllText(MainConfigPath));
        //    }
        //}

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
                    AuthenticationType = AuthenticationType.OFFLINE
                },
                History = new History()
                {

                },
                Environment = new Environment()
                {
                    VersionIsolation = false,
                    AutoMemory = true,
                    GamePathType = GameDirEnum.ROOT,
                    DownloadLostAssets = true,
                    DownloadLostDepend = true,
                    GCEnabled = true,
                    GCType = Core.Modules.GCType.G1GC,
                    AutoJava = true,
                    WindowSize = new Core.Modules.WindowSize() { FullScreen = false },
                    ExitAfterLaunch = false
                },
                Download = new Download()
                {
                    DownloadSource = Core.Net.DownloadSource.Mojang,
                    DownloadThreadsSize = 5
                },
                Launcher = new Launcher()
                {
                    Debug = false
                },
                Server = new Server()
                {
                    ShowServerInfo = false,
                    LaunchToServer = false,
                    Port = 25565
                },
                Customize = new Customize()
                {
                    CustomBackGroundMusic = false,
                    CustomBackGroundPicture = false,
                    AccentColor = "Blue",
                    AppThme = "BaseLight"
                }
            };
            Save();
        }
    }
}
