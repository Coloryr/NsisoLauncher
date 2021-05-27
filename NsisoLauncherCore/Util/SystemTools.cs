﻿using System;
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

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_INFO
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public uint dwTotalPhys;
            public uint dwAvailPhys;
            public uint dwTotalPageFile;
            public uint dwAvailPageFile;
            public uint dwTotalVirtual;
            public uint dwAvailVirtual;
        }
        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MEMORY_INFO meminfo);

        /// <summary>
        /// 获取电脑总内存(MB)
        /// </summary>
        /// <returns>物理内存</returns>
        public static ulong GetTotalMemory()
        {
            MEMORY_INFO MemInfo;
            MemInfo = new MEMORY_INFO();
            GlobalMemoryStatus(ref MemInfo);

            return Convert.ToUInt64(MemInfo.dwTotalPhys.ToString()) / 1024 / 1024;
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
