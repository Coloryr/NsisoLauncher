﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncher.Config.ConfigObj;
using NsisoLauncherCore;
using NsisoLauncherCore.Net;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Windows;

namespace NsisoLauncher.Config
{
    public class ConfigHandler : IDisposable
    {
        /// <summary>
        /// 首要配置文件路径
        /// </summary>
        public string MainConfigPath { get => PathManager.BaseStorageDirectory + @"\MainConfig.json"; }

        /// <summary>
        /// 官方用户配置文件路径
        /// </summary>
        public string LauncherProfilesConfigPath { get => Path.GetFullPath(@".minecraft\launcher_profiles.json"); }

        /// <summary>
        /// 首要配置文件内容
        /// </summary>
        public MainConfigObj MainConfig { get; set; }

        /// <summary>
        /// 官方用户配置文件内容
        /// </summary>
        public LauncherProfilesConfigObj LauncherProfilesConfig { get; set; }

        private ReaderWriterLockSlim mainconfigLock = new();
        private ReaderWriterLockSlim launcherProfilesLock = new();

        public ConfigHandler()
        {
            try
            {
                if (!File.Exists(MainConfigPath))
                { NewConfig(); }
                else
                { Read(); }
                if (!File.Exists(LauncherProfilesConfigPath))
                { NewProfilesConfig(); }
            }
            catch (UnauthorizedAccessException e)
            {
                NoAccessWarning(e);
            }
            catch (SecurityException e)
            {
                NoAccessWarning(e);
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void Save()
        {
            mainconfigLock.EnterWriteLock();
            try
            {
                string dir = Path.GetDirectoryName(MainConfigPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(MainConfigPath, JsonConvert.SerializeObject(MainConfig, Formatting.Indented));
            }
            catch (UnauthorizedAccessException e)
            {
                NoAccessWarning(e);
            }
            catch (System.Security.SecurityException e)
            {
                NoAccessWarning(e);
            }
            finally
            {
                mainconfigLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 读配置文件
        /// </summary>
        public void Read()
        {
            mainconfigLock.EnterReadLock();
            try
            {
                MainConfig = JsonConvert.DeserializeObject<MainConfigObj>(File.ReadAllText(MainConfigPath));
            }
            catch (UnauthorizedAccessException e)
            {
                NoAccessWarning(e);
            }
            catch (SecurityException e)
            {
                NoAccessWarning(e);
            }
            finally
            {
                mainconfigLock.ExitReadLock();
            }
        }

        private void NoAccessWarning(Exception e)
        {
            var result = MessageBox.Show("启动器无法正常读/写配置文件。\n" +
                    "这可能是由于您将启动器放置在系统敏感目录（如C盘，桌面等系统关键位置）\n" +
                    "或您没有足够的系统操作权限而导致系统自我保护机制，禁止启动器读写文件。\n" +
                    "是否以管理员模式运行启动器？若拒绝则请自行移动到有权限的路径运行。\n" +
                    "详细信息:\n" +
                    e.ToString(),
                    "启动器权限不足", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                App.Reboot(true);
            }
        }

        /// <summary>
        /// 写配置文件
        /// </summary>
        public void SaveProfilesConfig()
        {
            launcherProfilesLock.EnterWriteLock();
            try
            {
                string profilesConfigDir = Path.GetDirectoryName(LauncherProfilesConfigPath);
                if (!Directory.Exists(profilesConfigDir))
                {
                    Directory.CreateDirectory(profilesConfigDir);
                }
                File.WriteAllText(LauncherProfilesConfigPath, JsonConvert.SerializeObject(LauncherProfilesConfig));
            }
            catch (UnauthorizedAccessException e)
            {
                NoAccessWarning(e);
            }
            catch (SecurityException e)
            {
                NoAccessWarning(e);
            }
            finally
            {
                launcherProfilesLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 新的官方用户配置文件
        /// </summary>
        private void NewProfilesConfig()
        {
            LauncherProfilesConfig = new LauncherProfilesConfigObj()
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
            MainConfig = new()
            {
                User = new()
                {
                    ClientToken = Guid.NewGuid().ToString("N"),
                    UserDatabase = new(),
                    AuthenticationDic = new()
                    {
                        {
                            "offline",
                            new()
                            {
                                AuthType = AuthenticationType.OFFLINE
                            }
                        },
                        {
                            "online",
                            new()
                            {
                                AuthType = AuthenticationType.MOJANG
                            }
                        },
                        {
                            "Microsoft",
                            new()
                            {
                                AuthType = AuthenticationType.MICROSOFT
                            }
                        }
                    }
                },
                History = new(),
                Environment = new()
                {
                    VersionIsolation = false,
                    MaxMemory = 2048,
                    MinMemory = 512,
                    GamePathType = GameDirEnum.ROOT,
                    DownloadLostAssets = true,
                    DownloadLostDepend = true,
                    GCEnabled = true,
                    GCType = NsisoLauncherCore.Modules.GCType.G1GC,
                    AutoJava = true,
                    WindowSize = new()
                    {
                        FullScreen = false,
                        Height = 0,
                        Width = 0
                    },
                    ExitAfterLaunch = false,
                    AdvencedJvmArguments = "-XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow"
                },
                Download = new()
                {
                    DownloadSource = DownloadSource.MCBBS,
                    DownloadThreadsSize = 5,
                    CheckDownloadFileHash = true
                },
                Launcher = new()
                {
                    Debug = false,
                    NoTracking = false
                },
                Server = new()
                {
                    ShowServerInfo = false,
                    LaunchToServer = false,
                    Port = 25565,
                    UpdataCheck = new()
                    {
                        Enable = false,
                        Version = "0.0.0",
                        Packname = "modpack"
                    }
                },
                Customize = new()
                {
                    CustomBackGroundVedio = false,
                    CustomBackGroundVedioCyclic = false,
                    CustomBackGroundMusic = false,
                    CustomBackGroundMusicCyclic = false,
                    CustomBackGroundPicture = false,
                    CustomBackGroundPictureCyclic = false,
                    CustomBackGroundPictureCyclicTime = 10000,
                    CustomBackGroundPictureBlurEffect = false,
                    CustomBackGroundPictureBlurEffectSize = 5,
                    AppThme = "Light.Blue"
                },
                ConfigVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                Lauguage = "中文"
            };
            Save();
        }

        public void Dispose()
        {
            mainconfigLock.Dispose();
            launcherProfilesLock.Dispose();
        }
    }
}
