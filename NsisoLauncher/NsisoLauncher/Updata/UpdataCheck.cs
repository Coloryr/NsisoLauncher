using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class UpdataCheck
    {
        /// <summary>
        /// 更新信息
        /// </summary>
        private updataOBJ UpdataObj;
        /// <summary>
        /// 资源检查
        /// </summary>
        public async Task<UpdataCheck> Check()
        {
            string Url = App.Config.MainConfig.Server.UpdataCheck.Address;
            try
            {
                var http = new HttpRequesterAPI(TimeSpan.FromSeconds(10));
                var res = await http.HttpGetAsync(Url);
                if (res.IsSuccessStatusCode)
                {
                    JObject json = JObject.Parse(await res.Content.ReadAsStringAsync());
                    UpdataObj = json.ToObject<updataOBJ>();
                    if (string.IsNullOrWhiteSpace(App.Config.MainConfig.Server.UpdataCheck.Packname))
                        return this;
                    else if (App.Config.MainConfig.Server.UpdataCheck.Version != UpdataObj.Version)
                        return this;
                    else
                        return null;
                }
            }
            catch
            {

            }
            return null;
        }

        public async Task<List<DownloadTask>> CheckUpdata(OtherCheck pack)
        {
            List<DownloadTask> DownloadTask = new List<DownloadTask>();
            if (UpdataObj.mods.Count != 0)
            {
                try
                {
                    var LocalMod = await new ModCheck().ReadModInfo(App.Handler.GameRootPath);
                    foreach (KeyValuePair<string, UpdataItem> updataItem in UpdataObj.mods)
                    {
                        if (updataItem.Value.function == "delete")
                        {
                            FileInfo file = new FileInfo(App.Handler.GameRootPath + @"\mods\" + updataItem.Value.filename);
                            if (file.Exists)
                            {
                                file.Delete();
                            }
                        }
                        else if (LocalMod.ContainsKey(updataItem.Key))
                        {
                            UpdataItem lo = LocalMod[updataItem.Value.name];
                            if (!string.Equals(lo.check, updataItem.Value.check, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Delete(lo.local);
                                DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.UpdateMod"),
                                    updataItem.Value.url, App.Handler.GameRootPath + @"\mods\" + updataItem.Value.filename));
                            }
                        }
                        else
                        {
                            DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.LostMod"),
                                updataItem.Value.url, App.Handler.GameRootPath + @"\mods\" + updataItem.Value.filename));
                        }
                    }
                }
                catch (Exception e)
                {
                    App.LogHandler.AppendFatal(e);
                }
            }
            if (UpdataObj.scripts.Count != 0)
            {
                var LocalScripts = await new ScriptsCheck().ReadscriptsInfo(App.Handler.GameRootPath);
                foreach (UpdataItem updataItem in UpdataObj.scripts)
                {
                    if (LocalScripts.ContainsKey(updataItem.name))
                    {
                        UpdataItem LocalScript = LocalScripts[updataItem.name];
                        if (string.Equals(LocalScript.check, updataItem.check, StringComparison.OrdinalIgnoreCase))
                        {
                            LocalScripts.Remove(updataItem.name);
                        }
                        else
                        {
                            File.Delete(LocalScript.local);
                            DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.UpdateScripts"),
                                updataItem.url, App.Handler.GameRootPath + @"\scripts\" + updataItem.filename));
                            LocalScripts.Remove(updataItem.name);
                        }
                    }
                    else
                    {
                        DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.LostScripts"), 
                            updataItem.url, App.Handler.GameRootPath + @"\scripts\" + updataItem.filename));
                    }
                }
                if (LocalScripts.Count != 0)
                {
                    foreach (UpdataItem updataItem in LocalScripts.Values)
                    {
                        File.Delete(updataItem.local);
                    }
                }
            }
            if (UpdataObj.resourcepacks.Count != 0)
            {
                var LocalResourcepacks = await new ResourcepacksCheck().ReadresourcepacksInfo(App.Handler.GameRootPath);
                foreach (UpdataItem updataItem in UpdataObj.resourcepacks)
                {
                    if (LocalResourcepacks.ContainsKey(updataItem.name))
                    {
                        UpdataItem LocalResourcepack = LocalResourcepacks[updataItem.name];
                        if (string.Equals(LocalResourcepack.check, updataItem.check, StringComparison.OrdinalIgnoreCase))
                        {
                            LocalResourcepacks.Remove(updataItem.name);
                        }
                        else
                        {
                            File.Delete(LocalResourcepack.local);
                            DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.UpdateResources"), 
                                updataItem.url, App.Handler.GameRootPath + @"\resourcepacks\" + updataItem.filename));
                            UpdataObj.scripts.Remove(updataItem);
                            LocalResourcepacks.Remove(updataItem.name);
                        }
                    }
                    else
                    {
                        DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.LostResources"), 
                            updataItem.url, App.Handler.GameRootPath + @"\resourcepacks\" + updataItem.filename));
                    }
                }
            }
            if (UpdataObj.config.Count != 0)
            {
                foreach (UpdataItem updataItem in UpdataObj.config)
                {
                    pack.packList.Add(updataItem.filename);
                    DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.UpdataConfig"), 
                        updataItem.url, App.Handler.GameRootPath + @"\" + updataItem.filename));
                }
            }

            return DownloadTask;
        }
        public string getVersion()
        {
            return UpdataObj == null ? "0.0.0" : UpdataObj.Version;
        }
        public string getPackName()
        {
            return UpdataObj == null ? "modpack" : UpdataObj.packname;
        }
    }
}
