﻿using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Color_yr.updata
{
    class updata_check
    {
        /// <summary>
        /// 更新信息
        /// </summary>
        private updata_obj updata_obj;
        /// <summary>
        /// 资源检查
        /// </summary>
        public async Task<updata_check> check()
        {
            string Url = App.config.MainConfig.Server.Mods_Check.Address;
            try
            {
                var res = await APIRequester.HttpGetAsync(Url);
                if (res.IsSuccessStatusCode)
                {
                    JObject json = JObject.Parse(await res.Content.ReadAsStringAsync());
                    updata_obj = json.ToObject<updata_obj>();
                    if (string.IsNullOrWhiteSpace(App.config.MainConfig.Server.Mods_Check.packname))
                        return this;
                    else if (App.config.MainConfig.Server.Mods_Check.Vision != updata_obj.Vision)
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

        public async Task<List<DownloadTask>> setupdata(updata_pack pack)
        {
            List<DownloadTask> task = new List<DownloadTask>();
            if (updata_obj.mods.Count != 0)
            {
                var local_mods = await new mod_check().ReadModInfo(App.handler.GameRootPath);
                foreach (KeyValuePair<string, updata_item> th in updata_obj.mods)
                {
                    if (th.Value.function == "delete")
                    {
                        FileInfo file = new FileInfo(App.handler.GameRootPath + @"\mods\" + th.Value.filename);
                        if (file.Exists)
                        {
                            local_mods.Remove(th.Key);
                            file.Delete();
                        }
                    }
                    else if (local_mods.ContainsKey(th.Key))
                    {
                        updata_item lo = local_mods[th.Value.name];
                        if (string.Equals(lo.check, th.Value.check, StringComparison.OrdinalIgnoreCase))
                        {
                            local_mods.Remove(th.Key);
                        }
                        else
                        {
                            File.Delete(lo.local);
                            task.Add(new DownloadTask("更新Mod", th.Value.url, App.handler.GameRootPath + @"\mods\" + th.Value.filename));
                            local_mods.Remove(th.Key);
                        }
                    }
                    else
                    {
                        task.Add(new DownloadTask("缺失Mod", th.Value.url, App.handler.GameRootPath + @"\mods\" + th.Value.filename));
                    }
                }
            }
            if (updata_obj.scripts.Count != 0)
            {
                var local_scripts = await new scripts_check().ReadscriptsInfo(App.handler.GameRootPath);
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
                            task.Add(new DownloadTask("更新魔改", th.url, App.handler.GameRootPath + @"\scripts\" + th.filename));
                            local_scripts.Remove(th.name);
                        }
                    }
                    else
                    {
                        task.Add(new DownloadTask("缺失魔改", th.url, App.handler.GameRootPath + @"\scripts\" + th.filename));
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
                var local_resourcepacks = await new resourcepacks_check().ReadresourcepacksInfo(App.handler.GameRootPath);
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
                            task.Add(new DownloadTask("更新材质", th.url, App.handler.GameRootPath + @"\resourcepacks\" + th.filename));
                            updata_obj.scripts.Remove(th);
                            local_resourcepacks.Remove(th.name);
                        }
                    }
                    else
                    {
                        task.Add(new DownloadTask("缺失材质", th.url, App.handler.GameRootPath + @"\resourcepacks\" + th.filename));
                    }
                }
            }
            if (updata_obj.config.Count != 0)
            {
                foreach (updata_item th in updata_obj.config)
                {
                    pack.pack_list.Add(th.filename);
                    task.Add(new DownloadTask("更新配置", th.url, App.handler.GameRootPath + @"\" + th.filename));
                }
            }

            return task;
        }
        public string getvision()
        {
            return updata_obj == null ? "0.0.0" : updata_obj.Vision;
        }
        public string getpackname()
        {
            return updata_obj == null ? "modpack" : updata_obj.packname;
        }
    }
}
