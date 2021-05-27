﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.PhalAPI
{
    public class APIHandler
    {
        const string APIUrl = "http://hn2.api.okayapi.com/";
        const string App_key = "7B27B7B6A3C10158C28E3DE0B13785CD";

        public bool NoTracking { get; set; }

        public APIHandler(bool isNoTracking)
        {
            NoTracking = isNoTracking;
        }

        public async Task<NsisoLauncherVersionResponse> GetLatestLauncherVersion()
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("app_key", App_key);
                //表模型
                args.Add("model_name", "VersionList");
                //order
                args.Add("order", "[\"id DESC\"]");
                //查询规则（ID>0）
                args.Add("where", "[[\"id\", \">\", \"0\"]]");
                //仅返回一条（即ID最高的最新版本）
                args.Add("perpage", "1");
                string result = await HttpRequesterAPI.HttpPostReadAsStringForString(APIUrl + "?s=App.Table.FreeQuery", args);
                PhalApiClientResponse desObj = JsonConvert.DeserializeObject<PhalApiClientResponse>(result);
                JObject listJobj = desObj.Data;
                NsisoLauncherVersionListResponse list = listJobj.ToObject<NsisoLauncherVersionListResponse>();
                return list.List.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 异步报告日志
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="log">日志内容</param>
        /// <returns></returns>
        public async Task PostLogAsync(Modules.LogLevel level, string log)
        {
            var escapeLog = Uri.EscapeDataString(log);
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("app_key", App_key);
            args.Add("super_type", level.ToString());
            args.Add("super_message", log);
            var result = await HttpRequesterAPI.HttpPostReadAsStringForString(APIUrl + "?s=App.Market_SuperLogger.Record", args);
            Console.WriteLine(result);
        }

        /// <summary>
        /// 刷新使用次数计数器（统计）
        /// </summary>
        /// <returns></returns>
        public async Task RefreshUsingTimesCounter()
        {
            if (!NoTracking)
            {
                try
                {
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args.Add("app_key", App_key);
                    args.Add("type", "forever");
                    args.Add("name", "NsisoLauncher(Color_yr edit)UsingTimes");
                    args.Add("value", "1");
                    await HttpRequesterAPI.HttpPostAsync(APIUrl + "?s=App.Main_Counter.SmartRefresh", args);
                }
                catch
                { }
            }
        }
    }
}
