﻿using NsisoLauncher.Core.Modules;
using NsisoLauncher.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using NsisoLauncher.Core.LaunchException;
using System.Threading.Tasks;
using System.Threading;

namespace NsisoLauncher.Core
{
    public class LaunchHandler
    {
        private object launchLocker = new object();
        private object getVersionsLocker = new object();

        /// <summary>
        /// 启动器所处理的游戏根目录
        /// </summary>
        public string GameRootPath { get; set; }

        /// <summary>
        /// 启动器所使用JAVA
        /// </summary>
        public Java Java { get; set; }

        /// <summary>
        /// 是否开启版本隔离
        /// </summary>
        public bool VersionIsolation { get; set; }

        private ArgumentsParser argumentsParser;
        private VersionReader versionReader;

        public LaunchHandler(string gamepath, Java java, bool isversionIsolation)
        {
            this.GameRootPath = gamepath;
            this.Java = java;
            this.VersionIsolation = isversionIsolation;
            versionReader = new VersionReader(new System.IO.DirectoryInfo(GameRootPath + @"\versions"));
            argumentsParser = new ArgumentsParser(this);
        }

        public async Task<LaunchResult> LaunchAsync(LaunchSetting setting)
        {
            return await Task.Factory.StartNew(() =>
            {
                return Launch(setting);
            });
        }

        private LaunchResult Launch(LaunchSetting setting)
        {
            try
            {
                foreach (var item in setting.Version.Natives)
                {
                    string nativePath = GetNativePath(item);
                    if (File.Exists(nativePath))
                    {
                        Unzip.UnZipFile(nativePath, GetGameVersionRootDir(setting.Version) + @"\$natives", item.Exclude);
                    }
                    else
                    {
                        return new LaunchResult(new NativeNotFoundException(item, nativePath));
                    }

                }
                string arg = argumentsParser.Parse(setting);
                ProcessStartInfo startInfo = new ProcessStartInfo(Java.Path, arg)
                { RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false, WorkingDirectory = GameRootPath };
                var process = Process.Start(startInfo);
                return new LaunchResult() { Process = process, IsSuccess = true, LaunchArguments = arg };
            }
            catch (LaunchException.LaunchException ex)
            {
                return new LaunchResult(ex);
            }
            catch (Exception ex)
            {
                return new LaunchResult(ex);
            }
        }

        private List<Modules.Version> GetVersions()
        {
            return versionReader.GetVersions();
        }

        public async Task<List<Modules.Version>> GetVersionsAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                return GetVersions();
            });
        }

        public string GetGameVersionRootDir(Modules.Version ver)
        {
            if (VersionIsolation)
            {
                return string.Format(@"{0}\versions\{1}", GameRootPath, ver.ID);
            }
            else
            {
                return GameRootPath;
            }

        }

        public string GetLibraryPath(Modules.Library lib)
        {
            return String.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}.jar",
                this.GameRootPath, lib.Package.Replace(".", @"\"), lib.Name, lib.Version);
        }

        public string GetNativePath(Native native)
        {
            return String.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}-{4}.jar",
                this.GameRootPath, native.Package.Replace(".", @"\"), native.Name, native.Version, native.NativeSuffix);
        }

        public string GetJarPath(Modules.Version ver)
        {
            return string.Format(@"{0}\versions\{1}\{1}.jar", this.GameRootPath, ver.ID);
        }
    }
}
