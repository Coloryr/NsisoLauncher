using NsisoLauncher.Utils;
using NsisoLauncherCore.Util.Checker;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class ScriptsCheck
    {
        public async Task<Dictionary<string, UpdataItem>> ReadscriptsInfo(string path)
        {
            path += @"\scripts\";
            if (!Directory.Exists(path))
            {
                return new();
            }
            Dictionary<string, UpdataItem> list = new();
            IChecker checker = new MD5Checker();
            await Task.Run(() =>
            {
                var GetFiles = new GetFiles();
                foreach (string FilePath in GetFiles.GetDirectory(path))
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
