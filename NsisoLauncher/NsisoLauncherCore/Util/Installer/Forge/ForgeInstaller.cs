﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Tools;
using NsisoLauncherCore.Util.Installer.Forge.Actions;
using NsisoLauncherCore.Util.Installer.Forge.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Installer.Forge
{
    /*
     * 感谢所有对这个.NET Forge安装器有贡献的开发者
     * 如果没有这些大佬的帮助，也就不会有这个Forge安装器
     * 
     * 特别感谢：
     * bangbang93 java方面及程序指导，以及下载服务器提供
     * []海螺螺 java方面指导
     * 33 java方面，安装思路指导
     * 喵喵喵？ 下载forge安装包和补全的时候一直用的是TA的服务器
     */
    public class DataProcessorsForgeInstaller
    {
        public string InstallerPath { get; set; }
        public CommonInstallOptions Options { get; set; }

        Install profile;

        public DataProcessorsForgeInstaller(string installerPath, CommonInstallOptions options)
        {
            if (string.IsNullOrWhiteSpace(installerPath))
            {
                throw new ArgumentException("Installer path can not be null or whitespace.");
            }
            this.InstallerPath = installerPath;
            this.Options = options ?? throw new ArgumentNullException("Install options is null");
        }

        public async Task BeginInstallFromJObject(ProgressCallback monitor, CancellationToken cancellationToken, JObject jObj, string tempPath)
        {
            profile = jObj.ToObject<Install>();

            string target = Options.GameRootPath;
            if (!Directory.Exists(target))
            {
                throw new DirectoryNotFoundException("The minecraft root is not found");
            }

            //I think we dont need to inject the launcher profiles, so we dont need this json :)
            //string launcherProfiles = Path.Combine(target, "launcher_profiles.json");
            //if (!File.Exists(launcherProfiles))
            //{
            //    throw new FileNotFoundException("There is no minecraft launcher profile");
            //}

            string versionRoot = Path.Combine(target, "versions");
            string librariesDir = Path.Combine(target, "libraries");
            if (!Directory.Exists(librariesDir))
            {
                Directory.CreateDirectory(librariesDir);
            }

            //Extracting json
            monitor.SetState("提取json");
            string jsonPath = PathManager.GetJsonPath(Options.GameRootPath, profile.Version);
            if (!Directory.Exists(Path.GetDirectoryName(jsonPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            }
            File.Copy(tempPath + profile.Json, jsonPath, true);

            //Consider minecraft client jar
            monitor.SetState("检查游戏文件");
            string clientTarget = PathManager.GetJarPath(Options.GameRootPath, profile.Minecraft);
            if (!File.Exists(PathManager.GetJsonPath(Options.GameRootPath, profile.Minecraft)))
            {
                throw new FileNotFoundException("Minecraft json is not exists");
            }
            if (!File.Exists(clientTarget))
            {
                throw new FileNotFoundException("Minecraft jar is not exists");
            }

            var exc = await DownloadUtils.DownloadForgeJLibrariesAsync(monitor, Options.DownloadSource, cancellationToken, profile.Libraries, librariesDir);
            if (exc != null)
            {
                throw exc;
            }

            string[] mavenFolders = Directory.GetDirectories(tempPath + "\\maven");
            foreach (var item in mavenFolders)
            {
                DirectoryInfo info = new DirectoryInfo(item);
                FileHelper.CopyDirectory(item, librariesDir + '\\' + info.Name, true);
            }

            PostProcessors postProcessors = new PostProcessors(profile, Options.IsClient, monitor);
            Exception procExc = postProcessors.Process(tempPath, Options.GameRootPath, clientTarget, Options.Java);
            if (procExc != null)
            {
                throw procExc;
            }
        }
    }

    public class ForgeInstaller : IInstaller
    {
        public string InstallerPath { get; set; }
        public CommonInstallOptions Options { get; set; }

        public ForgeInstaller(string installerPath, CommonInstallOptions options)
        {
            if (string.IsNullOrWhiteSpace(installerPath))
            {
                throw new ArgumentException("Installer path can not be null or whitespace.");
            }
            this.InstallerPath = installerPath;
            this.Options = options ?? throw new ArgumentNullException("Install options is null");
        }

        public async void BeginInstall(ProgressCallback monitor, CancellationToken cancellationToken)
        {
            CommonInstaller.IsInstal = true;
            string installerName = Path.GetFileNameWithoutExtension(InstallerPath);
            string tempPath = string.Format("{0}\\{1}Temp", PathManager.TempDirectory, installerName);
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);
            Unzip.UnZipFile(InstallerPath, tempPath);
            string mainJson = File.ReadAllText(tempPath + "\\install_profile.json");
            JObject jObject = JObject.Parse(mainJson);

            if (jObject.ContainsKey("install") && jObject.ContainsKey("versionInfo"))
            {
                CommonInstaller commonInstaller = new CommonInstaller(InstallerPath, Options);
                await commonInstaller.BeginInstallFromJObject(monitor, cancellationToken, jObject, tempPath);
            }
            else if (jObject.ContainsKey("data") && jObject.ContainsKey("processors") && jObject.ContainsKey("libraries"))
            {
                DataProcessorsForgeInstaller forgeInstaller = new DataProcessorsForgeInstaller(InstallerPath, Options);
                await forgeInstaller.BeginInstallFromJObject(monitor, cancellationToken, jObject, tempPath);
            }
            else
            {
                throw new JsonException("JSON has no matching format");
            }

            Directory.Delete(tempPath, true);
            File.Delete(InstallerPath);
            CommonInstaller.IsInstal = false;
        }
    }
}
