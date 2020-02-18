using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Tools;
using System.Collections.Generic;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace NsisoLauncherCore.Util
{
    public static class FileHelper
    {
        #region 文件工具

        public static bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting)
        {
            bool ret;
            try
            {
                SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
                DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(DestinationPath) == false)
                        Directory.CreateDirectory(DestinationPath);

                    foreach (string fls in Directory.GetFiles(SourcePath))
                    {
                        FileInfo flinfo = new FileInfo(fls);
                        flinfo.CopyTo(DestinationPath + flinfo.Name, overwriteexisting);
                    }
                    foreach (string drs in Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo drinfo = new DirectoryInfo(drs);
                        if (CopyDirectory(drs, DestinationPath + drinfo.Name, overwriteexisting) == false)
                            ret = false;
                    }
                }
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        #endregion

        #region 检查Jar核心文件
        public static bool IsLostJarCore(LaunchHandler core, MCVersion version)
        {
            if (version.InheritsVersion == null)
            {
                string jarPath = core.GetJarPath(version);
                return !File.Exists(jarPath);
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region 检查Libs库文件
        /// <summary>
        /// 获取版本是否丢失任何库文件
        /// </summary>
        /// <param name="core">启动核心</param>
        /// <param name="version">检查的版本</param>
        /// <returns>是否丢失任何库文件</returns>
        public static bool IsLostAnyLibs(LaunchHandler core, MCVersion version)
        {
            foreach (var item in version.Libraries)
            {
                string path = core.GetLibraryPath(item);
                if (!File.Exists(path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取版本丢失的库文件
        /// </summary>
        /// <param name="core">所使用的启动核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为库实例的集合</returns>
        public static Dictionary<string, Modules.Library> GetLostLibs(LaunchHandler core, MCVersion version)
        {
            Dictionary<string, Modules.Library> lostLibs = new Dictionary<string, Modules.Library>();

            foreach (var item in version.Libraries)
            {
                string path = core.GetLibraryPath(item);
                if (lostLibs.ContainsKey(path))
                {
                    continue;
                }
                else if (!File.Exists(path))
                {
                    lostLibs.Add(path, item);
                }
            }
            return lostLibs;
        }
        #endregion

        #region 检查Natives本机文件
        /// <summary>
        /// 获取版本是否丢失任何natives文件
        /// </summary>
        /// <param name="core"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool IsLostAnyNatives(LaunchHandler core, MCVersion version)
        {
            foreach (var item in version.Natives)
            {
                string path = core.GetNativePath(item);
                if (!File.Exists(path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取版本丢失的natives文件
        /// </summary>
        /// <param name="core">所使用的核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为native实例的集合</returns>
        public static Dictionary<string, Native> GetLostNatives(LaunchHandler core, MCVersion version)
        {
            Dictionary<string, Native> lostNatives = new Dictionary<string, Native>();

            foreach (var item in version.Natives)
            {
                string path = core.GetNativePath(item);
                if (lostNatives.ContainsKey(path))
                {
                    continue;
                }
                else if (!File.Exists(path))
                {
                    lostNatives.Add(path, item);
                }
            }
            return lostNatives;
        }
        #endregion

        #region 检查Assets资源文件
        private static bool IsLostAnyAssetsFromJassets(LaunchHandler core, JAssets assets)
        {
            if (assets == null)
            {
                return false;
            }
            foreach (var item in assets.Objects)
            {

                string path = core.GetAssetsPath(item.Value);
                if (!File.Exists(path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取版本丢失的资源文件
        /// </summary>
        /// <param name="core">所使用的核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为资源文件信息实例的集合</returns>
        public static Dictionary<string, JAssetsInfo> GetLostAssets(LaunchHandler core, JAssets assets)
        {
            Dictionary<string, JAssetsInfo> lostAssets = new Dictionary<string, JAssetsInfo>();
            if (assets == null)
                return lostAssets;
            foreach (var item in assets.Objects)
            {

                string path = core.GetAssetsPath(item.Value);
                if ((!lostAssets.ContainsKey(path)) && (!File.Exists(path)))
                    lostAssets.Add(path, item.Value);
            }
            return lostAssets;
        }

        public static async Task<bool> IsLostAssetsAsync(DownloadSource source, LaunchHandler core, MCVersion ver)
        {
            string assetsPath = core.GetAssetsIndexPath(ver.Assets);
            if (!File.Exists(assetsPath))
            {
                return true;
            }
            else
            {
                var assets = await core.GetAssetsAsync(ver);
                return await Task.Factory.StartNew(() =>
                {
                    return IsLostAnyAssetsFromJassets(core, assets);
                });
            }
        }
        #endregion

        #region 丢失依赖文件帮助

        /// <summary>
        /// 获取全部丢失的文件下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="core">使用的核心</param>
        /// <param name="version">检查的版本</param>
        /// <returns></returns>
        public async static Task<List<DownloadTask>> GetLostDependDownloadTaskAsync(DownloadSource source, LaunchHandler core, MCVersion version, MetroWindow window)
        {
            var lostLibs = GetLostLibs(core, version);
            var lostNatives = GetLostNatives(core, version);
            List<DownloadTask> tasks = new List<DownloadTask>();
            if (IsLostJarCore(core, version))
            {
                if (version.Jar == null)
                {
                    tasks.Add(GetDownloadUrl.GetCoreJarDownloadTask(source, version, core));
                }
            }

            if (version.InheritsVersion != null)
            {
                string innerJsonPath = core.GetJsonPath(version.InheritsVersion);
                string innerJsonStr;
                if (!File.Exists(innerJsonPath))
                {
                    var http = new HttpRequesterAPI(TimeSpan.FromSeconds(10));
                    innerJsonStr = await http.HttpGetStringAsync(GetDownloadUrl.GetCoreJsonDownloadURL(source, version.InheritsVersion));
                    if(innerJsonStr == null)
                    {
                        await window.ShowMessageAsync("检查错误", "检查源错误，请切换下载源后重试");
                        return new List<DownloadTask>();
                    }
                    string jsonFolder = Path.GetDirectoryName(innerJsonPath);
                    if (!Directory.Exists(jsonFolder))
                    {
                        Directory.CreateDirectory(jsonFolder);
                    }
                    File.WriteAllText(innerJsonPath, innerJsonStr);
                }
                else
                {
                    innerJsonStr = File.ReadAllText(innerJsonPath);
                }
                MCVersion innerVer = core.JsonToVersion(innerJsonStr);
                if (innerVer != null)
                {
                    tasks.AddRange(await GetLostDependDownloadTaskAsync(source, core, innerVer, window));
                }

            }
            foreach (var item in lostLibs)
            {
                tasks.Add(GetDownloadUrl.GetLibDownloadTask(source, item));
            }
            foreach (var item in lostNatives)
            {
                tasks.Add(GetDownloadUrl.GetNativeDownloadTask(source, item));
            }
            return tasks;
        }
        #endregion


        /// <summary>
        /// 获取丢失的资源文件下载任务
        /// </summary>
        /// <param name="source"></param>
        /// <param name="core"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async static Task<List<DownloadTask>> GetLostAssetsDownloadTaskAsync(DownloadSource source, LaunchHandler core, MCVersion ver)
        {
            List<DownloadTask> tasks = new List<DownloadTask>();
            JAssets assets = null;

            string assetsPath = core.GetAssetsIndexPath(ver.Assets);
            if (!File.Exists(assetsPath))
            {
                if (ver.AssetIndex != null)
                {
                    var http = new HttpRequesterAPI(TimeSpan.FromSeconds(10));
                    string jsonUrl = GetDownloadUrl.DoURLReplace(source, ver.AssetIndex.URL);
                    string assetsJson = await http.HttpGetStringAsync(jsonUrl);
                    assets = core.GetAssetsByJson(assetsJson);
                    tasks.Add(new DownloadTask("资源文件引导", jsonUrl, assetsPath));
                }
                else
                {
                    return tasks;
                }
            }
            else
            {
                assets = core.GetAssets(ver);
            }
            var lostAssets = GetLostAssets(core, assets);
            foreach (var item in lostAssets)
            {
                DownloadTask task = GetDownloadUrl.GetAssetsDownloadTask(source, item.Value, core);
                tasks.Add(task);
            }
            return tasks;
        }
    }
}
