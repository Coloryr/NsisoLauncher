﻿using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util.Installer.Forge.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static NsisoLauncherCore.PathManager;
using static NsisoLauncherCore.Util.Installer.Forge.Json.Install;

namespace NsisoLauncherCore.Util.Installer.Forge.Actions
{
    public class PostProcessors
    {
        public Install Profile { get; set; }
        public bool IsClient { get; set; }
        public ProgressCallback Monitor { get; set; }
        public bool HasTasks { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public List<Processor> Processors { get; set; }

        public PostProcessors(Install profile, bool isClient, ProgressCallback monitor)
        {
            this.Profile = profile;
            this.IsClient = isClient;
            this.Monitor = monitor;
            this.Processors = profile.Processors;
            this.HasTasks = this.Processors.Count != 0;
            this.Data = new Dictionary<string, string>();
        }
        public Exception Process(string temp, string gamerootPath, string minecraft, Java java)
        {
            try
            {
                int progress = 1;
                Dictionary<string, string> originalData = Profile.GetData(IsClient);
                if (originalData.Count != 0)
                {
                    Monitor.SetTotalSize(originalData.Count);

                    foreach (var item in originalData)
                    {
                        Monitor.IncreaseDoneSize(progress++);

                        if (item.Value[0] == '[' && item.Value[item.Value.Length - 1] == ']')
                        { //Artifact
                            Data.Add(item.Key, GetArtifactPath(gamerootPath, item.Value.Substring(1, item.Value.Length - 2)));
                        }
                        else if (item.Value[0] == '\'' && item.Value[item.Value.Length - 1] == '\'')
                        { //Literal
                            Data.Add(item.Key, item.Value.Substring(1, item.Value.Length - 2));
                        }
                        else
                        {
                            string target = temp + item.Value;
                            Monitor.SetState(string.Format("Extracting:{0}", item.Value));
                            Data.Add(item.Key, target);
                        }
                    }
                }
                Data.Add("SIDE", IsClient ? "client" : "server");
                Data.Add("MINECRAFT_JAR", minecraft);

                Monitor.SetTotalSize(Processors.Count);
                Monitor.SetDoneSize(0);
                Monitor.SetState("Building Processors");

                foreach (var proc in Processors)
                {
                    string jarPath = GetArtifactPath(gamerootPath, proc.Jar);
                    if (!File.Exists(jarPath))
                    {
                        return new FileNotFoundException("Jar file can not found", jarPath);
                    }

                    string jarTemp = Path.GetDirectoryName(jarPath);
                    Unzip.UnZipFile(jarPath, jarTemp);

                    string MetaPath = jarTemp + @"\META-INF\MANIFEST.MF";
                    if (!File.Exists(MetaPath))
                    {
                        return new FileNotFoundException("META-INF file can not found", MetaPath);
                    }

                    string metaContent = File.ReadAllText(MetaPath);
                    string mainClass = Regex.Match(metaContent, @"(?M)^Main-Class: (.*)$").Groups[1].Value.Trim();

                    StringBuilder argBuilder = new StringBuilder();

                    argBuilder.Append("-cp \"");
                    foreach (var item in proc.ClassPath)
                    {
                        argBuilder.Append(GetArtifactPath(gamerootPath, item)).Append(';');
                    }
                    argBuilder.Append(jarPath).Append("\" ");

                    argBuilder.Append(mainClass).Append(' ');

                    foreach (var item in proc.Args)
                    {
                        char start = item[0];
                        char end = item[item.Length - 1];

                        if (start == '[' && end == ']') //Library
                            argBuilder.Append("\"").Append(GetArtifactPath(gamerootPath, item.Substring(1, item.Length - 2))).Append("\" ");
                        else if (start == '{' && end == '}')
                        { // Data
                            string key = item.Substring(1, item.Length - 2);
                            string value = Data[key];
                            if (value != null)
                            {
                                argBuilder.Append("\"").Append(value).Append("\" ");
                            }
                        }
                        else
                        {
                            argBuilder.Append(item).Append(' ');
                        }
                    }

                    ProcessStartInfo processStartInfo = new ProcessStartInfo(java.Path, argBuilder.ToString())
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    Monitor.SetState(string.Format("执行安装器{0}", proc.Jar.Name));
                    var result = System.Diagnostics.Process.Start(processStartInfo);
                    result.BeginOutputReadLine();
                    result.OutputDataReceived += Result_OutputDataReceived;
                    result.WaitForExit();

                    Monitor.IncreaseDoneSize(1);
                }
                Monitor.SetDone();
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return e;
            }
        }

        private void Result_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
