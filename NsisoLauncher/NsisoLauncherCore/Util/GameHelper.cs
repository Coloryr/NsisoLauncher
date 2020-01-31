using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util
{
    /// <summary>
    /// 游戏设置OPTION类
    /// </summary>
    public class VersionOption
    {
        /// <summary>
        /// 设置项
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 设置值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 转换为MC设置文件格式string
        /// </summary>
        /// <returns>一条设置Line</returns>
        public override string ToString()
        {
            return Key.Trim() + ':' + Value.Trim();
        }
        public enum Type
        {
            options,
            optionsof
        }
    }

    public static class GameHelper
    {
        public async static Task<List<VersionOption>> GetOptionsAsync(VersionOption.Type type, LaunchHandler core, Modules.Version version)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (version != null)
                {
                    try
                    {
                        string Path = "";
                        switch (type)
                        {
                            case VersionOption.Type.options:
                                Path = core.GetVersionOptionsPath(version);
                                break;
                            case VersionOption.Type.optionsof:
                                Path = core.GetVersionOptionsofPath(version);
                                break;
                            default:
                                return null;
                        }
                        
                        if (!File.Exists(Path))
                        {
                            return null;
                        }
                        string[] lines = File.ReadAllLines(Path);
                        List<VersionOption> options = new List<VersionOption>();
                        foreach (var item in lines)
                        {
                            string[] kv = item.Split(':');
                            if (kv.Length < 2)
                                continue;
                            if (kv.Length > 2)
                            {
                                string a = kv[1];
                                for (int i = 2; i < kv.Length; i++)
                                {
                                    a += ":" + kv[i];
                                }
                                options.Add(new VersionOption() { Key = kv[0], Value = a });
                            }
                            else
                                options.Add(new VersionOption() { Key = kv[0], Value = kv[1] });
                        }
                        return options;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            });
        }

        [DllImport("User32.dll")]
        private static extern int SetWindowText(IntPtr winHandle, string title);

        public static void SetGameTitle(LaunchResult result, string title)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var handle = result.Process.MainWindowHandle;
                    while (!result.Process.HasExited)
                    {
                        SetWindowText(handle, title);
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception) { }
            });
        }

        public async static Task SaveOptionsAsync(VersionOption.Type type, List<VersionOption> opts, LaunchHandler core, Modules.Version version)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    if (version != null && opts != null)
                    {
                        List<string> optLines = new List<string>();
                        foreach (var item in opts)
                        {
                            optLines.Add(item.ToString());
                        }
                        string Path = "";
                        switch (type)
                        {
                            case VersionOption.Type.options:
                                Path = core.GetVersionOptionsPath(version);
                                break;
                            case VersionOption.Type.optionsof:
                                Path = core.GetVersionOptionsofPath(version);
                                break;
                        }
                        File.WriteAllLines(Path, optLines.ToArray());
                    }
                }
                catch (Exception) { }
            });
        }
    }
}
