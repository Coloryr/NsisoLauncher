﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Installer
{
    public class CommonJsonObj
    {
        [JsonProperty("install")]
        public InstallInfo Install { get; set; }

        [JsonProperty("versionInfo")]
        public JObject VersionInfo { get; set; }

        public class InstallInfo
        {
            [JsonProperty("target")]
            public string Target { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("filePath")]
            public string FilePath { get; set; }
        }
    }

    public class CommonInstallOptions
    {
        public string GameRootPath { get; set; }

        public bool IsClient { get; set; }

        public DownloadSource DownloadSource { get; set; }

        public Java Java { get; set; }

        public object obj { get; set; }
    }

    public class CommonInstaller : IInstaller
    {
        public static bool IsInstal;
        public string InstallerPath { get; set; }
        public CommonInstallOptions Options { get; set; }

        public CommonInstaller(string installerPath, CommonInstallOptions options)
        {
            if (string.IsNullOrWhiteSpace(installerPath))
            {
                throw new ArgumentException("Installer path can not be null or whitespace.");
            }
            this.InstallerPath = installerPath;
            this.Options = options ?? throw new ArgumentNullException("Install options is null");
        }

        public async void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken)
        {
            IsInstal = true;
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

            await BeginInstallFromJObject(callback, cancellationToken, jObject, tempPath);

            Directory.Delete(tempPath, true);
            File.Delete(InstallerPath);
            IsInstal = false;
        }

        public async Task BeginInstallFromJObject(ProgressCallback callback, CancellationToken cancellationToken, JObject jObj, string tempPath)
        {
            var jsonObj = jObj.ToObject<CommonJsonObj>();
            var t = jsonObj.Install.Path.Split(':');
            var libPackage = t[0];
            var libName = t[1];
            var libVersion = t[2];
            string libPath = string.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}.jar",
                Options.GameRootPath, libPackage.Replace(".", @"\"), libName, libVersion);

            string libDir = Path.GetDirectoryName(libPath);
            if (!Directory.Exists(libDir))
            {
                Directory.CreateDirectory(libDir);
            }
            await Task.Factory.StartNew(() =>
            {
                File.Copy(tempPath + '\\' + jsonObj.Install.FilePath, libPath, true);

                string newPath = PathManager.GetJsonPath(Options.GameRootPath, jsonObj.Install.Target);
                string newDir = Path.GetDirectoryName(newPath);
                string jarPath = PathManager.GetJarPath(Options.GameRootPath, jsonObj.Install.Target);

                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }
                File.WriteAllText(newPath, jsonObj.VersionInfo.ToString());
                File.Copy(tempPath + '\\' + jsonObj.Install.FilePath, jarPath, true);
            });
        }
    }
}
