﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NsisoLauncher.Core
{
    public class ArgumentsParser
    {
        private LaunchHandler handler;

        public ArgumentsParser(LaunchHandler handler)
        {
            this.handler = handler;
        }

        public string Parse(Modules.LaunchSetting setting)
        {
            #region 处理JVM启动头参数
            StringBuilder jvmHead = new StringBuilder();
            if (setting.GCEnabled)
            {
                switch (setting.GCType)
                {
                    case Modules.GCType.G1GC:
                        jvmHead.Append("-XX:+UseG1GC");
                        break;
                    case Modules.GCType.SerialGC:
                        jvmHead.Append("-XX:+UseSerialGC");
                        break;
                    case Modules.GCType.ParallelGC:
                        jvmHead.Append("-XX:+UseParallelGC");
                        break;
                    case Modules.GCType.CMSGC:
                        jvmHead.Append("-XX:+UseConcMarkSweepGC");
                        break;
                    default:
                        break;
                }
                jvmHead.Append(' ');
            }
            if (!string.IsNullOrWhiteSpace(setting.GCArgument))
            {
                jvmHead.Append(setting.GCArgument).Append(' ');
            }
            if (setting.MinMemory != 0)
            {
                jvmHead.Append(string.Format("-Xms{0}m ", setting.MinMemory));
            }
            if (setting.MaxMemory != 0)
            {
                jvmHead.Append(string.Format("-Xmx{0}m ", setting.MaxMemory));
            }
            if (!string.IsNullOrWhiteSpace(setting.AdvencedJvmArguments))
            {
                jvmHead.Append(setting.AdvencedJvmArguments).Append(' ');
            }
            #endregion

            #region 处理游戏参数
            string assetsPath = setting.Version.Assets == "legacy" ? "assets\\virtual\\legacy" : "assets";
            string legacy = setting.AuthenticateSelectedUUID.Legacy ? "Legacy" : "Mojang";
            Dictionary<string, string> gameArgDic = new Dictionary<string, string>()
            {
                {"${auth_player_name}",setting.AuthenticateSelectedUUID.PlayerName },
                {"${auth_session}",setting.AuthenticateResponse.AccessToken },
                {"${version_name}",setting.Version.ID },
                {"${game_directory}",handler.GetGameVersionRootDir(setting.Version) },
                {"${game_assets}",assetsPath },
                {"${assets_root}",assetsPath },
                {"${assets_index_name}",setting.Version.Assets },
                {"${auth_uuid}",setting.AuthenticateSelectedUUID.Value },
                {"${auth_access_token}",setting.AuthenticateResponse.AccessToken },
                {"${user_properties}",ToList(setting.AuthenticateResponse.User.Properties) },
                {"${user_type}",legacy },
                {"${version_type}",setting.VersionType }
            };
            string gameArg = ReplaceByDic(setting.Version.MinecraftArguments, gameArgDic);
            #endregion

            #region 处理游戏JVM参数
            Dictionary<string, string> jvmArgDic = new Dictionary<string, string>()
            {
                {"${natives_directory}",string.Format("\"{0}{1}\"",handler.GetGameVersionRootDir(setting.Version), @"\$natives") },
                {"${launcher_name}","NsisoLauncher" },
                {"${launcher_version}", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                {"${classpath}",GetClassPaths(setting.Version.Libraries,setting.Version) },
            };
            string jvmArg = ReplaceByDic(setting.Version.JvmArguments, jvmArgDic);
            #endregion

            return jvmHead.ToString() + jvmArg + ' ' + setting.Version.MainClass + ' ' + gameArg;
        }

        private static string ToList(List<Net.MojangApi.Responses.AuthenticateResponse.UserData.Property> properties)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            foreach (var item in properties)
            {
                sb.Append(string.Format("\"{0}\":[\"{1}\"]", item.Name, item.Value));
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string GetClassPaths(List<Modules.Library> libs, Modules.Version ver)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('\"');
            foreach (var item in libs)
            {
                stringBuilder.Append(handler.GetLibraryPath(item)).Append(';');
            }
            stringBuilder.Append(handler.GetJarPath(ver));
            stringBuilder.Append('\"');
            return stringBuilder.ToString();
        }

        private static string ReplaceByDic(string str, Dictionary<string,string> dic)
        {
            return dic.Aggregate(str, (a, b) => a.Replace(b.Key, b.Value));
        }
    }
}