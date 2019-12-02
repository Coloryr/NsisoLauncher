using NsisoLauncherCore.Util.Checker;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Color_yr.updata
{
    class scripts_check
    {
        private List<string> getFileName(string path)
        {
            List<string> files = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (FileInfo f in root.GetFiles())
            {
                files.Add(f.FullName);
            }
            return files;
        }
        private List<string> getDirectory(string path)
        {
            List<string> files = new List<string>();
            files.AddRange(getFileName(path));
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (DirectoryInfo d in root.GetDirectories())
            {
                files.AddRange(getDirectory(d.FullName));
            }
            return files;
        }

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
                foreach (string FilePath in getDirectory(path))
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
