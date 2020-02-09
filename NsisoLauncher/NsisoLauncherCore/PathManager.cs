﻿using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;

namespace NsisoLauncherCore
{
    /// <summary>
    /// 全局路径管理器
    /// </summary>
    public static class PathManager
    {
        /// <summary>
        /// 启动器名称（用作配置文件路径组成，请使用英文）
        /// </summary>
        public static string LauncherName { get; set; } = "NsisoLauncher";

        /// <summary>
        /// 启动器当前真实目录
        /// </summary>
        public static string CurrentLauncherDirectory { get => Environment.CurrentDirectory; }

        private static string _baseStorageDirectory;
        /// <summary>
        /// 基本的启动器仓库目录（存放配置文件，资源，缓存等。默认当前目录加启动器名称）
        /// </summary>
        public static string BaseStorageDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_baseStorageDirectory))
                {
                    _baseStorageDirectory = CurrentLauncherDirectory + '\\' + "Config";
                }
                return _baseStorageDirectory;
            }
            set
            {
                _baseStorageDirectory = value;
            }
        }

        public static string TempDirectory { get => BaseStorageDirectory + "\\Download"; }

        #region 启动器路径处理
        /// <summary>
        /// 获取游戏版本根目录
        /// </summary>
        /// <param name="versionIsolation">版本是否隔离</param>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="ver">版本</param>
        /// <returns></returns>
        public static string GetGameVersionRootDir(bool versionIsolation, string gameRootPath, Modules.MCVersion ver)
        {
            if (versionIsolation)
            {
                return string.Format(@"{0}\versions\{1}", gameRootPath, ver.ID);
            }
            else
            {
                return gameRootPath;
            }
        }

        /// <summary>
        /// 获取Artifact（字符串形式）路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="artifactStr">obj</param>
        /// <returns>路径</returns>
        public static string GetArtifactPath(string gameRootPath, string artifactStr)
        {
            Artifact artifact = new Artifact(artifactStr);
            return GetArtifactPath(gameRootPath, artifact);
        }

        /// <summary>
        /// 获取Artifact路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="artifact">obj</param>
        /// <returns>路径</returns>
        public static string GetArtifactPath(string gameRootPath, Artifact artifact)
        {
            if (string.IsNullOrEmpty(artifact.Classifier))
            {
                return string.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}.{4}",
                gameRootPath, artifact.Package.Replace(".", @"\"), artifact.Name, artifact.Version, artifact.Extension);
            }
            else
            {
                return string.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}-{4}.{5}",
                gameRootPath, artifact.Package.Replace(".", @"\"), artifact.Name, artifact.Version, artifact.Classifier, artifact.Extension);
            }
        }

        /// <summary>
        /// 获取library库路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="lib">库</param>
        /// <returns>路径</returns>
        public static string GetLibraryPath(string gameRootPath, Library lib)
        {
            return GetArtifactPath(gameRootPath, lib.Artifact);
        }

        /// <summary>
        /// 获取Native库路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="native">库</param>
        /// <returns>路径</returns>
        public static string GetNativePath(string gameRootPath, Native native)
        {
            return string.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}-{4}.jar",
                gameRootPath, native.Artifact.Package.Replace(".", @"\"), native.Artifact.Name, native.Artifact.Version, native.NativeSuffix);
        }

        /// <summary>
        /// 获取Json文件
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="ID">版本ID</param>
        /// <returns></returns>
        public static string GetJsonPath(string gameRootPath, string ID)
        {
            return string.Format(@"{0}\versions\{1}\{1}.json", gameRootPath, ID);
        }

        /// <summary>
        /// 获取核心Jar包路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="ver">版本</param>
        /// <returns></returns>
        public static string GetJarPath(string gameRootPath, MCVersion ver)
        {
            if (ver.Jar != null)
            {
                return string.Format(@"{0}\versions\{1}\{1}.jar", gameRootPath, ver.Jar);
            }
            else
            {
                return GetJarPath(gameRootPath, ver.ID);
            }
        }

        /// <summary>
        /// 获取核心Jar包路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="id">版本ID</param>
        /// <returns></returns>
        public static string GetJarPath(string gameRootPath, string id)
        {
            return string.Format(@"{0}\versions\{1}\{1}.jar", gameRootPath, id);
        }

        /// <summary>
        /// 获取资源引导文件路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="assetsID">版本ID</param>
        /// <returns></returns>
        public static string GetAssetsIndexPath(string gameRootPath, string assetsID)
        {
            return string.Format(@"{0}\assets\indexes\{1}.json", gameRootPath, assetsID);
        }

        /// <summary>
        /// 获取资源文件路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="assetsInfo">资源文件Obj</param>
        /// <returns></returns>
        public static string GetAssetsPath(string gameRootPath, JAssetsInfo assetsInfo)
        {
            return string.Format(@"{0}\assets\objects\{1}\{2}", gameRootPath, assetsInfo.Hash.Substring(0, 2), assetsInfo.Hash);
        }

        /// <summary>
        /// 获取NIDE8核心路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <returns>NIDE8路径</returns>
        public static string GetNide8JarPath(string gameRootPath)
        {
            return string.Format(@"{0}\nide8auth.jar", gameRootPath);
        }

        /// <summary>
        /// 获取Authlib-injector核心路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <returns>NIDE8路径</returns>
        public static string GetAIJarPath(string gameRootPath)
        {
            return string.Format(@"{0}\Authlib-injector.jar", gameRootPath);
        }

        /// <summary>
        /// 获取版本设置文件Options路径
        /// </summary>
        /// <param name="versionIsolation">版本是否隔离</param>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="version">版本</param>
        /// <returns>版本配置文件路径</returns>
        public static string GetVersionOptionsPath(bool versionIsolation, string gameRootPath, Modules.MCVersion version)
        {
            string verRoot = GetGameVersionRootDir(versionIsolation, gameRootPath, version);
            return verRoot + "\\options.txt";
        }

        /// <summary>
        /// 获取版本设置文件optionsof路径
        /// </summary>
        /// <param name="versionIsolation">版本是否隔离</param>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="version">版本</param>
        /// <returns>版本配置文件路径</returns>
        public static string GetVersionOptionsofPath(bool versionIsolation, string gameRootPath, Modules.MCVersion version)
        {
            string verRoot = GetGameVersionRootDir(versionIsolation, gameRootPath, version);
            return verRoot + "\\optionsof.txt";
        }
        #endregion
    }
}
