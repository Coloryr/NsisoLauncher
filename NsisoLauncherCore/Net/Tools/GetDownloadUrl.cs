﻿using NsisoLauncher;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.FunctionAPI;
using NsisoLauncherCore.Util;
using NsisoLauncherCore.Util.Checker;
using NsisoLauncherCore.Util.Installer;
using NsisoLauncherCore.Util.Installer.Fabric;
using NsisoLauncherCore.Util.Installer.Forge;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Tools
{
    public static class GetDownloadUrl
    {
        public const string BMCLUrl = "https://bmclapi2.bangbang93.com/";
        public const string BMCLLibrariesURL = BMCLUrl + "libraries/";
        public const string BMCLVersionURL = BMCLUrl + "mc/game/version_manifest.json";
        public const string BMCLAssetsURL = BMCLUrl + "objects/";

        public const string MCBBSUrl = "https://download.mcbbs.net/";
        public const string MCBBSLibrariesURL = MCBBSUrl + "libraries/";
        public const string MCBBSVersionURL = MCBBSUrl + "mc/game/version_manifest.json";
        public const string MCBBSAssetsURL = MCBBSUrl + "objects/";

        public const string MojangMainUrl = "https://launcher.mojang.com/";
        public const string MojangMetaUrl = "https://launchermeta.mojang.com/";
        public const string MojangVersionUrl = MojangMetaUrl + "mc/game/version_manifest.json";
        public const string MojanglibrariesUrl = "https://libraries.minecraft.net/";
        public const string MojangAssetsBaseUrl = "https://resources.download.minecraft.net/";

        public const string ForgeUrl = "https://files.minecraftforge.net/";

        static Dictionary<string, string> bmclapiDic = new Dictionary<string, string>()
        {
            {MojangVersionUrl, BMCLVersionURL },
            {MojangMainUrl, BMCLUrl },
            {MojangMetaUrl, BMCLUrl },
            {MojanglibrariesUrl, BMCLLibrariesURL },
            {MojangAssetsBaseUrl, BMCLAssetsURL },
            {@"http://files.minecraftforge.net/maven/", BMCLLibrariesURL },
            {@"https://files.minecraftforge.net/maven/", BMCLLibrariesURL }
        };

        static Dictionary<string, string> mcbbsDic = new Dictionary<string, string>()
        {
            {MojangVersionUrl, MCBBSVersionURL },
            {MojangMainUrl, MCBBSUrl },
            {MojangMetaUrl, MCBBSUrl },
            {MojanglibrariesUrl, MCBBSLibrariesURL },
            {MojangAssetsBaseUrl, MCBBSAssetsURL },
            {@"http://files.minecraftforge.net/maven/", MCBBSLibrariesURL },
            {@"https://files.minecraftforge.net/maven/", MCBBSLibrariesURL }
        };

        public static string DoURLReplace(DownloadSource source, string url)
        {
            switch (source)
            {
                case DownloadSource.Mojang:
                    return url;

                case DownloadSource.BMCLAPI:
                    return ReplaceURLByDic(url, bmclapiDic);

                case DownloadSource.MCBBS:
                    return ReplaceURLByDic(url, mcbbsDic);

                default:
                    return url;
            }
        }

        private static string ReplaceURLByDic(string str, Dictionary<string, string> dic)
        {
            string ret = str;
            foreach (var item in dic)
            {
                ret = ret.Replace(item.Key, item.Value);
            }
            return ret;
        }

        private static string GetLibBasePath(Library lib)
        {
            if (!string.IsNullOrWhiteSpace(lib.LibDownloadInfo?.Path))
            {
                return lib.LibDownloadInfo.Path;
            }
            else
            {
                return string.Format(@"{0}\{1}\{2}\{1}-{2}.jar", lib.Artifact.Package.Replace(".", "\\"), lib.Artifact.Name, lib.Artifact.Version);
            }
        }

        private static string GetNativeBasePath(Native native)
        {
            if (!string.IsNullOrWhiteSpace(native.NativeDownloadInfo?.Path))
            {
                return native.NativeDownloadInfo.Path;
            }
            else
            {
                return string.Format(@"{0}\{1}\{2}\{1}-{2}-{3}.jar", native.Artifact.Package.Replace(".", "\\"), native.Artifact.Name, native.Artifact.Version, native.NativeSuffix);
            }
        }

        private static string GetAssetsBasePath(JAssetsInfo assetsInfo)
        {
            return string.Format(@"{0}\{1}", assetsInfo.Hash.Substring(0, 2), assetsInfo.Hash);
        }

        public static string GetCoreJsonDownloadURL(DownloadSource source, string verID)
        {
            switch (source)
            {
                case DownloadSource.Mojang:
                    return string.Format("{0}version/{1}/json", MojangMainUrl, verID);
                case DownloadSource.BMCLAPI:
                    return string.Format("{0}version/{1}/json", BMCLUrl, verID);
                case DownloadSource.MCBBS:
                    return string.Format("{0}version/{1}/json", MCBBSUrl, verID);
                default:
                    return string.Format("{0}version/{1}/json", MojangMainUrl, verID);
            }
        }

        public static DownloadTask GetCoreJsonDownloadTask(DownloadSource downloadSource, string verID, LaunchHandler core)
        {
            string to = core.GetJsonPath(verID);
            string from = GetCoreJsonDownloadURL(downloadSource, verID);
            return new DownloadTask("游戏版本核心Json文件", from, to);
        }

        public static string GetCoreJarDownloadURL(DownloadSource source, Modules.MCVersion ver)
        {
            if (ver.Downloads?.Client != null)
            {
                return DoURLReplace(source, ver.Downloads.Client.URL);
            }
            else
            {
                return string.Format("{0}version/{1}/client", BMCLUrl, ver.ID);
            }
        }

        /// <summary>
        /// 获取游戏核心下载
        /// </summary>
        /// <param name="downloadSource">下载源</param>
        /// <param name="version">版本</param>
        /// <param name="core">核心</param>
        /// <returns></returns>
        public static DownloadTask GetCoreJarDownloadTask(DownloadSource downloadSource, MCVersion version, LaunchHandler core)
        {
            string to = core.GetJarPath(version);
            string from = GetCoreJarDownloadURL(downloadSource, version);
            DownloadTask downloadTask = new DownloadTask("游戏版本核心Jar文件", from, to);
            if (!string.IsNullOrWhiteSpace(version.Downloads?.Client?.SHA1))
            {
                downloadTask.Checker = new SHA1Checker() { CheckSum = version.Downloads.Client.SHA1, FilePath = to };
            }
            return downloadTask;
        }

        /// <summary>
        /// 获取Forge下载
        /// </summary>
        /// <param name="downloadSource">下载源</param>
        /// <param name="mcversion">Mc版本</param>
        /// <param name="forgeversion">Forge版本</param>
        /// <returns></returns>
        public static DownloadTask GetForgeDownloadURL(DownloadSource downloadSource, string mcversion, string forgeversion)
        {
            string local = App.Handler.GameRootPath + "\\forge-" + mcversion + "-" + forgeversion + "-installer.jar";
            string forgePath = string.Format("maven/net/minecraftforge/forge/{0}-{1}/forge-{0}-{1}-installer.jar", mcversion, forgeversion);
            string Source = ForgeUrl;
            switch (downloadSource)
            {
                case DownloadSource.Mojang:
                    Source = ForgeUrl;
                    break;
                case DownloadSource.BMCLAPI:
                    Source = BMCLUrl;
                    break;
                case DownloadSource.MCBBS:
                    Source = MCBBSUrl;
                    break;
            }

            Source += forgePath;

            DownloadTask dt = new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Core.Forge"),
                Source, local);
            dt.Todo = new Func<ProgressCallback, CancellationToken, Exception>((callback, cancelToken) =>
            {
                try
                {
                    IInstaller installer = new ForgeInstaller(local, new CommonInstallOptions()
                    {
                        GameRootPath = App.Handler.GameRootPath,
                        IsClient = true,
                        DownloadSource = App.Config.MainConfig.Download.DownloadSource,
                        Java = App.Handler.Java
                    });
                    installer.BeginInstall(callback, cancelToken);
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            return dt;
        }

        /// <summary>
        /// 获取Forge下载
        /// </summary>
        /// <param name="downloadSource">下载源</param>
        /// <param name="forge">Forge信息</param>
        /// <returns></returns>
        public static DownloadTask GetForgeDownloadURL(DownloadSource downloadSource, APIModules.JWForge forge)
        {
            string local = PathManager.TempDirectory + "\\forge-" + forge.Build + ".jar";
            string Source = BMCLUrl;
            switch (downloadSource)
            {
                case DownloadSource.Mojang:
                    Source = BMCLUrl;
                    break;
                case DownloadSource.BMCLAPI:
                    Source = BMCLUrl;
                    break;
                case DownloadSource.MCBBS:
                    Source = MCBBSUrl;
                    break;
            }

            Source += "forge/download/" + forge.Build + ".jar";

            DownloadTask dt = new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Core.Forge"),
                Source, local);
            dt.Todo = new Func<ProgressCallback, CancellationToken, Exception>((callback, cancelToken) =>
            {
                try
                {
                    IInstaller installer = new ForgeInstaller(local, new CommonInstallOptions()
                    {
                        GameRootPath = App.Handler.GameRootPath,
                        IsClient = true,
                        DownloadSource = App.Config.MainConfig.Download.DownloadSource,
                        Java = App.Handler.Java
                    });
                    installer.BeginInstall(callback, cancelToken);
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            return dt;
        }

        /// <summary>
        /// 获取Fabric下载
        /// </summary>
        /// <param name="downloadSource">下载源</param>
        /// <param name="forge">Forge信息</param>
        /// <returns></returns>
        public static DownloadTask GetFabricDownloadURL(DownloadSource downloadSource, APIModules.JWFabric fabric, MCVersion version)
        {
            string local = PathManager.TempDirectory + "\\fabric-installer-0.6.1.45.jar";

            DownloadTask dt = new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Core.Fabric"),
                @"https://maven.fabricmc.net/net/fabricmc/fabric-installer/0.6.1.45/fabric-installer-0.6.1.45.jar", local);
            dt.Todo = new Func<ProgressCallback, CancellationToken, Exception>((callback, cancelToken) =>
            {
                try
                {
                    IInstaller installer = new FabricInstaller(local, new CommonInstallOptions()
                    {
                        GameRootPath = App.Handler.GameRootPath,
                        IsClient = true,
                        DownloadSource = App.Config.MainConfig.Download.DownloadSource,
                        Java = App.Handler.Java,
                        obj = new APIModules.TwoObj
                        {
                            fabric = fabric,
                            version = version
                        }
                    });
                    installer.BeginInstall(callback, cancelToken);
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            return dt;
        }

        /// <summary>
        /// 获取Liteloader mod
        /// </summary>
        /// <param name="downloadSource">下载源</param>
        /// <param name="liteloader">liteloader信息</param>
        /// <returns></returns>
        public static DownloadTask GetLiteloaderDownloadURL(DownloadSource downloadSource, APIModules.JWLiteloader liteloader)
        {
            string local = App.Handler.GameRootPath + "\\mods\\" + liteloader.Version + ".jar";
            string Source = BMCLUrl;
            switch (downloadSource)
            {
                case DownloadSource.Mojang:
                    Source = BMCLUrl;
                    break;
                case DownloadSource.BMCLAPI:
                    Source = BMCLUrl;
                    break;
                case DownloadSource.MCBBS:
                    Source = MCBBSUrl;
                    break;
            }

            Source += string.Format("maven/com/mumfrey/liteloader/{0}/liteloader-{0}", liteloader.Version) + ".jar";

            DownloadTask dt = new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Core.Liteloader"),
                Source, local);
            return dt;
        }

        /// <summary>
        /// 获取Lib下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="lib">lib实例</param>
        /// <returns>下载URL</returns>
        public static string GetLibDownloadURL(DownloadSource source, Library lib)
        {
            if (!string.IsNullOrWhiteSpace(lib.LibDownloadInfo?.URL))
            {
                return DoURLReplace(source, lib.LibDownloadInfo.URL);
            }
            else
            {
                string libUrlPath = GetLibBasePath(lib).Replace('\\', '/');
                if (lib.Url != null)
                {
                    return DoURLReplace(source, lib.Url) + libUrlPath;
                }
                else
                {
                    switch (source)
                    {
                        case DownloadSource.Mojang:
                            return MojanglibrariesUrl + libUrlPath;

                        case DownloadSource.BMCLAPI:
                            return BMCLLibrariesURL + libUrlPath;

                        case DownloadSource.MCBBS:
                            return MCBBSLibrariesURL + libUrlPath;

                        default:
                            throw new ArgumentNullException("source");

                    }
                }
            }
        }

        /// <summary>
        /// 获取Lib下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="lib">lib实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetLibDownloadTask(DownloadSource source, KeyValuePair<string, Library> lib)
        {
            string from = GetLibDownloadURL(source, lib.Value);
            string to = lib.Key;
            DownloadTask task = new DownloadTask("版本依赖库文件" + lib.Value.Artifact.Name, from, to);
            if (lib.Value.LibDownloadInfo != null)
            {
                task.Checker = new SHA1Checker() { CheckSum = lib.Value.LibDownloadInfo.SHA1, FilePath = to };
            }
            return task;
        }

        /// <summary>
        /// 获取native下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="native">native实例</param>
        /// <returns>下载URL</returns>
        public static string GetNativeDownloadURL(DownloadSource source, Native native)
        {
            if (!string.IsNullOrWhiteSpace(native.NativeDownloadInfo?.URL))
            {
                return DoURLReplace(source, native.NativeDownloadInfo.URL);
            }
            else
            {
                switch (source)
                {
                    case DownloadSource.Mojang:
                        return (MojanglibrariesUrl + GetNativeBasePath(native)).Replace('\\', '/');

                    case DownloadSource.BMCLAPI:
                        return (BMCLLibrariesURL + GetNativeBasePath(native)).Replace('\\', '/');

                    case DownloadSource.MCBBS:
                        return (MCBBSLibrariesURL + GetNativeBasePath(native)).Replace('\\', '/');
                    default:
                        throw new ArgumentNullException("source");

                }
            }
        }

        /// <summary>
        /// 获取Native下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="native">native实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetNativeDownloadTask(DownloadSource source, KeyValuePair<string, Native> native)
        {
            string from = GetNativeDownloadURL(source, native.Value);
            string to = native.Key;
            DownloadTask task = new DownloadTask("版本系统依赖库文件" + native.Value.Artifact.Name, from, to);
            if (native.Value.NativeDownloadInfo != null)
            {
                task.Checker = new SHA1Checker() { CheckSum = native.Value.NativeDownloadInfo.SHA1, FilePath = to };
            }
            return task;
        }

        /// <summary>
        /// 获取assets下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="assets">assets实例</param>
        /// <returns>下载URL</returns>
        public static string GetAssetsDownloadURL(DownloadSource source, JAssetsInfo assets)
        {
            switch (source)
            {
                case DownloadSource.Mojang:
                    return (MojangAssetsBaseUrl + GetAssetsBasePath(assets)).Replace('\\', '/');

                case DownloadSource.BMCLAPI:
                    return (BMCLUrl + "objects\\" + GetAssetsBasePath(assets)).Replace('\\', '/');

                case DownloadSource.MCBBS:
                    return (MCBBSUrl + "objects\\" + GetAssetsBasePath(assets)).Replace('\\', '/');
                default:
                    throw new ArgumentNullException("source");

            }
        }

        /// <summary>
        /// 获取assets下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="assets">assets实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetAssetsDownloadTask(DownloadSource source, JAssetsInfo assets, LaunchHandler core)
        {
            string from = GetAssetsDownloadURL(source, assets);
            string to = core.GetAssetsPath(assets);
            return new DownloadTask("游戏资源文件" + assets.Hash, from, to);
        }

        /// <summary>
        /// 获取NIDE8核心下载任务
        /// </summary>
        /// <param name="downloadTo">下载目的路径</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetNide8CoreDownloadTask(string downloadTo)
        {
            return new DownloadTask("统一通行证核心", "https://login2.nide8.com:233/index/jar", downloadTo);
        }

        public async static Task<DownloadTask> GetAICoreDownloadTask(DownloadSource source, string downloadTo)
        {
            AuthlibInjectorAPI.APIHandler handler = new AuthlibInjectorAPI.APIHandler();
            return await handler.GetLatestAICoreDownloadTask(source, downloadTo);
        }
    }
}
