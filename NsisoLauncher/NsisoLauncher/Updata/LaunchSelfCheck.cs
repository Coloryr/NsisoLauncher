using NsisoLauncher.Utils;
using NsisoLauncherCore.Util.Checker;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class LaunchSelfCheck
    {
        public async Task<Dictionary<string, UpdataItem>> ReadLaunchrSelfInfo(string path)
        {
            if (!Directory.Exists(path))
            {
                return new Dictionary<string, UpdataItem>();
            }
            Dictionary<string, UpdataItem> list = new Dictionary<string, UpdataItem>();
            IChecker checker = new MD5Checker();
            await Task.Factory.StartNew(() =>
            {
                var GetFiles = new GetFiles();
                foreach (string FilePath in GetFiles.GetFileName(path))
                {
                    checker.FilePath = FilePath;
                    UpdataItem mod = new UpdataItem();
                    mod.local = FilePath;
                    mod.name = mod.filename = FilePath.Replace(path, "");
                    mod.check = checker.GetFileChecksum();
                    if (list.ContainsKey(mod.name) == false)
                        list.Add(mod.name, mod);
                }
            });
            return list;
        }
    }
}
