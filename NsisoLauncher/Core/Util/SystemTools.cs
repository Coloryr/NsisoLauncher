﻿using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace NsisoLauncher.Core.Util
{
    public enum ArchEnum
    {
        x32,
        x64
    }

    public class SystemTools
    {
        /// <summary>
        /// 获取匹配JAVA位数的最佳内存
        /// </summary>
        /// <param name="arch">JAVA位数</param>
        /// <returns>最佳内存大小</returns>
        public static int GetBestMemory(Java java)
        {
            if (java != null)
            {
                int rm = Convert.ToInt32(Math.Floor(GetRunmemory() * 0.6));
                switch (java.Arch)
                {
                    case ArchEnum.x32:
                        if (rm > 1024) { return 1024; }
                        else { return rm; }

                    case ArchEnum.x64:
                        if (rm > 4096) { return 4096; }
                        else { return rm; }

                    default:
                        return rm;
                }
            }
            else
            {
                return 1024;
            }

        }

        /// <summary>
        /// 获取电脑总内存(MB)
        /// </summary>
        /// <returns>物理内存</returns>
        public static ulong GetTotalMemory()
        {
            return new Computer().Info.TotalPhysicalMemory / 1048576;
        }

        /// <summary>
        ///     获取系统位数
        /// </summary>
        /// <returns>32 or 64</returns>
        public static ArchEnum GetSystemArch()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return ArchEnum.x64;
            }
            else
            {
                return ArchEnum.x32;
            }
        }

        /// <summary>
        /// 获取系统剩余内存(MB)
        /// </summary>
        /// <returns>剩余内存</returns>
        public static ulong GetRunmemory()
        {
            ComputerInfo ComputerMemory = new ComputerInfo();
            return ComputerMemory.AvailablePhysicalMemory / 1048576;
        }

        /// <summary>
        /// 获取显卡信息
        /// </summary>
        /// <returns></returns>
        public static string GetVideoCardInfo()
        {
            try
            {
                var sb = new StringBuilder();
                var i = 0;
                foreach (var mo in new ManagementClass("Win32_VideoController").GetInstances().Cast<ManagementObject>())
                {
                    sb.Append('#').Append(i++).Append(mo["Name"].ToString().Trim()).Append(' ');
                }
                return sb.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns></returns>
        public static string GetProcessorInfo()
        {
            try
            {
                var sb = new StringBuilder();
                var i = 0;
                foreach (var mo in new ManagementClass("WIN32_Processor").GetInstances().Cast<ManagementObject>())
                {
                    sb.Append('#').Append(i++).Append(mo["Name"].ToString().Trim()).Append(' ');
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsSetupFrameworkUpdate(string name)
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Updates"))
            {
                foreach (string baseKeyName in baseKey.GetSubKeyNames())
                {
                    if (baseKeyName.Contains(".NET Framework"))
                    {
                        using (RegistryKey updateKey = baseKey.OpenSubKey(baseKeyName))
                        {
                            foreach (string kbKeyName in updateKey.GetSubKeyNames())
                            {
                                Console.WriteLine(kbKeyName);
                                if (kbKeyName.Equals(name))
                                {
                                    return true;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
        }
    }
}
