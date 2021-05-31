using Newtonsoft.Json.Linq;
using NsisoLauncherCore;
using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class UpdataCheckDo
    {
        /// <summary>
        /// 更新信息
        /// </summary>
        private UpdataOBJ UpdataObj;
        /// <summary>
        /// 更新自己
        /// </summary>
        public bool UpdataSelf { get; private set; } = false;

        private bool CheckVersion()
        {
            if (string.IsNullOrWhiteSpace(UpdataObj.LastVersion))
                return true;
            string oldVersion = App.Config.MainConfig.Server.UpdataCheck.Version.Replace(".", "");
            bool b = int.TryParse(oldVersion, out int a);
            if (b == false)
                return false;
            b = int.TryParse(UpdataObj.LastVersion.Replace(".", ""), out int c);
            if (b == false)
                return false;
            if (c > a)
                return false;
            return true;
        }

        /// <summary>
        /// 资源检查
        /// </summary>
        public async Task<dynamic> Check()
        {
            string Url = App.Config.MainConfig.Server.UpdataCheck.Address;
            try
            {
                var res = await HttpRequesterAPI.HttpGetAsync(Url);
                if (res.IsSuccessStatusCode)
                {
                    JObject json = JObject.Parse(await res.Content.ReadAsStringAsync());
                    UpdataObj = json.ToObject<UpdataOBJ>();
                    if (UpdataObj == null)
                        return true;
                    if (string.IsNullOrWhiteSpace(App.Config.MainConfig.Server.UpdataCheck.Packname))
                        return this;
                    else if (!CheckVersion())
                        return false;
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
            List<DownloadTask> DownloadTask = new();
            if (UpdataObj.mods != null && UpdataObj.mods.Count != 0)
            {
                try
                {
                    var LocalMod = await new ModCheck().ReadModInfo(App.Handler.GameRootPath);
                    foreach (var updataItem in UpdataObj.mods)
                    {
                        if (updataItem.Value.function == "delete")
                        {
                            FileInfo file = new(App.Handler.GameRootPath + @"\mods\" + updataItem.Value.filename);
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
            if (UpdataObj.scripts != null && UpdataObj.scripts.Count != 0)
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
            if (UpdataObj.resourcepacks != null && UpdataObj.resourcepacks.Count != 0)
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
            if (UpdataObj.config != null && UpdataObj.config.Count != 0)
            {
                foreach (UpdataItem updataItem in UpdataObj.config)
                {
                    pack.packList.Add(updataItem.filename);
                    DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.UpdataConfig"),
                        updataItem.url, App.Handler.GameRootPath + @"\" + updataItem.filename));
                }
            }
            if (UpdataObj.launch != null && UpdataObj.launch.Count != 0)
            {
                var LocalConfig = await new LaunchCheck().ReadLaunchrInfo(PathManager.BaseStorageDirectory + @"\");
                foreach (UpdataItem updataItem in UpdataObj.launch)
                {
                    if (LocalConfig.ContainsKey(updataItem.name))
                    {
                        UpdataItem LocalConfig1 = LocalConfig[updataItem.name];
                        if (string.Equals(LocalConfig1.check, updataItem.check, StringComparison.OrdinalIgnoreCase))
                        {
                            LocalConfig.Remove(updataItem.name);
                        }
                        else
                        {
                            File.Delete(LocalConfig1.local);
                            DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.UpdataConfig"),
                                updataItem.url, PathManager.BaseStorageDirectory + @"\" + updataItem.filename));
                            LocalConfig.Remove(updataItem.name);
                        }
                    }
                    else
                    {
                        DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.LostConfig"),
                            updataItem.url, PathManager.BaseStorageDirectory + @"\" + updataItem.filename));
                    }
                }
                if (LocalConfig.Count != 0)
                {
                    foreach (UpdataItem updataItem in LocalConfig.Values)
                    {
                        File.Delete(updataItem.local);
                    }
                }
            }
            if (UpdataObj.self != null && UpdataObj.self.Count != 0)
            {
                var LocalSelf = await new LaunchSelfCheck().ReadLaunchrSelfInfo(PathManager.CurrentLauncherDirectory + @"\");
                foreach (UpdataItem updataItem in UpdataObj.self)
                {
                    if (LocalSelf.ContainsKey(updataItem.name))
                    {
                        var updata = false;
                        if (updataItem.name.Contains("NsisoLauncher.exe"))
                        {
                            updata = true;
                        }
                        UpdataItem LocalConfig1 = LocalSelf[updataItem.name];
                        if (string.Equals(LocalConfig1.check, updataItem.check, StringComparison.OrdinalIgnoreCase))
                        {
                            LocalSelf.Remove(updataItem.name);
                        }
                        else
                        {
                            if (updata)
                            {
                                DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.UpdataConfig"),
                                    updataItem.url, PathManager.BaseStorageDirectory + @"\" + updataItem.filename));
                                LocalSelf.Remove(updataItem.name);
                                UpdataSelf = true;
                            }
                            else
                            {
                                try
                                {
                                    File.Delete(LocalConfig1.local);
                                }
                                catch
                                {

                                }
                                DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.UpdataConfig"),
                                    updataItem.url, PathManager.BaseStorageDirectory + @"\" + updataItem.filename));
                                LocalSelf.Remove(updataItem.name);
                            }
                        }
                    }
                    else
                    {
                        DownloadTask.Add(new DownloadTask(App.GetResourceString("String.Update.LostConfig"),
                            updataItem.url, PathManager.BaseStorageDirectory + @"\" + updataItem.filename));
                    }
                }
                if (LocalSelf.Count != 0)
                {
                    foreach (UpdataItem updataItem in LocalSelf.Values)
                    {
                        try
                        {
                            File.Delete(updataItem.local);
                        }
                        catch
                        {

                        }
                    }
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
