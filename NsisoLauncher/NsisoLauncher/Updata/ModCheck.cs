using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Util.Checker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class ModCheck
    {
        public async Task<Dictionary<string, UpdataItem>> ReadModInfo(string path)
        {
            path += @"\mods\";
            App.LogHandler.AppendInfo("检查mod:" + path.Replace(App.Handler.GameRootPath + path, ""));
            try
            {
                if (!Directory.Exists(path))
                {
                    return new Dictionary<string, UpdataItem>();
                }
                string[] files = Directory.GetFiles(path, "*.jar");
                Dictionary<string, UpdataItem> list = new Dictionary<string, UpdataItem>();
                foreach (string file in files)
                {
                    await Task.Factory.StartNew(() =>
                    {
                        UpdataItem save = GetModsInfo(path, file);
                        while (list.ContainsKey(save.name))
                            save.name += "1";
                        if (list.ContainsValue(save) == false)
                            list.Add(save.name, save);
                    });
                }
                return list;
            }
            catch (Exception e)
            {
                App.LogHandler.AppendInfo("检查mod错误" + e.Source);
            }
            return new Dictionary<string, UpdataItem>();
        }
        public UpdataItem GetModsInfo(string path, string fileName)
        {
            try
            {
                JToken modinfo = null;
                UpdataItem mod = new UpdataItem();
                mod.filename = fileName.Replace(path, "");
                using (ZipFile zip = new ZipFile(fileName))
                {
                    ZipEntry zp = zip.GetEntry("mcmod.info");
                    if (zp == null)
                    {
                        foreach (string name in TranList)
                        {
                            if (mod.filename.Contains(name))
                                mod.name = name;
                        }
                    }
                    else
                    {
                        using (Stream stream = zip.GetInputStream(zp))
                        {
                            TextReader reader = new StreamReader(stream);
                            string jsonString = reader.ReadToEnd();
                            try
                            {
                                if (jsonString.StartsWith("{"))
                                    modinfo = JArray.Parse(jsonString)[0];
                                else if (jsonString.StartsWith("["))
                                {
                                    var a = JObject.Parse(jsonString).ToObject<ModObjList.Root>().modList[0];
                                    modinfo = JObject.FromObject(a);
                                }
                            }
                            catch
                            {
                                modinfo = null;
                            }
                        }
                        if (modinfo != null)
                        {
                            var c = modinfo.ToObject<ModObj>();
                            if (c.name != null)
                            {
                                mod.name = c.name;
                            }
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(mod.name))
                {
                    mod.name = fileName.Replace(path + "\\", "");
                }
                IChecker checker = new MD5Checker();
                checker.FilePath = fileName;
                mod.check = checker.GetFileChecksum();
                mod.local = fileName;
                return mod;
            }
            catch
            {
                return null;
            }
        }
        private static List<string> TranList = new List<string>()
        {
            "AppleCore", "BetterFps", "jehc", "MakeZoomZoom", "MCMultiPart",
            "Rally+Health", "SelfControl", "BNBGamingCore", "rftoolspower"
        };
    }
}
