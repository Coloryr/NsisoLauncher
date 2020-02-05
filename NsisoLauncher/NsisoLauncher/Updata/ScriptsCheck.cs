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
                return new Dictionary<string, UpdataItem>();
            }
            Dictionary<string, UpdataItem> list = new Dictionary<string, UpdataItem>();
            IChecker checker = new MD5Checker();
            await Task.Factory.StartNew(() =>
            {
                var GetFiles = new GetFiles();
                foreach (string FilePath in GetFiles.getDirectory(path))
                {
                    checker.FilePath = FilePath;
                    UpdataItem mod = new UpdataItem();
                    mod.local = FilePath;
                    mod.name = mod.filename = FilePath.Replace(path, "");
                    mod.url = server_info.server_local + @"\scripts\" + mod.filename;
                    mod.check = checker.GetFileChecksum();
                    if (list.ContainsKey(mod.name) == false)
                        list.Add(mod.name, mod);
                }
            });
            return list;
        }
    }
}
