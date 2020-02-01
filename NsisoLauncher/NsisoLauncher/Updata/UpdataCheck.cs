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
        private updata_obj updata_obj;
        /// <summary>
        /// 资源检查
        /// </summary>
        public async Task<UpdataCheck> Check()
        {
            string Url = App.Config.MainConfig.Server.Updata_Check.Address;
            try
            {
                var res = await APIRequester.HttpGetAsync(Url);
                if (res.IsSuccessStatusCode)
                {
                    JObject json = JObject.Parse(await res.Content.ReadAsStringAsync());
                    updata_obj = json.ToObject<updata_obj>();
                    if (string.IsNullOrWhiteSpace(App.Config.MainConfig.Server.Updata_Check.packname))
                        return this;
                    else if (App.Config.MainConfig.Server.Updata_Check.Vision != updata_obj.Version)
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

        public async Task<List<DownloadTask>> setupdata(OtherCheck pack)
        {
            List<DownloadTask> task = new List<DownloadTask>();
            if (updata_obj.mods.Count != 0)
            {
                try
                {
                    var local_mods = await new ModCheck().ReadModInfo(App.Handler.GameRootPath);
                    foreach (KeyValuePair<string, updata_item> th in updata_obj.mods)
                    {
                        if (th.Value.function == "delete")
                        {
                            FileInfo file = new FileInfo(App.Handler.GameRootPath + @"\mods\" + th.Value.filename);
                            if (file.Exists)
                            {
                                file.Delete();
                            }
                        }
                        else if (local_mods.ContainsKey(th.Key))
                        {
                            updata_item lo = local_mods[th.Value.name]; 
                            if (!string.Equals(lo.check, th.Value.check, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Delete(lo.local);
                                task.Add(new DownloadTask("更新Mod", th.Value.url, App.Handler.GameRootPath + @"\mods\" + th.Value.filename));
                            }
                        }
                        else
                        {
                            task.Add(new DownloadTask("缺失Mod", th.Value.url, App.Handler.GameRootPath + @"\mods\" + th.Value.filename));
                        }
                    }
                }
                catch (Exception e)
                {
                    App.LogHandler.AppendFatal(e);
                }
            }
            if (updata_obj.scripts.Count != 0)
            {
                var local_scripts = await new ScriptsCheck().ReadscriptsInfo(App.Handler.GameRootPath);
                foreach (updata_item th in updata_obj.scripts)
                { 
                    if(local_scripts.ContainsKey(th.name))
                    {
                        updata_item lo = local_scripts[th.name];
                        if (string.Equals(lo.check, th.check, StringComparison.OrdinalIgnoreCase))
                        {
                            local_scripts.Remove(th.name);
                        }
                        else
                        {
                            File.Delete(lo.local);
                            task.Add(new DownloadTask("更新魔改", th.url, App.Handler.GameRootPath + @"\scripts\" + th.filename));
                            local_scripts.Remove(th.name);
                        }
                    }
                    else
                    {
                        task.Add(new DownloadTask("缺失魔改", th.url, App.Handler.GameRootPath + @"\scripts\" + th.filename));
                    }
                }
                if(local_scripts.Count!=0)
                {
                    foreach (updata_item lo in local_scripts.Values)
                    {
                        File.Delete(lo.local);
                    }
                }
            }
            if (updata_obj.resourcepacks.Count != 0)
            {
                var local_resourcepacks = await new ResourcepacksCheck().ReadresourcepacksInfo(App.Handler.GameRootPath);
                foreach (updata_item th in updata_obj.resourcepacks)
                {
                    if (local_resourcepacks.ContainsKey(th.name))
                    {
                        updata_item lo = local_resourcepacks[th.name];
                        if (string.Equals(lo.check, th.check, StringComparison.OrdinalIgnoreCase))
                        {
                            local_resourcepacks.Remove(th.name);
                        }
                        else
                        {
                            File.Delete(lo.local);
                            task.Add(new DownloadTask("更新材质", th.url, App.Handler.GameRootPath + @"\resourcepacks\" + th.filename));
                            updata_obj.scripts.Remove(th);
                            local_resourcepacks.Remove(th.name);
                        }
                    }
                    else
                    {
                        task.Add(new DownloadTask("缺失材质", th.url, App.Handler.GameRootPath + @"\resourcepacks\" + th.filename));
                    }
                }
            }
            if (updata_obj.config.Count != 0)
            {
                foreach (updata_item th in updata_obj.config)
                {
                    pack.pack_list.Add(th.filename);
                    task.Add(new DownloadTask("更新配置", th.url, App.Handler.GameRootPath + @"\" + th.filename));
                }
            }

            return task;
        }
        public string getvision()
        {
            return updata_obj == null ? "0.0.0" : updata_obj.Version;
        }
        public string getpackname()
        {
            return updata_obj == null ? "modpack" : updata_obj.packname;
        }
    }
}
