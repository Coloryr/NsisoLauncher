using NsisoLauncherCore.Util.Checker;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class ResourcepacksCheck
    {
        public async Task<Dictionary<string, updata_item>> ReadresourcepacksInfo(string path)
        {
            path += @"\resourcepacks\";
            if (!Directory.Exists(path))
            {
                return new Dictionary<string, updata_item>();
            }
            Dictionary<string, updata_item> list = new Dictionary<string, updata_item>();
            await Task.Factory.StartNew(() =>
            {
                string[] files = Directory.GetFiles(path, "*.zip");
                IChecker checker = new MD5Checker();
                foreach (string FilePath in files)
                {
                    checker.FilePath = FilePath;
                    updata_item mod = new updata_item();
                    mod.local = FilePath;
                    mod.name = mod.filename = FilePath.Replace(path, "");
                    mod.url = server_info.server_local + @"\resourcepacks\" + mod.filename;
                    mod.check = checker.GetFileChecksum();
                    if (list.ContainsKey(mod.name) == false)
                        list.Add(mod.name, mod);
                }
            });
            return list;
        }
    }
}
