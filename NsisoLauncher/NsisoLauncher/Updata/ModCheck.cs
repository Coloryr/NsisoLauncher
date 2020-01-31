using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using NsisoLauncher;
using NsisoLauncherCore.Util.Checker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class ModCheck
    {
        public async Task<Dictionary<string, updata_item>> ReadModInfo(string path)
        {
            path += @"\mods\";
            App.logHandler.AppendInfo("检查mod:" + path);
            try
            {
                if (!Directory.Exists(path))
                {
                    return new Dictionary<string, updata_item>();
                }
                string[] files = Directory.GetFiles(path, "*.jar");
                Dictionary<string, updata_item> list = new Dictionary<string, updata_item>();
                foreach (string file in files)
                {
                    await Task.Factory.StartNew(() =>
                    {
                        updata_item save = GetModsInfo(path, file);
                        if (list.ContainsKey(save.name))
                        {
                            save.name += "1";
                        }
                        if (list.ContainsValue(save) == false)
                            list.Add(save.name, save);
                    });
                }
                return list;
            }
            catch (Exception e)
            {
                App.logHandler.AppendInfo("检查mod错误" + e.Source);
            }
            return new Dictionary<string, updata_item>();
        }
        public updata_item GetModsInfo(string path, string fileName)
        {
            try
            {
                JToken modinfo = null;
                updata_item mod = new updata_item();
                mod.filename = fileName.Replace(path, "");
                try
                {
                    ZipFile zip = new ZipFile(fileName);
                    ZipEntry zp = zip.GetEntry("mcmod.info");
                    if (zp == null)
                    {
                        zip.Close();
                        foreach (string name in Tran_1_12_list)
                        {
                            if (mod.filename.Contains(name))
                            {
                                mod.name = name;
                            }
                        }
                        goto a;
                    }
                    Stream stream = zip.GetInputStream(zp);
                    TextReader reader = new StreamReader(stream);
                    string b = reader.ReadToEnd();
                    try
                    {
                        modinfo = JArray.Parse(b)[0];
                    }
                    catch
                    {
                        try
                        {
                            var a = JObject.Parse(b).ToObject<Mod_Obj_List.Root>().modList[0];
                            modinfo = JObject.FromObject(a);
                        }
                        catch
                        {
                            zip.Close();
                            stream.Close();
                            modinfo = null;
                        }
                    }
                    zip.Close();
                    stream.Close();
                    if (modinfo != null)
                    {
                        var c = modinfo.ToObject<Mod_Obj>();
                        if (c.name != null)
                        {
                            mod.name = c.name;
                        }
                    }
                }
                catch
                {
                    
                }
            a:
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
        private static List<string> Tran_1_12_list = new List<string>()
        {
            "AppleCore", "BetterFps", "jehc", "MakeZoomZoom", "MCMultiPart",
            "Rally+Health", "SelfControl", "BNBGamingCore", "rftoolspower"
        };
    }
}
