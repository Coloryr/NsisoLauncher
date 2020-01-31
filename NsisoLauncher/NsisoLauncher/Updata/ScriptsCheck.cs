using NsisoLauncher.Utils;
using NsisoLauncherCore.Util.Checker;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class ScriptsCheck
    {
        public async Task<Dictionary<string, updata_item>> ReadscriptsInfo(string path)
        {
            path += @"\scripts\";
            if (!Directory.Exists(path))
            {
                return new Dictionary<string, updata_item>();
            }
            Dictionary<string, updata_item> list = new Dictionary<string, updata_item>();
            IChecker checker = new MD5Checker();
            await Task.Factory.StartNew(() =>
            {
                var GetFiles = new GetFiles();
                foreach (string FilePath in GetFiles.getDirectory(path))
                {
                    checker.FilePath = FilePath;
                    updata_item mod = new updata_item();
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
