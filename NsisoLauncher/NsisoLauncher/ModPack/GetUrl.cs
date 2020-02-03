using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NsisoLauncher.ModPack
{
    class GetUrlRes
    {
        public string url { get; set; }
        public string filename { get; set; }
    }
    class UrlResObj
    {
        public long id { get; set; }
        public string displayName { get; set; }
        public string fileName { get; set; }
        public string fileDate { get; set; }
        public long fileLength { get; set; }
        public int releaseType { get; set; }
        public int fileStatus { get; set; }
        public string downloadUrl { get; set; }
        public bool isAlternate { get; set; }
        public long alternateFileId { get; set; }
        public List<dependencieObj> dependencies { get; set; }
        public bool isAvailable { get; set; }
        public List<moduleObj> modules { get; set; }
        public long packageFingerprint { get; set; }
        public List<string> gameVersion { get; set; }
        public string installMetadata { get; set; }
        public string serverPackFileId { get; set; }
        public bool hasInstallScript { get; set; }
        public string gameVersionDateReleased { get; set; }
        public string gameVersionFlavor { get; set; }
    }
    class dependencieObj
    {
        public long addonId { get; set; }
        public int type { get; set; }
    }
    class moduleObj
    {
        public string foldername { get; set; }
        public long fingerprint { get; set; }
    }
    class GetUrl
    {
        ProgressDialogController window;
        public GetUrl(ProgressDialogController window)
        {
            this.window = window;
            window.Maximum = 1;
            window.Minimum = 0;
        }

        private string base_url = @"https://addons-ecs.forgesvc.net/api/v2/addon/{0}/file/{1}";
        public async Task<List<GetUrlRes>> get_urlAsync(List<FilesItem> mods)
        {

            List<GetUrlRes> list = new List<GetUrlRes>();

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);
            foreach (var item in mods)
            {
                for (int a = 0; a < 5; a++)
                {
                    try
                    {
                        double pro = (double)list.Count / (double)mods.Count;
                        window.SetMessage("当前进度：" + list.Count + "/" + mods.Count);
                        window.SetProgress(pro);
                        string res = await client.GetStringAsync(string.Format(base_url, item.projectID, item.fileID));
                        var obj1 = JObject.Parse(res).ToObject<UrlResObj>();
                        list.Add(new GetUrlRes
                        {
                            url = obj1.downloadUrl,
                            filename = obj1.fileName
                        });
                        a = 5;
                        continue;
                    }
                    catch
                    {
                        client.CancelPendingRequests();
                        a++;
                        if (a == 5)
                            return null;
                    }
                }
            }
            return list;
        }
    }
}
