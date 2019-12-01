using Newtonsoft.Json.Linq;
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
        /// 本地mod信息
        /// </summary>
        private Dictionary<string, updata_mod> local_mods;
        /// <summary>
        /// 更新信息
        /// </summary>
        private updata_obj updata_obj;
        /// <summary>
        /// mod检查
        /// </summary>
        public async Task<List<DownloadTask>> updata()
        {
            mod_check();
            List<DownloadTask> task = new List<DownloadTask>();
            string Url = "http://www.coloryr.xyz/server/server.json";
            try
            {
                var res = await APIRequester.HttpGetAsync(Url);
                if (res.IsSuccessStatusCode)
                {
                    JObject json = JObject.Parse(await res.Content.ReadAsStringAsync());
                    updata_obj = json.ToObject<updata_obj>();
                    foreach (updata_mod th in updata_obj.mods)
                    {
                        if (local_mods.ContainsKey(th.name))
                        {
                            updata_mod lo = local_mods[th.name];
                            if (string.Equals(lo.check, th.check, StringComparison.OrdinalIgnoreCase))
                            {
                                updata_obj.mods.Remove(th);
                            }
                            else
                            {
                                File.Delete(lo.local);
                                task.Add(new DownloadTask("更新Mod", th.url, App.handler.GameRootPath + @"\mods"));
                                updata_obj.mods.Remove(th);
                            }
                        }
                        else
                        {
                            task.Add(new DownloadTask("缺失Mod", th.url, App.handler.GameRootPath + @"\mods"));
                        }
                    }
                }
            }
            catch
            {

            }
            return task;
        }
        /// <summary>
        /// mod检查任务
        /// </summary>
        private void mod_check()
        {
            mod_md5 mod = new mod_md5();
            local_mods = mod.ReadModInfo(App.handler.GameRootPath);
        }
    }
}
