﻿using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;

namespace NsisoLauncherCore.Util
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
                int rm = Convert.ToInt32(Math.Floor(GetTotalMemory() * 0.6));
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
            ManagementClass mc = new("Win32_PhysicalMemory");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                foreach (PropertyData pd in mo.Properties)
                {
                    if (pd?.Name == "Capacity")
                    {
                        return Convert.ToUInt64(pd.Value.ToString()) / 1024 / 1024;
                    }
                }
            }
            return 0;
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
    }
}
