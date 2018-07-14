﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NsisoLauncher.Core.Util
{
    public class VersionReader
    {
        private DirectoryInfo dirInfo;
        private LaunchHandler handler;
        private object locker = new object();
        public VersionReader(LaunchHandler launchHandler)
        {
            dirInfo = new DirectoryInfo(launchHandler.GameRootPath + @"\versions");
            handler = launchHandler;
        }

        public Modules.Version GetVersion(string ID)
        {
            lock (locker)
            {
                string jsonPath = handler.GetJsonPath(ID);
                if (!File.Exists(jsonPath))
                {
                    return null;
                }
                string jsonStr = File.ReadAllText(jsonPath, Encoding.UTF8);
                var obj = JObject.Parse(jsonStr);
                Modules.Version ver = new Modules.Version();
                ver = obj.ToObject<Modules.Version>();
                if (obj.ContainsKey("arguments"))
                {
                    #region 处理新版本引导

                    JToken gameArg = obj["arguments"]["game"];
                    StringBuilder gameArgBuilder = new StringBuilder();
                    foreach (var arg in gameArg)
                    {
                        if (arg.Type == JTokenType.String)
                        {
                            gameArgBuilder.AppendFormat("{0} ", arg.ToString());
                        }
                    }
                    ver.MinecraftArguments = gameArgBuilder.ToString();

                    JToken jvmArg = obj["arguments"]["jvm"];
                    StringBuilder jvmArgBuilder = new StringBuilder();
                    foreach (var arg in jvmArg)
                    {
                        if (arg.Type == JTokenType.String)
                        {
                            jvmArgBuilder.AppendFormat("{0} ", arg.ToString());
                        }
                        else if (arg.Type == JTokenType.Object)
                        {
                            JToken rules = arg["rules"];
                            foreach (var rule in rules)
                            {
                                if (rule["action"].ToString() == "allow")
                                {
                                    if (rule["os"]["name"].ToString() == "windows")
                                    {
                                        if (rule["os"]["version"] != null)
                                        {
                                            if (Regex.Match(Environment.OSVersion.Version.ToString(), rule["os"]["version"].ToString()).Success)
                                            {
                                                if (arg["value"].Type == JTokenType.String)
                                                {
                                                    jvmArgBuilder.AppendFormat("{0} ", arg["value"].ToString());
                                                }
                                                else if (arg["value"].Type == JTokenType.Array)
                                                {
                                                    foreach (var str in arg["value"])
                                                    {
                                                        jvmArgBuilder.AppendFormat("\"{0}\" ", str);
                                                    }
                                                }

                                            }
                                        }
                                        else
                                        {
                                            if (arg["value"].Type == JTokenType.String)
                                            {
                                                jvmArgBuilder.AppendFormat("{0} ", arg["value"].ToString());
                                            }
                                            else if (arg["value"].Type == JTokenType.Array)
                                            {
                                                foreach (var str in arg["value"])
                                                {
                                                    jvmArgBuilder.AppendFormat("{0} ", str);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    ver.JvmArguments = jvmArgBuilder.ToString();
                    #endregion
                }
                else
                {
                    ver.JvmArguments = "-Djava.library.path=${natives_directory} -cp ${classpath}";
                }

                #region 处理库文件
                ver.Libraries = new List<Modules.Library>();
                ver.Natives = new List<Modules.Native>();
                var libToken = obj["libraries"];
                foreach (JToken lib in libToken)
                {
                    var libObj = lib.ToObject<Library>();
                    var parts = libObj.Name.Split(':');
                    if (libObj.Natives == null)
                    {
                        if (CheckAllowed(libObj.Rules))
                        {
                            ver.Libraries.Add(new Modules.Library()
                            {
                                Package = parts[0],
                                Name = parts[1],
                                Version = parts[2]
                            });
                        }
                    }
                    else
                    {
                        if (CheckAllowed(libObj.Rules))
                        {
                            var native = new Modules.Native
                            {
                                Package = parts[0],
                                Name = parts[1],
                                Version = parts[2],
                                NativeSuffix = libObj.Natives["windows"].Replace("${arch}", SystemTools.GetSystemArch() == ArchEnum.x64 ? "64" : "32")
                            };
                            ver.Natives.Add(native);
                            if (libObj.Extract != null)
                            {
                                native.Exclude = libObj.Extract.Exculde;
                            }
                        }
                    }
                }
                #endregion

                #region 处理版本继承
                if (ver.InheritsVersion != null)
                {
                    var iv = GetVersion(ver.InheritsVersion);
                    if (iv != null)
                    {
                        ver.AssetIndex = iv.AssetIndex;
                        ver.Natives.AddRange(iv.Natives);
                        ver.Libraries.AddRange(iv.Libraries);
                    }
                }
                #endregion

                return ver;
            }
        }

        public async Task<Modules.Version> GetVersionAsync(string ID)
        {
            return await Task.Factory.StartNew(() =>
            {
                return GetVersion(ID);
            });
        }

        public List<Modules.Version> GetVersions()
        {
            if (!dirInfo.Exists)
            {
                return new List<Modules.Version>();
            }
            var dirs = dirInfo.EnumerateDirectories();
            List<Modules.Version> versions = new List<Modules.Version>();
            foreach (var item in dirs)
            {
                var ver = GetVersion(item.Name);
                if (ver != null)
                {
                    versions.Add(ver);
                }
            }
            return versions;
        }

        public async Task<List<Modules.Version>> GetVersionsAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                return GetVersions();
            });
        }

        /// <summary>
		/// 判断规则
		/// </summary>
		/// <param name="rules">规则列表</param>
		/// <returns>是否启用</returns>
		public bool CheckAllowed(List<Rule> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                return true;
            }
            var allowed = false;
            foreach (var rule in rules)
            {
                if (rule.OS == null)
                {
                    allowed = rule.Action == "allow";
                }
                else if (rule.OS.Name == "windows")
                {
                    allowed = rule.Action == "allow";
                }
            }
            return allowed;
        }
    }

    public class Library
    {
        /// <summary>
        /// 库名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Native列表
        /// </summary>
        [JsonProperty("natives")]
        public Dictionary<string, string> Natives { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        [JsonProperty("rules")]
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// 解压声明
        /// </summary>
        [JsonProperty("extract")]
        public Extract Extract { get; set; }
    }

    public class Rule
    {
        /// <summary>
        /// action
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        [JsonProperty("os")]
        public OperatingSystem OS { get; set; }
    }

    public class OperatingSystem
    {
        /// <summary>
        /// 系统名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Extract
    {
        /// <summary>
        /// 排除列表
        /// </summary>
        [JsonProperty("exclude")]
        public List<string> Exculde { get; set; }
    }
}
