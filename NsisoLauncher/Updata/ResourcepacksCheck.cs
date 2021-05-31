﻿using NsisoLauncherCore.Util.Checker;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class ResourcepacksCheck
    {
        public async Task<Dictionary<string, UpdataItem>> ReadresourcepacksInfo(string path)
        {
            path += @"\resourcepacks\";
            if (!Directory.Exists(path))
            {
                return new Dictionary<string, UpdataItem>();
            }
            Dictionary<string, UpdataItem> list = new();
            await Task.Run(() =>
            {
                string[] files = Directory.GetFiles(path, "*.zip");
                IChecker checker = new MD5Checker();
                foreach (string FilePath in files)
                {
                    checker.FilePath = FilePath;
                    UpdataItem mod = new()
                    {
                        local = FilePath,
                        check = checker.GetFileChecksum()
                    };
                    mod.name = mod.filename = FilePath.Replace(path, "");
                    if (list.ContainsKey(mod.name) == false)
                        list.Add(mod.name, mod);
                }
            });
            return list;
        }
    }
}
